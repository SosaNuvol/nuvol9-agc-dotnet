#region File Header
// <copyright file="Utilities.SqlStatements.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SqlClient;
	using System.Linq;
	using Models.Base;

	public static partial class Utilities
    {
        #region SQL String Constants

        private const string _TABLE_COL_SQL_STATEMENT = @"WITH PKColumnCTE AS (
	SELECT
		KCU.COLUMN_NAME AS ColumnName
		, TC.TABLE_NAME AS TableName
		, TC.TABLE_SCHEMA AS TableSchema
		, CAST(1 AS BIT) AS IsPK
	FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU
	JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
	ON
		KCU.CONSTRAINT_NAME=TC.CONSTRAINT_NAME
	WHERE
		(TC.CONSTRAINT_TYPE='PRIMARY KEY')
		AND (TC.TABLE_SCHEMA IN (@SCHEMA_LIST@))
)
, MainCte AS (
	SELECT DISTINCT
		C.TABLE_SCHEMA AS TableSchema,
		C.TABLE_NAME AS TableName, 
		C.TABLE_CATALOG AS [Database],
		C.TABLE_SCHEMA AS [Owner], 
		C.COLUMN_NAME AS ColumnName, 
		C.ORDINAL_POSITION AS OrdinalPosition, 
		C.COLUMN_DEFAULT AS DefaultSetting, 
		C.IS_NULLABLE AS IsNullable,
		C.DATA_TYPE AS DataType, 
		C.CHARACTER_MAXIMUM_LENGTH AS MaxLength, 
		C.DATETIME_PRECISION AS DatePrecision,
		COLUMNPROPERTY(object_id('[' + C.TABLE_SCHEMA + '].[' + C.TABLE_NAME + ']'), C.COLUMN_NAME, 'IsIdentity') AS IsIdentity,
		COLUMNPROPERTY(object_id('[' + C.TABLE_SCHEMA + '].[' + C.TABLE_NAME + ']'), C.COLUMN_NAME, 'IsComputed') AS IsComputed,
		PKColumn.IsPK
	FROM 
		INFORMATION_SCHEMA.COLUMNS AS C
		LEFT OUTER JOIN PKColumnCTE AS PKColumn
		ON
			(C.COLUMN_NAME = PKColumn.ColumnName)
			AND (C.TABLE_NAME = PKColumn.TableName)
			AND (C.TABLE_SCHEMA = PKColumn.TableSchema)
	WHERE
		(C.TABLE_SCHEMA IN (@SCHEMA_LIST@))
)
SELECT 
	*
FROM
	MainCte AS C
WHERE
		(C.ColumnName NOT IN (@SYSTEM_COL_TO_IGNORE@))
		AND (C.ColumnName NOT IN (SELECT name FROM sys.computed_columns))
		AND (C.DataType NOT IN (@COL_DATA_TYPES_TO_IGNORE@))
ORDER BY
	C.[Owner],
	C.TableName,
	C.OrdinalPosition ASC
;";

        private const string _FOREIGN_KEY_COLLECTIONS_STATEMENT = @"
SELECT
	FK.TABLE_NAME AS ThisTable
	, KCU.COLUMN_NAME AS ThisColumn
	, FK.TABLE_SCHEMA AS ThisTableSchema
	
	, PK.TABLE_NAME AS OtherTable
	, PT.COLUMN_NAME AS OtherColumn
	, PT.TABLE_SCHEMA AS OtherTableSchema

	, TC.CONSTRAINT_NAME AS ConstraintName
	, FK.TABLE_SCHEMA AS [Owner]
FROM 
	INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS TC
	INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS FK
	ON
		(TC.CONSTRAINT_NAME = FK.CONSTRAINT_NAME)
		AND (FK.TABLE_SCHEMA IN (@SCHEMA_LIST@))
	INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS PK
	ON
		(TC.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME)
	INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU
	ON
		(TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME)
	INNER JOIN
	(	
		SELECT
			TC.TABLE_SCHEMA
			, TC.TABLE_NAME
			, KCU.COLUMN_NAME
		FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
		INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU
		ON
			(TC.CONSTRAINT_NAME = KCU.CONSTRAINT_NAME)
		WHERE
			TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
	) AS PT
	ON
		(PK.TABLE_NAME = PT.TABLE_NAME)
ORDER BY
	ConstraintName
	, [Owner]
	, ThisTable";

        private static string GetTableColSqlStatement
        {
	        get
	        {
				// ** Init
				var result = _TABLE_COL_SQL_STATEMENT.Replace("@SCHEMA_LIST@", SchemaList);
				result = result.Replace("@SYSTEM_COL_TO_IGNORE@", SQLSystemColToIgnoreList);
				result = result.Replace("@COL_DATA_TYPES_TO_IGNORE@", SQLColDataTypeToIgnoreList);

				// ** Return result
				return result;
	        }
        }
		#endregion SQL String Constants

		#region Database Artifacts

		private static IDataReader GetReader(string sql)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			SqlCommand cmd = new SqlCommand(sql, conn);
			conn.Open();
			return cmd.ExecuteReader(CommandBehavior.CloseConnection);
		}

