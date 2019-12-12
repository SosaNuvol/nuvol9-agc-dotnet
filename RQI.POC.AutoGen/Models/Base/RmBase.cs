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
    using System.Data;
    using System.Data.SqlClient;
    using Microsoft.Practices.Unity;

    public class RmBase
    {

        #region Properties
        private string _classType;
        protected readonly string sqlConnectionString;
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
	AND (t.TABLE_SCHEMA IN (@SCHEMA_LIST@))
ORDER BY
	T.TABLE_SCHEMA,
	T.TABLE_NAME
";

        protected string sqlStatement;

        #endregion Properties

        public RmBase([Dependency("SqlConnectionString")]string sqlConnectionString)
        {
            this.sqlConnectionString = sqlConnectionString;
        }

        #region Methods

        protected void InitInstance(string schemaListString, string classType)
        {
            // ** Bind Properties
            _classType = classType;

            // ** Return result
            sqlStatement = SQL_STATEMENT.Replace("@SCHEMA_LIST@", schemaListString);
            sqlStatement = sqlStatement.Replace("@CLASS_TYPE@", _classType);
        }

        protected IDataReader GetReader(string sql)
        {
            var conn = new SqlConnection(sqlConnectionString);
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