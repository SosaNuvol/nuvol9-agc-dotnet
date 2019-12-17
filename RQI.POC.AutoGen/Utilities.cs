#region File Header

// <copyright file="Class1.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:15 PM
// </summary>
// <author>Andres Sosa</author>

#endregion

namespace RQI.POC.AutoGen
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;

    public static partial class Utilities
    {
        private static Configuration _config;

        public static Configuration Config => _config;

        // ** Initialization
        public static void InitializeMe(Configuration config)
        {
            _config = config;
        }

        #region Methods

        /// <summary>
        /// All table names should have an associated schema.  If the passed <c>tableName</c> does not have a schema, it will assign 'dbo.' to the table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>string</returns>
        public static string AddSchemaIfMissing(string tableName)
        {
            // ** Check that there is a schema
            if (tableName.IndexOf(".", StringComparison.Ordinal) > -1) return tableName;

            // ** Add default schema of dbo
            return $"dbo.{tableName}";
        }

        public static bool ShouldGenerateTable(string tableName)
        {
            return ShouldGenerate(tableName, IncludeTableListEnumerable, ExcludeTableListEnumerable);
        }

        public static bool ShouldGenerateView(string viewName)
        {
            return ShouldGenerate(viewName, IncludeViewListEnumerable, ExcludeViewListEnumerable);
        }

        public static bool ShouldGenerate(string objectName, IEnumerable<string> includeList, IEnumerable<string> excludeList)
        {
            // ** Init
            var includeItems = includeList as string[] ?? includeList.ToArray();
            var generateAll = includeItems.FirstOrDefault(s => s.Equals("*"));

            // ** Check Generate All tables
            if (!string.IsNullOrEmpty(generateAll))
            {
                /*********************************************************************
                 * When generating all tables then we look at the exclude tables list
                 *********************************************************************/
                // ** Check that the table name is not in the exclude list
                foreach (var excludeItem in excludeList)
                {
                    if (IsRegexMatch(objectName, excludeItem.Trim())) return false;
                }

                // ** Table was not on the exclude list
                return true;
            }

            /************************************************************************************
             * If includeList does not have a '*' then we ignore the excludeList and only use
             *  the tables in the includesList.
             * This means that tables were specifically requested in the includeList need to make
             * them prove themselves.
             ************************************************************************************/
            foreach (var includeItem in includeItems)
            {
                if (IsRegexMatch(objectName, includeItem.Trim())) return true;
            }

            // ** Return result
            return false;
        }

        public static string TransformName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            string newName = name;
            newName = RegexTransform(newName, RegexIgnoreCase, RegexMatchExpression, RegexDictionaryReplace, RegexReplaceExpression);
            newName = StripText(newName, StripTextConstant);
            //newName = Inflector.ToPascalCase(newName, false);
            newName = IsStringNumeric(newName) ? "_" + newName : newName;
            newName = StripNonAlphaNumeric(newName);
            newName = newName.Trim();
            newName = KeyWordCheck(newName, String.Empty);
            return newName;
        }

        #endregion Methods
    }
}