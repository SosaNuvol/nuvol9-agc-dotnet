#region File Header
// <copyright file="Utilities.ConfigSettings.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
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
                var rawValue = ConfigurationManager.AppSettings["ConnectionString"];

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "";

                // ** Return result
                return rawValue;
            }
        }

        /// <summary>
        /// List of Schemas that will be used
        /// </summary>
        public static string SchemaList
        {
            get
            {
                // ** Init
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.SchemaList"];

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) rawValue = "Docs,rules";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

                // ** Return result
                return result;
            }
        }

        /// <summary>
        /// List of all tables that will be included in the ORM.  Make sure that the schema is specified.
        ///     i.e. Docs.Packages,rules.Rulesets
        /// </summary>
        public static string IncludeTableList
        {
            get
            {
                // ** Init
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.IncludeTableList"];

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
                return tableNameArray.Select(AddSchemaIfMissing).ToList();
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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.ExcludeTableList"];

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
                return tableNameArray.Select(AddSchemaIfMissing).ToList();
            }
        }

        /// <summary>
        /// This is used for all Regex comparisons.  If true it will ignore the case of string comparisons.
        /// </summary>
        public static bool RegexIgnoreCase
        {
            get
            {
                // ** Init
                bool result;
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexIgnoreCase"];

                // ** Check configuration value
                if (bool.TryParse(rawValue, out result)) return result;

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexReplaceExpression"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexMatchExpression"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.RegexDictionaryReplace"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.StripText"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.SQL.SystemColToIgnoreList"];

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
                var rawValue = ConfigurationManager.AppSettings["RQI.POC.AutoGen.SQL.ColumnDataTypeToIgnoreList"];

                // ** Check configuration value
                if (string.IsNullOrEmpty(rawValue)) return "''";

                // ** Build the
                var result = ConvertToSQLListFormat(rawValue);

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

        #endregion Configuration Properties
    }
}