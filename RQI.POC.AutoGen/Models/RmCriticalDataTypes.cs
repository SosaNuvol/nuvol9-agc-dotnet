#region File Header
// <copyright file="RmCriticalDataTypes.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2025-01-03
// </summary>
// <author>GitHub Copilot</author>
#endregion

namespace RQI.POC.AutoGen.Models
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    public class RmCriticalDataTypes : RmBase
    {
        #region Properties
        List<CriticalDataType> _criticalDataTypes;
        #endregion Properties

        public RmCriticalDataTypes(string sqlConnectionString) : base(sqlConnectionString)
        {
            // ** Init Class properties
            _criticalDataTypes = null;
            // ** Init
            InitInstance(Utilities.CriticalDataTypesSchemaList, SQLTypesEnum.CriticalDataTypes);
        }

        public List<CriticalDataType> LoadCriticalDataTypes()
        {
            // ** Check if initialized
            if (_criticalDataTypes != null) return _criticalDataTypes;
            // ** Init
            _criticalDataTypes = new List<CriticalDataType>();

            // ** Build the SQL Statement
            BuildSqlStatement();

            // ** Build Critical Data Types list
            using (IDataReader dataReader = GetReader(SQLStatement))
            {
                // ** Root loop through reader
                while (dataReader.Read())
                {
                    // ** Init
                    var criticalDataType = new CriticalDataType
                    {
                        Id = (int)dataReader["Id"],
                        Prefix = (string)dataReader["Prefix"],
                        Name = (string)dataReader["Name"],
                        Description = (string)dataReader["Description"],
                        Schema = (string)dataReader["Schema"],
                        TableName = (string)dataReader["TableName"],
                        TableNameSingular = Utilities.Inflector.MakeSingular((string)dataReader["TableName"])
                    };

                    // ** Add to list
                    _criticalDataTypes.Add(criticalDataType);
                }
            }

            return _criticalDataTypes;
        }

        private void BuildSqlStatement()
        {
            // ** Build schema clause
            var criticalDataTypesTableNamesList = new List<string>();
            if (Utilities.CriticalDataTypesTablesList != null && Utilities.CriticalDataTypesTablesList.Length > 0)
            {
                criticalDataTypesTableNamesList = Utilities.CriticalDataTypesTablesList.Split(',').ToList();
            }

            var sqlStatement = string.Empty;

            criticalDataTypesTableNamesList.ToList().ForEach(criticalDataType =>
            {
                var (singularTableName, schema) = GetTableNameInSingularAndSchema(criticalDataType);

                if (string.IsNullOrEmpty(sqlStatement))
                {
                    sqlStatement = SQLStatement.Replace("@TABLE_NAME@", singularTableName)
                        .Replace("@SCHEMA_NAME@", schema)
                        .Replace("@TABLE_NAME_WITH_SCHEMA@", criticalDataType);
                }
                else
                {
                    sqlStatement += "\r\nUNION\r\n";
                    sqlStatement += SQLStatement.Replace("@TABLE_NAME@", singularTableName)
                        .Replace("@SCHEMA_NAME@", schema)
                        .Replace("@TABLE_NAME_WITH_SCHEMA@", criticalDataType);
                }
            });

            SQLStatement = sqlStatement + @"
ORDER BY
	[Schema]
	, [TableName]
	, [Id]";
        }

        /// <summary>
        /// The "crititcalDataTableName" is of this format: [Schema].[TableName].
        /// </summary>
        /// <param name="crititcalDataTableName"></param>
        /// <returns></returns>
        private (string SingularTableName, string Schema) GetTableNameInSingularAndSchema(string crititcalDataTableName)
        {
            crititcalDataTableName = crititcalDataTableName.Replace("[", string.Empty).Replace("]", string.Empty);
            var data = crititcalDataTableName.Split('.');
            var schema = data[0];
            var tableName = data[1];

            // ** Init
            var singularTableName = Utilities.Inflector.MakeSingular(tableName);

            // ** Return
            return (singularTableName, schema);
        }
    }
}