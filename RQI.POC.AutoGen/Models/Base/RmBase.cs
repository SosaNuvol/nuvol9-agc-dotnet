#region File Header
// <copyright file="RmBase.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-12-9:57 AM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Practices.Unity;

    public class RmBase
    {

        #region Properties
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

        protected string SQLStatement;

        #endregion Properties

        public RmBase([Dependency("SqlConnectionString")]string sqlConnectionString)
        {
            SQLConnectionString = sqlConnectionString;
        }

        #region Methods

        protected void InitInstance(string schemaListString, string classType)
        {
            // ** Bind Properties
            _classType = classType;

            // ** Build class type
            SQLStatement = SQL_STATEMENT.Replace("@CLASS_TYPE@", _classType);

            // ** Check if we are going to show all schemas
            if (!schemaListString.Equals("'*'"))
            {
                // ** Build Schema clause
                SQLStatement = SQLStatement.Replace("@SCHEMA_CLAUSE@", SQL_SCHEMA_CLAUSE);
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

        #endregion Methods
    }
}