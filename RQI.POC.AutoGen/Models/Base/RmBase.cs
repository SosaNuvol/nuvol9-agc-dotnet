#region File Header
// <copyright file="RmBase.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-12-9:57 AM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Data;
    using System.Data.SqlClient;
    using Unity;

    public class RmBase
    {

        #region Properties
        private const string _BASE_TABLE = "BASE TABLE";
        private const string _VIEW = "VIEW";
        private const string _PROCEDURE = "PROCEDURE";
        private const string _FUNCTION = "FUNCTION";
        private const string _USER_TYPES = "USER TYPE";
        private const string _CRITICAL_DATA_TYPE_TABLE_NAME = "CriticalDataType";

        private string _classType;
        protected readonly string SQLConnectionString;
        protected const string SQL_STATEMENT = @"
SELECT
	  T.TABLE_CATALOG
	, T.TABLE_SCHEMA
	, T.TABLE_NAME
	, T.TABLE_TYPE
	, CAST(
		CASE WHEN COUNT(T.TABLE_NAME) OVER (PARTITION BY T.TABLE_NAME) > 1
			 THEN 1
			 ELSE 0
		END AS BIT
	  ) AS IsDuplicateName
FROM
    INFORMATION_SCHEMA.TABLES AS T WITH(NOLOCK)
WHERE
	(T.TABLE_TYPE='@CLASS_TYPE@')
@SCHEMA_CLAUSE@
ORDER BY
	T.TABLE_SCHEMA,
	T.TABLE_NAME
";

        protected const string SQL_SCHEMA_CLAUSE = @"	AND (t.TABLE_SCHEMA IN (@SCHEMA_LIST@))
";

        protected const string SQL_STATEMENT_SPROCS_FUNCTIONS = @"
WITH RootCTE AS (
	SELECT
		  OBJECT_ID(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME) AS [ObjectId]
		, R.ROUTINE_SCHEMA AS [Schema]
		, R.ROUTINE_NAME AS [Name]
		, OBJECT_DEFINITION(OBJECT_ID(R.ROUTINE_SCHEMA + '.' + R.ROUTINE_NAME)) AS [Definition]
	--	, * 
	FROM
		information_schema.routines AS R WITH(NOLOCK)
	WHERE
		(R.routine_type = '@CLASS_TYPE@')
@SCHEMA_CLAUSE@
), AddReturnTypesCTE AS (
	SELECT
		OBJECT_ID(SPT.[SchemaName] + '.' + SPT.[SPROCName]) AS ObjectId
		, SPT.[CSharpReturnType]
	FROM
		[GEN].[StoredProcedureReturnTypes] AS SPT WITH(NOLOCK)
), WithParametersCTE AS (
	SELECT
		RT.*
		, RETT.[CSharpReturnType]
		, (
SELECT
	--  SPECIFIC_SCHEMA AS SPSchema
	--, SPECIFIC_NAME AS SPName
	  PARAMETER_NAME AS [name]
	, REPLACE(PARAMETER_NAME, '@', '') AS [cleanName]
	--, PARAMETER_MODE AS [paramType]
	--, '' AS [sysType]
	, DATA_TYPE AS [dbType]
	, CHARACTER_MAXIMUM_LENGTH AS [dataLength]
	, CASE
		WHEN PARAMETER_MODE = 'OUT' THEN 2
		WHEN PARAMETER_MODE = 'INOUT' THEN 3
		WHEN PARAMETER_MODE = 'RETURN' THEN 6
		ELSE 1
	  END AS [mode]
	, NUMERIC_PRECISION as [numericPrecision]
	, NUMERIC_SCALE as [numericScale]
	, ORDINAL_POSITION AS [ordinalPosition]
	--, '' AS [DefaultValue]
	, IS_RESULT AS [isResult]
FROM INFORMATION_SCHEMA.PARAMETERS
WHERE
	(SPECIFIC_SCHEMA = RT.[Schema])
	AND (SPECIFIC_NAME = RT.[Name])
FOR JSON PATH
		) AS JsonParametersList
	FROM
		RootCTE AS RT WITH(NOLOCK)
	LEFT OUTER JOIN AddReturnTypesCTE AS RETT
	ON
		(RETT.ObjectId = RT.ObjectId)
)
SELECT * FROM WithParametersCTE";

        protected const string SQL_SCHEMA_CLAUSE_PF = @"		AND (R.ROUTINE_SCHEMA IN (@SCHEMA_LIST@))
";

        protected const string SQL_STATEMENT_TYPES = @"
WITH RootCTE AS (
    SELECT
          SCHEMA_NAME(T.schema_id) AS TYPE_SCHEMA
        , T.name AS TYPE_NAME
        , T.is_table_type AS IS_TABLE_TYPE
        , T.max_length AS MaxLength
        , T.precision AS Precision
        , T.scale AS Scale
        , T.is_nullable AS IsNullable
        , UPPER(ST.name) AS SYSTEM_TYPE -- Get the base system type name
    FROM
        sys.types AS T
    LEFT OUTER JOIN
        sys.types AS ST ON T.system_type_id = ST.user_type_id AND ST.is_user_defined = 0 -- Join to get the system type
    WHERE
        (T.is_user_defined = 1)@SCHEMA_CLAUSE@
)
SELECT
    *
FROM
    RootCTE
WHERE
    (IS_TABLE_TYPE = 0 OR IS_TABLE_TYPE = 1)
ORDER BY
    TYPE_SCHEMA,
    TYPE_NAME
";

        protected const string SQL_STATEMENT_CRITICAL_DATA_TYPES = @"
SELECT
      [@TABLE_NAME@ID] AS [Id]
    , [Prefix]
    , [@TABLE_NAME@Name] AS [Name]
    , [@TABLE_NAME@Description] AS [Description]
    , '@SCHEMA_NAME@' AS [Schema]
    , '@TABLE_NAME@s' AS [TableName]
FROM
    @TABLE_NAME_WITH_SCHEMA@
WHERE
  (IsDeleted = 0)
";
        protected const string SQL_SCHEMA_CLAUSE_TYPE = @"
    AND SCHEMA_NAME(T.schema_id) IN (@SCHEMA_LIST@)";

        protected string SQLStatement;

        public enum SQLTypesEnum
        {
            Tables,
            Views,
            Procedures,
            Functions,
            UserDefinedTypes,
            CriticalDataTypes
        }

        #endregion Properties

        public RmBase([Dependency("SqlConnectionString")] string sqlConnectionString)
        {
            SQLConnectionString = sqlConnectionString;
        }

        #region Methods

        protected void InitInstance(string schemaListString, SQLTypesEnum classType)
        {
            // ** Init
            SetClassTypeAndSqlStatement(classType, out var classTypeArgument, out var sqlStatement, out var sqlClauseStatement);

            // ** Bind Definitions
            _classType = classTypeArgument;

            // ** Build class type
            SQLStatement = sqlStatement.Replace("@CLASS_TYPE@", _classType);

            // ** Check if we are going to show all schemas
            if (!schemaListString.Equals("'*'"))
            {
                // ** Build Schema clause
                SQLStatement = SQLStatement.Replace("@SCHEMA_CLAUSE@", sqlClauseStatement);
                SQLStatement = SQLStatement.Replace("@SCHEMA_LIST@", schemaListString);
            }
            else
            {
                // ** Erase the Schema clause from SQL Statement
                SQLStatement = SQLStatement.Replace("@SCHEMA_CLAUSE@", string.Empty);
            }
        }

        protected IDataReader GetReader(string sql)
        {
            var conn = new SqlConnection(SQLConnectionString);
            var cmd = new SqlCommand(sql, conn);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }
        /*
                private SqlCommand GetCommand(string sql)
                {
                    SqlConnection conn = new SqlConnection(_sqlConnectionString);
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    conn.Open();
                    return cmd;
                }
        */

        private void SetClassTypeAndSqlStatement(SQLTypesEnum classTypeEnum, out string classTypeArgument, out string sqlStatement, out string sqlClauseStatement)
        {
            // ** Init
            switch (classTypeEnum)
            {
                case SQLTypesEnum.Tables:
                    sqlStatement = SQL_STATEMENT;
                    classTypeArgument = _BASE_TABLE;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE;
                    break;
                case SQLTypesEnum.Views:
                    sqlStatement = SQL_STATEMENT;
                    classTypeArgument = _VIEW;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE;
                    break;
                case SQLTypesEnum.Procedures:
                    sqlStatement = SQL_STATEMENT_SPROCS_FUNCTIONS;
                    classTypeArgument = _PROCEDURE;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE_PF;
                    break;
                case SQLTypesEnum.Functions:
                    sqlStatement = SQL_STATEMENT_SPROCS_FUNCTIONS;
                    classTypeArgument = _FUNCTION;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE_PF;
                    break;
                case SQLTypesEnum.UserDefinedTypes:
                    sqlStatement = SQL_STATEMENT_TYPES;
                    classTypeArgument = _USER_TYPES;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE_TYPE;
                    break;
                case SQLTypesEnum.CriticalDataTypes:
                    sqlStatement = SQL_STATEMENT_CRITICAL_DATA_TYPES;
                    classTypeArgument = _CRITICAL_DATA_TYPE_TABLE_NAME;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE_TYPE;
                    break;
                default:
                    sqlStatement = SQL_STATEMENT;
                    classTypeArgument = _BASE_TABLE;
                    sqlClauseStatement = SQL_SCHEMA_CLAUSE;
                    break;
            }
        }

        #endregion Methods
    }
}