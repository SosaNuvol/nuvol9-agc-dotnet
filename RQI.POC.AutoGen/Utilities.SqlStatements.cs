#region File Header
// <copyright file="Utilities.SqlStatements.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
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
		C.NUMERIC_PRECISION AS NumericPrecision,
		C.NUMERIC_SCALE AS NumericScale,
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
		AND (PK.TABLE_SCHEMA = PT.TABLE_SCHEMA)
ORDER BY
	ConstraintName
	, [Owner]
	, ThisTable";

        private const string _VIEWS_COL_SQL_STATEMENT = @"
SELECT
	  T.TABLE_CATALOG AS [VIEW_CATALOG]
	, T.TABLE_SCHEMA AS [VIEW_SCHEMA]
	, T.TABLE_NAME AS [VIEW_NAME]
	, COL.COLUMN_NAME
	, COL.IS_NULLABLE
	, COL.DATA_TYPE
	, COL.CHARACTER_MAXIMUM_LENGTH
	, COL.NUMERIC_PRECISION
	, COL.NUMERIC_SCALE
	, COL.COLUMN_DEFAULT
	, COL.ORDINAL_POSITION
FROM
    INFORMATION_SCHEMA.TABLES AS T WITH(NOLOCK)
	INNER JOIN INFORMATION_SCHEMA.COLUMNS AS COL WITH (NOLOCK)
	ON
		(COL.TABLE_SCHEMA = T.TABLE_SCHEMA)
		AND (COL.TABLE_NAME = T.TABLE_NAME)
WHERE
	(T.TABLE_TYPE='VIEW')
@SCHEMA_CLAUSE@
---	AND (T.TABLE_SCHEMA IN ('Docs','rules','Oracle'))
ORDER BY
	T.TABLE_SCHEMA,
	T.TABLE_NAME,
	COL.ORDINAL_POSITION

";

        private const string _VIEWS_SCHEMA_CLAUSE = @"	AND (T.TABLE_SCHEMA IN (@VIEW_SCHEMA_LIST@))
";

        private const string _UDTT_DEF_SQL_STATEMENT = @"
SELECT
      SCHEMA_NAME(TT.schema_id) AS TableSchema
    , TT.name AS TableName
    , DB_NAME() AS [Database]
    , SCHEMA_NAME(TT.schema_id) AS [Owner]
    , C.name AS [ColumnName]
    , C.column_id AS [OrdinalPosition]
    , NULL AS [DefaultSetting]
    , C.is_nullable AS [IsNullable]
    , TYPE_NAME(C.user_type_id) AS DataType
    , C.max_length AS [MaxLength]
    , C.precision AS [Precision]
    , C.scale AS [Scale]
    , CAST(0 AS BIT) AS [IsIdentity]
    , CAST(0 AS BIT) AS [IsComputed]
	, CAST(0 AS BIT) AS [IsPK]
FROM
    sys.table_types AS TT WITH(NOLOCK)
    INNER JOIN sys.columns AS C WITH(NOLOCK)
    ON
        (TT.type_table_object_id = C.object_id)
WHERE
    (TT.is_user_defined = 1)
    AND (TT.is_table_type = 1)
    AND (SCHEMA_NAME(TT.schema_id) IN (@SCHEMA_LIST@))
ORDER BY
    SCHEMA_NAME(TT.schema_id),
    TT.name,
    C.column_id;
";

        public const string SPROC_RESULT_SET_SQL_STATEMENT = @"
SELECT
	  SPR.[name] AS [ColName]
	, TP.[name] AS [DataTypeName]
	, SPR.max_length AS [MaxLength]
	, SPR.[precision] AS [Precision]
	, SPR.scale AS [Scale]
	, SPR.column_ordinal AS [ColumnOrdinal]
	, SPR.is_nullable AS [IsNullable]
FROM
	sys.dm_exec_describe_first_result_set(N'EXEC @SPROC_FULLNAME@', NULL, 0) AS SPR
	INNER JOIN sys.types AS TP
	ON
		(TP.system_type_id = SPR.system_type_id)
