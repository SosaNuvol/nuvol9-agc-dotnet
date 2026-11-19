#region File Header
// <copyright file="Utilities.ConfigSettings.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public static partial class Utilities
    {
        #region Configuration Properties

        public static string ConnectionString
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("ConnectionString");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "";

                // ** Return result
                return rawValue;
            }
        }

        public static string NamespaceModels
        {
            get
            {
                // ** Init
                const string key = "RQI.POC.AutoGen.NamespaceModels";
                var rawValue = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "";

                // ** Return result
                return rawValue;
            }
        }

        public static string NamespaceDataAccess
        {
            get
            {
                // ** Init
                const string key = "RQI.POC.AutoGen.NamespaceDataAccess";
                var rawValue = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// List of Schemas that will be used for tables.
        /// </summary>
        public static string TableSchemaList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.TableSchemaList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "Docs,rules";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        /// <summary>
        /// List of Schemas that will be used for views
        /// </summary>
        public static string ViewsSchemaList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.ViewsSchemaList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        /// <summary>
        /// List of Schemas that will be used for views
        /// </summary>
        public static string ProceduresSchemaList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.StPrcSchemaList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        public static string UserDefinedTypesSchemaList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.StUDTSchemaList") ?? AgcSettings.Get("RQI.POC.AutoGen.StPrcSchemaList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        /// <summary>
        /// List of Schemas that will be used for critical data types
        /// </summary>
        public static string CriticalDataTypesSchemaList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.CriticalDataTypesSchemaList") ?? AgcSettings.Get("RQI.POC.AutoGen.StUDTSchemaList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        public static string CriticalDataTypesTablesList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.CriticalDataTypesTablesList") ?? AgcSettings.Get("RQI.POC.AutoGen.CriticalDataTypesTablesList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "";

                // ** Build the
                var result = rawValue;

                // ** Return result
                return result;
            }
        }

        #region TABLES
        /// <summary>
        /// List of all tables that will be included in the ORM.  Make sure that the schema is specified.
        ///     i.e. Docs.Packages,rules.Rulesets
        /// </summary>
        public static string IncludeTableList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.IncludeTableList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// List of all stored procedures that will be included in the ORM.  Make sure that the schema is specified.
        ///     i.e. dbo.spGetCustomer,dbo.spUpdateOrder
        /// </summary>
        /// <remarks>Each entry in the returned list may be a RegEx expression used to match procedure names.</remarks>
        public static IEnumerable<string> IncludeProcedureList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.IncludeProcedureList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Return result
                return GetRegExEnumerableList(rawValue.Split(','));
            }
        }

        /// <summary>
        /// Gets the list of procedure name patterns to exclude.
        /// </summary>
        /// <remarks>The value is retrieved from the application configuration setting
        /// 'RQI.POC.AutoGen.ExcludeProcedureList'. If the setting is not found or is empty, the default value
        /// '^spGet*,^spUpdate*' is used. Each entry in the returned list may be a RegEx expression used to match
        /// procedure names when generating code or performing automated tasks.</remarks>
        public static IEnumerable<string> ExcludeProcedureList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.ExcludeProcedureList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "^spGet*,^spUpdate*";

                // ** Return result
                return GetRegExEnumerableList(rawValue.Split(','));
            }
        }

        /// <summary>
        /// Returns the include table list as an IEnumerable for RegEx filtering.
        /// </summary>
        public static IEnumerable<string> IncludeTableListEnumerable
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.IncludeTableList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Return result
                return GetRegExEnumerableList(rawValue.Split(','));
            }
        }

        /// <summary>
        /// This is a list of tables to exclude when the includeList has an '*'.
        /// </summary>
        public static string ExcludeTableList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.ExcludeTableList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "dbo.sysdiagrams";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// Wrapper around the ExcludeTableList that returns an IEnumerable
        /// </summary>
        public static IEnumerable<string> ExcludeTableListEnumerable
        {
            get
            {
                // ** Init
                var tableNameArray = ExcludeTableList.Split(',');

                // ** Return result
                return GetRegExEnumerableList(tableNameArray);
            }
        }
        #endregion TABLES

        #region VIEWS

        /// <summary>
        /// List of all tables that will be included in the ORM.  Make sure that the schema is specified.
        ///     i.e. Docs.Packages,rules.Rulesets
        /// </summary>
        public static string IncludeViewList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.IncludeViewList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// Wrapper around the IncludeTableList that returns an IEnumerable
        /// </summary>
        public static IEnumerable<string> IncludeViewListEnumerable
        {
            get
            {
                // ** Init
                var tableNameArray = IncludeViewList.Split(',');

                // ** Return result
                return GetRegExEnumerableList(tableNameArray);
            }
        }

        /// <summary>
        /// This is a list of tables to exclude when the includeList has an '*'.
        /// </summary>
        public static string ExcludeViewList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.ExcludeViewList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "^vwGen_*,vwUserExperienceSchedules";

                // ** Return result
                return rawValue;
            }
        }

        private static IList<string> _historicalColumns;
        public static IList<string> HistoricalColumns
        {
            get
            {
                if (_historicalColumns == null)
                {
                    _historicalColumns = new List<string>();
                    var rawValue = AgcSettings.Get("RQI.POC.AutoGen.HistoricalColumns");
                    if (string.IsNullOrEmpty(rawValue)) return _historicalColumns;
                    var historicalColumns = rawValue.Split(',');

                    foreach (var column in historicalColumns)
                    {
                        if (string.IsNullOrEmpty(column)) continue;
                        _historicalColumns.Add(column);
                    }
                }
                return _historicalColumns;
            }
        }

        private static IList<string> _auditColumns;
        public static IList<string> AuditColumns
        {
            get
            {
                if (_auditColumns == null)
                {
                    _auditColumns = new List<string>();
                    var rawValue = AgcSettings.Get("RQI.POC.AutoGen.AuditColumns");
                    if (string.IsNullOrEmpty(rawValue)) return _auditColumns;
                    var auditColumns = rawValue.Split(',');
                    foreach (var column in auditColumns)
                    {
                        if (string.IsNullOrEmpty(column)) continue;
                        _auditColumns.Add(column);
                    }
                }
                return _auditColumns;
            }
        }

        /// <summary>
        /// Wrapper around the ExcludeTableList that returns an IEnumerable
        /// </summary>
        public static IEnumerable<string> ExcludeViewListEnumerable
        {
            get
            {
                // ** Init
                var tableNameArray = ExcludeViewList.Split(',');

                // ** Return result
                return GetRegExEnumerableList(tableNameArray);
            }
        }

        #endregion VIEWS

        public static IEnumerable<string> GetRegExEnumerableList(string[] regExpressionList)
        {
            // ** Init
            var result = new List<string> { "*" };

            // ** Check for wildcard 
            if (regExpressionList.Length == 1 && regExpressionList[0].Equals("*")) return result;

            // ** Return result
            return regExpressionList.Select(AddSchemaIfMissing).ToList();
        }

        /// <summary>
        /// This is used for all Regex comparisons.  If true it will ignore the case of string comparisons.
        /// </summary>
        public static bool RegexIgnoreCase
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.RegexIgnoreCase");

                // ** Check configuration value
                if (bool.TryParse(rawValue, out var result)) return result;

                // ** Return result
                return false;
            }
        }

        /// <summary>
        /// This is the expression used to replace a value in a table name.
        /// </summary>
        public static string RegexReplaceExpression
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.RegexReplaceExpression");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = string.Empty;

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// This is the match expression used for tables.
        /// </summary>
        public static string RegexMatchExpression
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.RegexMatchExpression");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = string.Empty;

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// RegEx expression that identifies the items that will be modeled be it a table, view, SPROC.
        /// </summary>
        public static string RegexDictionaryReplace
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.RegexDictionaryReplace");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = string.Empty;

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// Text to strip from SPROCs that are custom.
        /// </summary>
        public static string StripTextConstant
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.StripText");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = string.Empty;

                // ** Return result
                return rawValue;
            }
        }

        public static string SQLSystemColToIgnoreList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.SQL.SystemColToIgnoreList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) return "''";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        public static string SQLColDataTypeToIgnoreList
        {
            get
            {
                // ** Init
                var rawValue = AgcSettings.Get("RQI.POC.AutoGen.SQL.ColumnDataTypeToIgnoreList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) return "''";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        public static string EnumTableList
        {
            get
            {
                // ** Init
                var result = AgcSettings.Get("RQI.POC.AutoGen.EnumTableList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string MetaDataTableList
        {
            get
            {
                // ** Init
                var result = AgcSettings.Get("RQI.POC.AutoGen.MetaDataTableList");

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        private static string ConvertToSQLListFormat(string rawList)
        {
            // ** Init
            var result = new StringBuilder();
            var arrayList = rawList.Split(',');

            // ** Check that there is a list
            if (string.IsNullOrEmpty(rawList)) return "''";

            // Loop through items
            foreach (var listItem in arrayList)
            {
                // ** Check for first pass
                if (result.Length == 0)
                {
                    result.AppendFormat("'{0}'", listItem);
                    continue;
                }

                // ** Build list
                result.AppendFormat(",'{0}'", listItem);
            }

            // ** Return result
            return result.ToString();
        }

        public static string SkipPluralNamesList
        {
            get
            {
                // ** Init
                const string key = "RQI.POC.AutoGen.SkipPluralNamesList";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string WordsThatShouldEndInESList
        {
            get
            {
                // ** Init
                const string key = "RQI.POC.AutoGen.WordsShouldEndES";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string AutogGenFileExtension
        {
            get
            {
                // ** Init
                const string key = "RQI.POC.AutoGen.AutogGenFileExtension";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return ".generated.cs";

                // ** Return result
                return result;
            }
        }

        #region GEN Configuration Properties

        public static string TableTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.TableTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string ViewTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.ViewTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string SprocTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.SPROCTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string ProxyTypeTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.ProxyTypeTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UddtTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UDDTTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DataModelsRootFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.DataModelsRootFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DataAccessRootFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.DataAccessRootFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string RepositoryRootFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.RepositoryRootFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DataEntityRootFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.Data.EntityRootFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string ProxyTypeRootFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.ProxyTypeRootFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DbContextTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.DbContextTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string RepositoryInterfaceTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.RepositoryInterfaceTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string RepositoryTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.RepositoryTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UnitOfWorkTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UnitOfWorkTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UnitOfWorkInterfaceTemplateFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UnitOfWorkInterfaceTemplateFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UnitOfWorkFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UnitOfWorkFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UnitOfWorkInterfaceFilePath
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UnitOfWorkInterfaceFilePath";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string UnitOfWorkNameSpace
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.UnitOfWorkNameSpace";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DbContextNamespance
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.DbContextNamespance";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        public static string DataDataNameSpace
        {
            get
            {
                // ** Init
                const string key = "RQI.GEN.DataDataNameSpace";
                var result = AgcSettings.Get(key);

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        #endregion GEN Configuration Properties

        #endregion Configuration Properties
    }
}