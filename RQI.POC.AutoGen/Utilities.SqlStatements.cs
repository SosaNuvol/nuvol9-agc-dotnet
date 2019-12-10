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

		private static SqlCommand GetCommand(string sql)
		{
			SqlConnection conn = new SqlConnection(ConnectionString);
			SqlCommand cmd = new SqlCommand(sql, conn);
			conn.Open();
			return cmd;
		}

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

		#endregion Database Artifacts
	}
}