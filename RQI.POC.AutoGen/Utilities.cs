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

    public static partial class Utilities
    {
        #region Methods

        /// <summary>
        /// All table names should have an associated schema.  If the passed <c>tableName</c> does not have a schema, it will assign 'dbo.' to the table name.
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns>string</returns>
        public static string AddSchemaIfMissing(string tableName)
        {
            // ** Check that there is a schema
            // ReSharper disable once StringIndexOfIsCultureSpecific.1
            if (tableName.IndexOf(".") > -1) return tableName;

            // ** Add default schema of dbo
            // ReSharper disable once UseStringInterpolation
            return string.Format("dbo.{0}", tableName);
        }

        public static bool ShouldGenerateTable(string tableName)
        {
            // ** Init
            var generateAll = IncludeTableList.Equals("*");

            // ** Check Generate All tables
            if (generateAll)
            {
                /*********************************************************************
                 * When generating all tables then we look at the exclude tables list
                 *********************************************************************/

                // ** Init
                var excludeList = ExcludeTableListEnumerable;

                // ** Check that the table name is not in the exclude list
                foreach (var excludeItem in excludeList)
                {
	                if (IsRegexMatch(tableName, excludeItem.Trim())) return false;
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
            foreach (var includeItem in IncludeTableListEnumerable)
            {
	            if (IsRegexMatch(tableName, includeItem.Trim())) return true;
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