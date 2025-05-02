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
        private const string _TABLE_TYPE = "TABLE TYPE";

        private string _classType;
        protected readonly string SQLConnectionString;
        protected const string SQL_STATEMENT = @"
SELECT
	  T.TABLE_CATALOG
	, T.TABLE_SCHEMA
	, T.TABLE_NAME
	, T.TABLE_TYPE
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
), WithParametersCTE AS (
	SELECT
		RT.*
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
)
SELECT * FROM WithParametersCTE";

        protected const string SQL_SCHEMA_CLAUSE_PF = @"		AND (R.ROUTINE_SCHEMA IN (@SCHEMA_LIST@))
";

        protected const string SQL_SCHEMA_CLAUSE_TYPE = @"";

        protected const string SQL_STATEMENT_TYPES = @"";

        protected string SQLStatement;

        public enum SQLTypesEnum
        {
            Tables,
            Views,
            Procedures,
            Functions,
            Types
        }

        #endregion Properties

        public RmBase([Dependency("SqlConnectionString")]string sqlConnectionString)
        {
            SQLConnectionString = sqlConnectionString;
        }

        #region Methods

        protected void InitInstance(string schemaListString, SQLTypesEnum classType)
        {
            // ** Init
            SetClassTypeAndSqlStatement(classType, out var classTypeArgument, out var sqlStatement, out var sqlClauseStatement);

            // ** Bind Properties
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
                case SQLTypesEnum.Types:
                    sqlStatement = SQL_STATEMENT_TYPES;
                    classTypeArgument = _TABLE_TYPE;
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