/*
		private static SqlCommand GetCommand(string sql)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			SqlCommand cmd = new SqlCommand(sql, conn);
			conn.Open();
			return cmd;
		}
*/

		private static Dictionary<string, List<Column>> _tableColumnDict;

		public static Dictionary<string, List<Column>> TableColumnDict
		{
			get
			{
				// ** Check if initialized
				if (_tableColumnDict != null) return _tableColumnDict;

				// ** Init
				_tableColumnDict = new Dictionary<string, List<Column>>();
				var currentTableName = "";
				var currentTableSchema = "";
				List<Column> list = null;

				// ** Loop through columns
				using (IDataReader rdr = GetReader(GetTableColSqlStatement))
				{
					// ** Loop through column tuples
					while (rdr.Read())
					{
						// ** Init
						var tableName = (string)rdr["TableName"];
						var tableSchema = (string)rdr["Owner"];

						// ** Check we are looking at the right table
						if (!currentTableName.Equals(tableName) || !currentTableSchema.Equals(tableSchema))
						{
							// ** Reset mechanism for the loop
							currentTableName = tableName;
							currentTableSchema = tableSchema;
							list = new List<Column>();
							_tableColumnDict.Add(tableSchema + "." + tableName, list);
						}

						// ** Build Column Info
						var col = new Column();
						col.Name = (string) rdr["ColumnName"];
						col.CleanName = TransformName(col.Name);
						col.DataType = rdr["DataType"].ToString();
						col.SysType = GetSysType(col.DataType);
						col.DbType = GetDbType(col.DataType);
						col.AutoIncrement = Convert.ToBoolean(rdr["IsIdentity"]);
						object obj = rdr["DefaultSetting"];
						col.DefaultSetting = Convert.IsDBNull(obj) ? "" : (string)obj;
						col.IsReadOnly = Convert.ToBoolean(rdr["IsComputed"]);
						col.IsNullable = rdr["IsNullable"].ToString() == "YES";
						col.TableName = tableName;
						col.TableSchema = tableSchema;
						// ReSharper disable InlineOutVariableDeclaration
						int maxLength;
						if (int.TryParse(rdr["MaxLength"].ToString(), out maxLength))
						{
							col.MaxLength = maxLength;
						}
						bool isPK;
						// ReSharper restore InlineOutVariableDeclaration
						if (bool.TryParse(rdr["IsPK"].ToString(), out isPK))
						{
							col.IsPK = isPK;
						}

						// ** Check if the column list needs to be reset.
						if (list == null) throw new Exception("Column list can not be null.");

						// ** Add column
						list.Add(col);
					}
				}

				// ** Default response
				return _tableColumnDict;
			}
		}


		private static ForeignKeyRowCollection _foreignKeyRows;
		public static ForeignKeyRowCollection ForeignKeyRows
		{
			get
			{
				// ** Check if this has been instantiated
				if (_foreignKeyRows != null) return _foreignKeyRows;

				// ** Load from DB
				_foreignKeyRows = new ForeignKeyRowCollection();

				// ** Init SQL Statement
				var sql = _FOREIGN_KEY_COLLECTIONS_STATEMENT.Replace("@SCHEMA_LIST@", SchemaList);

				using (IDataReader rdr = GetReader(sql))
				{
					// ** Build the dataSet.
					while (rdr.Read())
					{
						// ReSharper disable once UseObjectOrCollectionInitializer
						var row = new ForeignKeyRow();

						row.ThisTable = (string)rdr["ThisTable"];
						row.ThisColumn = (string)rdr["ThisColumn"];
						row.ThisTableSchema = (string) rdr["ThisTableSchema"];

						row.OtherTableSchema = (string)rdr["OtherTableSchema"];
						row.OtherTable = (string)rdr["OtherTable"];
						row.OtherColumn = (string)rdr["OtherColumn"];

						row.ConstraintName = (string)rdr["ConstraintName"];
						row.Owner = (string)rdr["Owner"];

						_foreignKeyRows.Add(row);
					}
				}

				// ** Return result
				return _foreignKeyRows;
			}
		}

		public static List<FKTable> LoadSingleFKTables(string tableName, string tableSchema)
		{
			// ** Init
			var results = new List<FKTable>();

			// ** Load collection
			var rowCollection = ForeignKeyRows.Where(a =>
				(string.Compare(a.ThisTable, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
				//&& (string.Compare(a.OtherTable, tableName, StringComparison.InvariantCultureIgnoreCase) != 0)//Don't match ThisTable-these are handled by many foreign keys
				&& (string.Compare(a.ThisTableSchema, tableSchema, StringComparison.InvariantCultureIgnoreCase) == 0)
			);

			// ** Build columns used
			var thisColumnUsed = new Dictionary<string, string>();
			foreach (ForeignKeyRow row in rowCollection)
			{
				// ** Check if already done.
				if (thisColumnUsed.ContainsKey(row.ThisColumn)) continue;

				// ** Add new FK information
				thisColumnUsed.Add(row.ThisColumn, row.ThisColumn);

				// ** Build FKTable information and save it
				// ReSharper disable once UseObjectOrCollectionInitializer
				var fk = new FKTable();

				fk.ThisSchema = row.ThisTableSchema;
				fk.ThisTable = row.ThisTable;
				fk.ThisColumn = row.ThisColumn;

				fk.OtherSchema = row.OtherTableSchema;
				fk.OtherTable = row.OtherTable;
				fk.OtherColumn = row.OtherColumn;

				fk.OtherClass = Inflector.MakeSingular(TransformName(fk.OtherTable));
				fk.OtherQueryable = Inflector.MakePlural(Inflector.MakeSingular(TransformName(fk.ThisTable)));

				//remove ID or add Value
				if (fk.ThisColumn.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase))
				{
					fk.PropName = fk.ThisColumn.Substring(0, fk.ThisColumn.Length - 2);
				}
				else
				{
					fk.PropName = fk.ThisColumn + "Value";
				}

				// ** Special case where a circular reference is found 
				if (string.Compare(fk.ThisTable, fk.OtherTable, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					fk.OtherQueryable = "Child" + fk.OtherQueryable;
				}

				fk.ConstraintName = row.ConstraintName;

				// ** Build result
				results.Add(fk);
			}

			// ** Return result
			return results;
		}

		public static List<FKTable> LoadManyFKTables(string tableName, string tableSchema)
		{
			var results = new List<FKTable>();

			var rowCollection = ForeignKeyRows.Where(a =>
				(string.Compare(a.OtherTable, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
				&& (string.Compare(a.OtherTableSchema, tableSchema, StringComparison.InvariantCultureIgnoreCase) == 0));

			var propNameCount = new Dictionary<string, int>();
			foreach (ForeignKeyRow row in rowCollection)
			{
				// ** Init 
				// ReSharper disable once UseObjectOrCollectionInitializer
				var fk = new FKTable();

				fk.ThisTable = row.OtherTable;
				fk.ThisColumn = row.OtherColumn;
				fk.ThisSchema = row.OtherTableSchema;

				fk.OtherTable = row.ThisTable;
				fk.OtherColumn = row.ThisColumn;
				fk.OtherSchema = row.ThisTableSchema;

				fk.OtherClass = Inflector.MakeSingular(TransformName(fk.OtherTable));
				fk.OtherQueryable = Inflector.MakePlural(fk.OtherClass);

				fk.PropName = fk.OtherQueryable;

				if (string.Compare(fk.ThisTable, fk.OtherTable, StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					fk.PropName = "Child" + fk.PropName;
				}

				//don't allow duplicate prop names
				if (!propNameCount.ContainsKey(fk.PropName))
				{
					propNameCount.Add(fk.PropName, 1);
				}
				else
				{
					propNameCount[fk.PropName]++;
					fk.PropName += propNameCount[fk.PropName].ToString().PadLeft(2, '0');
				}

				fk.ConstraintName = row.ConstraintName;

				// ** Build result
				results.Add(fk);
			}

			// ** Return result;
			return results;
		}

		public static List<FKTable> LoadFKTables(string tableName, string tableSchema)
		{
			// ** Init
			var results = new List<FKTable>();

			// ** Loop through and build list
			foreach (ForeignKeyRow row in ForeignKeyRows)
			{
				// TODO: ???? Not sure what this does
				// ** Check that thisTable is the same as OtherTable
				if (string.Compare(row.ThisTable, tableName, StringComparison.InvariantCultureIgnoreCase) == 0
				    || string.Compare(row.OtherTable, tableName, StringComparison.InvariantCultureIgnoreCase) == 0)
				{

					FKTable fk = new FKTable();

					if (tableName.ToLower() == row.ThisTable.ToLower())
					{

						fk.ThisSchema = row.ThisTableSchema;
						fk.ThisTable = row.ThisTable;
						fk.ThisColumn = row.ThisColumn;

						fk.OtherSchema = row.OtherTableSchema;
						fk.OtherTable = row.OtherTable;
						fk.OtherColumn = row.OtherColumn;
					}
					else
					{

						fk.ThisSchema = row.OtherTableSchema;
						fk.ThisTable = row.OtherTable;
						fk.ThisColumn = row.OtherColumn;

						fk.OtherSchema = row.ThisTableSchema;
						fk.OtherTable = row.ThisTable;
						fk.OtherColumn = row.ThisColumn;
					}

					fk.OtherClass = Inflector.MakeSingular(TransformName(fk.OtherTable));
					fk.OtherQueryable = Inflector.MakePlural(fk.OtherClass);

					fk.PropName = fk.OtherQueryable;

					fk.ConstraintName = row.ConstraintName;

					results.Add(fk);
				}
			}

			// ** Return results
			return results;
		}


		#endregion Database Artifacts
	}
}