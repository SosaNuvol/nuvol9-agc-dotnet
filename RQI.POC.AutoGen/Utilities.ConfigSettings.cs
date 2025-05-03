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
                var rawValue = Config?.AppSettings.Settings["ConnectionString"]?.Value ?? ConfigurationManager.AppSettings["ConnectionString"];

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
                var rawValue = Config?.AppSettings.Settings[key]?.Value ?? ConfigurationManager.AppSettings[key];

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
                var rawValue = Config?.AppSettings.Settings[key]?.Value ?? ConfigurationManager.AppSettings[key];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.TableSchemaList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.TableSchemaList"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.ViewsSchemaList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.ViewsSchemaList"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.StPrcSchemaList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.StPrcSchemaList"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.StUDTSchemaList"] ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.StPrcSchemaList"];

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.IncludeTableList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.IncludeTableList"];

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "*";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// Wrapper around the IncludeTableList that returns an IEnumerable
        /// </summary>
        public static IEnumerable<string> IncludeTableListEnumerable
        {
            get
            {
                // ** Init
                var tableNameArray = IncludeTableList.Split(',');

                // ** Return result
                return GetRegExEnumerableList(tableNameArray);
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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.ExcludeTableList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.ExcludeTableList"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.IncludeViewList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.IncludeViewList"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.ExcludeViewList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.ExcludeViewList"];

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
                    var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.HistoricalColumns"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.HistoricalColumns"];
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
                    var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.AuditColumns"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.AuditColumns"];
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
            var result = new List<string> {"*"};

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.RegexIgnoreCase"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexIgnoreCase"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.RegexReplaceExpression"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexReplaceExpression"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.RegexMatchExpression"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexMatchExpression"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.RegexDictionaryReplace"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexDictionaryReplace"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.StripText"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.StripText"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.SQL.SystemColToIgnoreList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.SQL.SystemColToIgnoreList"];

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
                var rawValue = Config?.AppSettings.Settings["RQI.POC.AutoGen.SQL.ColumnDataTypeToIgnoreList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.SQL.ColumnDataTypeToIgnoreList"];

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
                var result = Config?.AppSettings.Settings["RQI.POC.AutoGen.EnumTableList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.EnumTableList"];

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
                var result = Config?.AppSettings.Settings["RQI.POC.AutoGen.MetaDataTableList"].Value ?? ConfigurationManager.AppSettings["RQI.POC.AutoGen.MetaDataTableList"];

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
                var result = Config?.AppSettings.Settings[key]?.Value ?? ConfigurationManager.AppSettings[key];

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
                var result = Config?.AppSettings.Settings[key]?.Value ?? ConfigurationManager.AppSettings[key];

                // ** Check configuration value
                if (string.IsNullOrEmpty(result)) return string.Empty;

                // ** Return result
                return result;
            }
        }

        #endregion Configuration Properties
    }
}