ORDER BY
	SPR.column_ordinal";

        private static string GetTableColSqlStatement
        {
            get
            {
                // ** Init
                var result = _TABLE_COL_SQL_STATEMENT.Replace("@SCHEMA_LIST@", TableSchemaList);
                result = result.Replace("@SYSTEM_COL_TO_IGNORE@", SQLSystemColToIgnoreList);
                result = result.Replace("@COL_DATA_TYPES_TO_IGNORE@", SQLColDataTypeToIgnoreList);

                // ** Return result
                return result;
            }
        }

        private static string GetViewsColSqlStatement
        {
            get
            {
                // ** Init
                var result = _VIEWS_COL_SQL_STATEMENT;

                // ** Check that there is a Schema list
                // ** Check if we are going to show all schemas
                if (!ViewsSchemaList.Equals("'*'"))
                {
                    // ** Build Schema clause
                    var schemaClause = _VIEWS_SCHEMA_CLAUSE.Replace("@VIEW_SCHEMA_LIST@", ViewsSchemaList);
                    result = result.Replace("@SCHEMA_CLAUSE@", schemaClause);
                }
                else
                {
                    // ** Erase the Schema clause from SQL Statement
                    result = result.Replace("@SCHEMA_CLAUSE@", string.Empty);
                }

                // ** Return result;
                return result;
            }
        }

        private static string GetUDTableTypeDefSqlStatement
        {
            get
            {
                // ** Init
                var result = _UDTT_DEF_SQL_STATEMENT.Replace("@SCHEMA_LIST@", UserDefinedTypesSchemaList);
                result = result.Replace("@SYSTEM_COL_TO_IGNORE@", SQLSystemColToIgnoreList);
                result = result.Replace("@COL_DATA_TYPES_TO_IGNORE@", SQLColDataTypeToIgnoreList);
                // ** Return result
                return result;
            }
        }

        #endregion SQL String Constants

        #region Database Artifacts

        public static IDataReader GetReader(string sql)
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
                        var col = new Column
                        {
                            Name = (string)rdr["ColumnName"]
                        };
                        col.CleanName = TransformName(col.Name);
                        col.DataType = rdr["DataType"].ToString();
                        col.SysType = GetSysType(col.DataType);
                        col.DbType = GetDbType(col.DataType);
                        col.AutoIncrement = Convert.ToBoolean(rdr["IsIdentity"]);
                        object obj = rdr["DefaultSetting"];
                        col.DefaultSetting = Convert.IsDBNull(obj) ? "" : (string)obj;
                        col.IsReadOnly = Convert.ToBoolean(rdr["IsComputed"]);
                        col.IsNullable = rdr["IsNullable"].ToString().Equals("YES", StringComparison.OrdinalIgnoreCase)
                            || rdr["IsNullable"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase);
                        col.TableName = tableName;
                        col.TableSchema = tableSchema;
                        // ReSharper disable InlineOutVariableDeclaration
                        if (int.TryParse(rdr["MaxLength"].ToString(), out int maxLength))
                        {
                            col.MaxLength = maxLength;
                        }
                        if (int.TryParse(rdr["NumericPrecision"].ToString(), out int numericPrecision))
                        {
                            col.NumericPrecision = numericPrecision;
                        }
                        if (int.TryParse(rdr["NumericScale"].ToString(), out int numericScale))
                        {
                            col.NumericScale = numericScale;
                        }
                        // ReSharper restore InlineOutVariableDeclaration
                        if (bool.TryParse(rdr["IsPK"].ToString(), out bool isPK))
                        {
                            col.IsPK = isPK;
                        }
                        col.IsHistorical = IsHistoricalColumn(col);
                        col.IsAuditColumn = IsAuditColumn(col);

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

        private static Dictionary<string, List<Column>> _viewsColumnDict;

        public static Dictionary<string, List<Column>> ViewsColumnDict
        {
            get
            {
                // ** Check if this has been generated
                if (_viewsColumnDict != null) return _viewsColumnDict;

                // ** Init
                _viewsColumnDict = new Dictionary<string, List<Column>>();
                var currentViewName = "";
                var currentViewSchema = "";
                List<Column> list = null;

                // ** Loop through columns
                using (IDataReader rdr = GetReader(GetViewsColSqlStatement))
                {
                    // ** Loop through column tuples
                    while (rdr.Read())
                    {
                        // ** Init
                        var viewName = (string)rdr["VIEW_NAME"];
                        var viewSchemaName = (string)rdr["VIEW_SCHEMA"];

                        // ** Check we are looking at the right table
                        if (!currentViewName.Equals(viewName) || !currentViewSchema.Equals(viewSchemaName))
                        {
                            // ** Reset mechanism for the loop
                            currentViewName = viewName;
                            currentViewSchema = viewSchemaName;
                            list = new List<Column>();
                            _viewsColumnDict.Add(viewSchemaName + "." + viewName, list);
                        }

                        // ** Build Column Info
                        var col = new Column
                        {
                            Name = (string)rdr["COLUMN_NAME"]
                        };
                        col.CleanName = TransformName(col.Name);
                        col.DataType = rdr["DATA_TYPE"].ToString();
                        col.SysType = GetSysType(col.DataType);
                        col.DbType = GetDbType(col.DataType);
                        col.AutoIncrement = false;
                        object obj = rdr["COLUMN_DEFAULT"];
                        col.DefaultSetting = Convert.IsDBNull(obj) ? "" : (string)obj;
                        col.IsReadOnly = true;
                        col.IsNullable = rdr["IS_NULLABLE"].ToString().Equals("YES", StringComparison.OrdinalIgnoreCase)
                            || rdr["IS_NULLABLE"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase);
                        col.TableName = viewName;
                        col.TableSchema = viewSchemaName;
                        col.IsPK = false;

                        if (int.TryParse(rdr["CHARACTER_MAXIMUM_LENGTH"].ToString(), out var maxLength))
                        {
                            col.MaxLength = maxLength;
                        }
                        if (int.TryParse(rdr["NUMERIC_PRECISION"].ToString(), out var numericPrecision))
                        {
                            col.NumericPrecision = numericPrecision;
                        }
                        if (int.TryParse(rdr["NUMERIC_SCALE"].ToString(), out var numericScale))
                        {
                            col.NumericScale = numericScale;
                        }

                        // ** Check if the column list needs to be reset.
                        if (list == null) throw new Exception("Column list can not be null.");

                        // ** Add column
                        list.Add(col);
                    }
                }

                // ** Return result
                return _viewsColumnDict;
            }
        }

        public static Dictionary<string, List<Column>> _udttColumnDict;

        public static Dictionary<string, List<Column>> UdttColumnDict
        {
            get
            {
                // ** Check if initialized
                if (_udttColumnDict != null) return _udttColumnDict;

                // ** Init
                _udttColumnDict = new Dictionary<string, List<Column>>();
                var udTableTypeName = "";
                var udTableTypeSchema = "";
                List<Column> list = null;

                // ** Loop through columns
                using (IDataReader rdr = GetReader(GetUDTableTypeDefSqlStatement))
                {
                    // ** Loop through column tuples
                    while (rdr.Read())
                    {
                        // ** Init
                        var tableName = (string)rdr["TableName"];
                        var tableSchema = (string)rdr["Owner"];

                        // ** Check we are looking at the right table
                        if (!udTableTypeName.Equals(tableName) || !udTableTypeSchema.Equals(tableSchema))
                        {
                            // ** Reset mechanism for the loop
                            udTableTypeName = tableName;
                            udTableTypeSchema = tableSchema;
                            list = new List<Column>();
                            _udttColumnDict.Add($"{tableSchema.Trim().ToLowerInvariant()}.{tableName.Trim().ToLowerInvariant()}", list);
                        }

                        // ** Build Column Info
                        var col = new Column
                        {
                            Name = (string)rdr["ColumnName"]
                        };
                        col.CleanName = TransformName(col.Name);
                        col.DataType = rdr["DataType"].ToString();
                        col.SysType = GetSysType(col.DataType);
                        col.DbType = GetDbType(col.DataType);
                        col.AutoIncrement = Convert.ToBoolean(rdr["IsIdentity"]);
                        object obj = rdr["DefaultSetting"];
                        col.DefaultSetting = Convert.IsDBNull(obj) ? "" : (string)obj;
                        col.IsReadOnly = Convert.ToBoolean(rdr["IsComputed"]);
                        col.IsNullable = rdr["IsNullable"].ToString().Equals("YES", StringComparison.OrdinalIgnoreCase)
                            || rdr["IsNullable"].ToString().Equals("True", StringComparison.OrdinalIgnoreCase);
                        col.TableName = tableName;
                        col.TableSchema = tableSchema;
                        // ReSharper disable InlineOutVariableDeclaration
                        if (int.TryParse(rdr["MaxLength"].ToString(), out int maxLength))
                        {
                            col.MaxLength = maxLength;
                        }
                        // ReSharper restore InlineOutVariableDeclaration
                        if (bool.TryParse(rdr["IsPK"].ToString(), out bool isPK))
                        {
                            col.IsPK = isPK;
                        }
                        col.IsHistorical = IsHistoricalColumn(col);
                        col.IsAuditColumn = IsAuditColumn(col);

                        // ** Check if the column list needs to be reset.
                        if (list == null) throw new Exception("Column list can not be null.");

                        // ** Add column
                        list.Add(col);
                    }
                }

                // ** Default response
                return _udttColumnDict;
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
                var sql = _FOREIGN_KEY_COLLECTIONS_STATEMENT.Replace("@SCHEMA_LIST@", TableSchemaList);

                using (IDataReader rdr = GetReader(sql))
                {
                    // ** Build the dataSet.
                    while (rdr.Read())
                    {
                        // ReSharper disable once UseObjectOrCollectionInitializer
                        var row = new ForeignKeyRow
                        {
                            ThisTable = (string)rdr["ThisTable"],
                            ThisColumn = (string)rdr["ThisColumn"],
                            ThisTableSchema = (string)rdr["ThisTableSchema"],

                            OtherTableSchema = (string)rdr["OtherTableSchema"],
                            OtherTable = (string)rdr["OtherTable"],
                            OtherColumn = (string)rdr["OtherColumn"],

                            ConstraintName = (string)rdr["ConstraintName"],
                            Owner = (string)rdr["Owner"]
                        };

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
                var fk = new FKTable
                {
                    ThisSchema = row.ThisTableSchema,
                    ThisTable = row.ThisTable,
                    ThisColumn = row.ThisColumn,

                    OtherSchema = row.OtherTableSchema,
                    OtherTable = row.OtherTable,
                    OtherColumn = row.OtherColumn
                };

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
                var fk = new FKTable
                {
                    ThisTable = row.OtherTable,
                    ThisColumn = row.OtherColumn,
                    ThisSchema = row.OtherTableSchema,

                    OtherTable = row.ThisTable,
                    OtherColumn = row.ThisColumn,
                    OtherSchema = row.ThisTableSchema
                };

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
                if (string.Compare(row.ThisTable, tableName, StringComparison.InvariantCultureIgnoreCase) == 0
                    && string.Compare(row.ThisTableSchema, tableSchema, StringComparison.InvariantCultureIgnoreCase) == 0)
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

        private const string _ENUM_TABLE_SQL_FORMAT = "SELECT * FROM {0}.{1} ORDER BY 1";
        public static EnumTable GetEnumTable(string schemaName, string tableName)
        {
            // ** Init
            EnumTable result = new EnumTable
            {
                Values = new Dictionary<string, int>()
            };

            using (IDataReader rdr = GetReader(string.Format(_ENUM_TABLE_SQL_FORMAT, schemaName, tableName)))
            {
                // ** Init
                string enumName = rdr.GetName(0);
                result.IDName = enumName;
                if (enumName.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase))
                {
                    enumName = enumName.Substring(0, enumName.Length - 2);
                }
                result.EnumName = enumName;

                string baseClassName;
                Type fType = rdr.GetFieldType(0);
                if (fType == typeof(byte))
                {
                    baseClassName = "byte";
                }
                else if (fType == typeof(int))
                {
                    baseClassName = "int";
                }
                else if (fType == typeof(short))
                {
                    baseClassName = "short";
                }
                else if (fType == typeof(long))
                {
                    baseClassName = "long";
                }
                else
                {
                    // ReSharper disable once UseStringInterpolation
                    throw new NotSupportedException(string.Format("Enum type for {0}.{1} is {2} but must be byte, int, short or long", schemaName, tableName, fType.Name));
                }
                result.BaseClassName = baseClassName;

                while (rdr.Read())
                {
                    // ** Init
                    int id = Convert.ToInt32(rdr[0]);
                    string name = (string)rdr[1];

                    name = TransformEnumName(name);

                    result.Values.Add(name, id);
                }
            }

            // ** Return result
            return result;
        }

        public static MetaDataTable GetMetaDataTable(string schemaName, string szTableName)
        {
            // Locals
            var oResult = new MetaDataTable
            {
                Values = new Dictionary<string, string>()
            };

            // Loop through the result
            using (IDataReader rdr = GetReader(string.Format(_ENUM_TABLE_SQL_FORMAT, szTableName, schemaName)))
            {
                string szMetaDataName = rdr.GetName(0);
                oResult.IDName = szMetaDataName;
                if (szMetaDataName.EndsWith("ID", StringComparison.InvariantCultureIgnoreCase))
                {
                    szMetaDataName = szMetaDataName.Substring(0, szMetaDataName.Length - 2);
                }
                oResult.MetaDataName = szMetaDataName;

                // Loop through the tables and build the list
                while (rdr.Read())
                {
                    // Print to console
                    System.Diagnostics.Debug.WriteLine("What up");

                    string szId = (string)rdr[0];
                    string szName = (string)rdr[1];
                    // TODO:  Andres you can comment this out so that you can just take what is in the database.
                    szName = TransformEnumName(szName);

                    // Add to list
                    oResult.Values.Add(szId, szName);
                }
            }

            // Return Result
            return oResult;
        }

        public static string TransformEnumName(string name)
        {
            // ** Init
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            // ** Transform
            var newName = name;
            newName = Inflector.ToPascalCase(newName, false);
            newName = IsStringNumeric(newName) ? "_" + newName : newName;
            newName = StripNonAlphaNumeric(newName);
            newName = newName.Trim();
            newName = KeyWordCheck(newName, String.Empty);

            // ** Return result
            return newName;
        }

        #endregion Database Artifacts
    }
}