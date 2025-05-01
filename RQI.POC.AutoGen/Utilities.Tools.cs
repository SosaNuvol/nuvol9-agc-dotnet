#region File Header
// <copyright file="Utilities.Tools.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using Models.Base;

    public static partial class Utilities
    {
        #region Tools

        /// <summary>
        /// Performs a case-insensitive comparison of two passed strings, 
        /// with an option to trim the strings before comparison.
        /// </summary>
        /// <param name="stringA">The string to compare with the second parameter</param>
        /// <param name="stringB">The string to compare with the first parameter</param>
        /// <param name="trimStrings">if set to <c>true</c> strings will be trimmed before comparison.</param>
        /// <returns>
        /// 	<c>true</c> if the strings match; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMatch(string stringA, string stringB, bool trimStrings)
        {
            if (trimStrings)
                return String.Equals(stringA.Trim(), stringB.Trim(), StringComparison.InvariantCultureIgnoreCase);

            return String.Equals(stringA, stringB, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Determines whether the passed string matches the passed RegEx pattern.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="matchPattern">The RegEx match pattern.</param>
        /// <returns>
        /// 	<c>true</c> if the string matches the pattern; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRegexMatch(string inputString, string matchPattern)
        {
            return Regex.IsMatch(inputString, matchPattern,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Strips any whitespace from the passed string using RegEx
        /// </summary>
        /// <param name="inputString">The string to strip of whitespace</param>
        /// <returns></returns>
        public static string StripWhitespace(string inputString)
        {
            if (!String.IsNullOrEmpty(inputString))
                return Regex.Replace(inputString, @"\s", String.Empty);

            return inputString;
        }

        /// <summary>
        /// Determine whether the passed string is numeric, by attempting to parse it to a double
        /// </summary>
        /// <param name="str">The string to evaluated for numeric conversion</param>
        /// <returns>
        /// 	<c>true</c> if the string can be converted to a number; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStringNumeric(string str)
        {
            // ** Init
            // ReSharper disable once NotAccessedVariable
            double result;

            // ** Return result
            return (double.TryParse(str, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result));
        }

        /// <summary>
        /// Determines whether the passed DbType supports null values.
        /// </summary>
        /// <param name="dbType">The DbType to evaluate</param>
        /// <returns>
        /// 	<c>true</c> if the DbType supports null values; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullableDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Binary:
                //case DbType.Byte:
                case DbType.Object:
                case DbType.String:
                case DbType.StringFixedLength:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Reformats the passed string to ProperCase
        /// </summary>
        /// <param name="sIn">The string to reformat to proper case</param>
        /// <returns></returns>
        public static string GetProperName(string sIn)
        {
            string propertyName = Inflector.ToPascalCase(sIn);
            return propertyName;
        }

        /// <summary>
        /// Keys the word check.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="table">The table.</param>
        /// <param name="appendWith">The append with.</param>
        /// <returns></returns>
        public static string KeyWordCheck(string word, string table, string appendWith)
        {
            string newWord = String.Concat(word, appendWith);

            //if(word == "Schema")
            //    newWord =  word + appendWith;

            //Can't have a property with same name as class.
            if (word == table)
                return newWord;

            string evalWord = word.ToLower();

            switch (evalWord)
            {
                // C# keywords
                // ReSharper disable StringLiteralTypo
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "date":
                case "datetime":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typecode":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "volatile":
                case "void":
                case "while":
                // C# contextual keywords
                case "get":
                case "partial":
                case "set":
                case "value":
                case "where":
                case "yield":
                // VB.NET keywords (commented out keywords that are the same as in C#)
                case "alias":
                case "addHandler":
                case "ansi":
                //case "as": 
                case "assembly":
                case "auto":
                case "binary":
                case "byref":
                case "byval":
                case "custom":
                case "directcast":
                case "each":
                case "elseif":
                case "end":
                case "error":
                case "friend":
                case "global":
                case "handles":
                case "implements":
                case "lib":
                case "loop":
                case "me":
                case "module":
                case "mustinherit":
                case "mustoverride":
                case "mybase":
                case "myclass":
                case "narrowing":
                case "next":
                case "nothing":
                case "notinheritable":
                case "notoverridable":
                case "of":
                case "off":
                case "on":
                case "option":
                case "optional":
                case "overloads":
                case "overridable":
                case "overrides":
                case "paramarray":
                case "preserve":
                case "property":
                case "raiseevent":
                case "resume":
                case "shadows":
                case "shared":
                case "step":
                case "structure":
                case "text":
                case "then":
                case "to":
                case "trycast":
                case "unicode":
                case "until":
                case "when":
                case "widening":
                case "withevents":
                case "writeonly":
                // VB.NET unreserved keywords
                case "compare":
                case "isfalse":
                case "istrue":
                case "mid":
                case "strict":
                case "schema":
                    return newWord;
                default:
                    return word;
                // ReSharper restore StringLiteralTypo
            }
        }

        /// <summary>
        /// Keys the word check.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public static string KeyWordCheck(string word, string table)
        {
            var appendWith = "X";
            return KeyWordCheck(word, table, appendWith);
        }

        /// <summary>
        /// Strips the text.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <param name="stripString">The strip string.</param>
        /// <returns></returns>
        public static string StripText(string inputString, string stripString)
        {
            if (!string.IsNullOrEmpty(stripString))
            {
                string[] replace = stripString.Split(new char[] {','});
                for (int i = 0; i < replace.Length; i++)
                {
                    if (!string.IsNullOrEmpty(inputString))
                        inputString = Regex.Replace(inputString, replace[i], string.Empty);
                }
            }

            return inputString;
        }

        /// <summary>
        /// Replaces any matches found in word from list.
        /// </summary>
        /// <param name="word">The string to check against.</param>
        /// <param name="find">A comma separated list of values to replace.</param>
        /// <param name="replaceWith">The value to replace with.</param>
        /// <param name="removeUnderscores">Whether or not underscores will be kept.</param>
        /// <returns></returns>
        public static string Replace(string word, string find, string replaceWith, bool removeUnderscores)
        {
            string[] findList = Split(find);
            string newWord = word;
            foreach (string f in findList)
            {
                if (f.Length > 0)
                    newWord = newWord.Replace(f, replaceWith);
            }

            if (removeUnderscores)
                return newWord.Replace(" ", String.Empty).Replace("_", String.Empty).Trim();
            return newWord.Replace(" ", String.Empty).Trim();
        }

        /// <summary>
        /// A custom split method
        /// </summary>
        /// <param name="list">A list of values separated by either ", " or ","</param>
        /// <returns></returns>
        public static string[] Split(string list)
        {
            string[] find;
            try
            {
                find = list.Split(new [] {", ", ","}, StringSplitOptions.RemoveEmptyEntries);
            }
            catch
            {
                find = new [] {string.Empty};
            }

            return find;
        }

        /// <summary>
        /// Gets the name of the parameter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="stripParamText">Text to strip.</param>
        /// <returns></returns>
        public static string GetParameterName(string name, string stripParamText)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            string newName = name;
            newName = Replace(newName, stripParamText, String.Empty, false);
            newName = GetProperName(newName);
            newName = IsStringNumeric(newName) ? String.Concat("_", newName) : newName;
            newName = StripNonAlphaNumeric(newName);
            newName = newName.Replace("@", String.Empty);
            newName = newName.Trim();
            return KeyWordCheck(newName, String.Empty);
        }

        /// <summary>
        /// Replaces most non-alpha-numeric chars
        /// </summary>
        /// <param name="sIn">The s in.</param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string sIn)
        {
            return StripNonAlphaNumeric(sIn, " ".ToCharArray()[0]);
        }

        /// <summary>
        /// Replaces most non-alpha-numeric chars
        /// </summary>
        /// <param name="sIn">The s in.</param>
        /// <param name="cReplace">The placeholder character to use for replacement</param>
        /// <returns></returns>
        public static string StripNonAlphaNumeric(string sIn, char cReplace)
        {
            StringBuilder sb = new StringBuilder(sIn);
            //these are illegal characters - remove zem
            const string stripList = ".'?\\/><$!@%^*&+,;:\"(){}[]|-#";

            for (int i = 0; i < stripList.Length; i++)
                sb.Replace(stripList[i], cReplace);

            sb.Replace(cReplace.ToString(), String.Empty);
            return sb.ToString();
        }

        /// <summary>
        /// Regexes the transform.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="regexIgnoreCase">flag to ignore case</param>
        /// <param name="regexMatchExpression"></param>
        /// <param name="regexDictionaryReplace"></param>
        /// <param name="regexReplaceExpression"></param>
        /// <returns>string</returns>
        public static string RegexTransform(string inputText, bool regexIgnoreCase, string regexMatchExpression,
            string regexDictionaryReplace, string regexReplaceExpression)
        {
            if (!string.IsNullOrEmpty(regexMatchExpression) || !string.IsNullOrEmpty(regexDictionaryReplace))
            {
                Regex rx;

                if (!string.IsNullOrEmpty(regexMatchExpression))
                {
                    rx = regexIgnoreCase
                        ? new Regex(regexMatchExpression, RegexOptions.IgnoreCase)
                        : new Regex(regexMatchExpression);
                    inputText = rx.Replace(inputText, regexReplaceExpression);
                }

                if (!string.IsNullOrEmpty(regexDictionaryReplace) && !string.IsNullOrEmpty(inputText))
                {
                    string regexString = Regex.Replace(regexDictionaryReplace, "[\r\n\t]", string.Empty);

                    string[] pairs = Regex.Split(regexString, ";");
                    foreach (string pair in pairs)
                    {
                        string[] keys = Regex.Split(pair, ",");
                        if (keys.Length == 2)
                        {
                            rx = regexIgnoreCase ? new Regex(keys[0], RegexOptions.IgnoreCase) : new Regex(keys[0]);
                            inputText = rx.Replace(inputText, keys[1]);
                        }
                    }
                }
            }

            return inputText;
        }

        public static string GetSysTypeWithNullable(string sqlType, bool isNullable)
        {
            // ** Init
            var result = GetSysType(sqlType);

            // ** Set nullable if needed
            if (isNullable)
            {
                switch (result)
                {
                    case "int":
                    case "long":
                    case "short":
                    case "Guid":
                    case "double":
                    case "decimal":
                    case "byte":
                    case "bool":
                    case "DateTime":
                    case "DateTimeOffset":
                    case "TimeSpan":
                        result = $"{result}?";
                        break;
                }
            }

            // ** Return result
            return result;
        }

        public static string GetSysType(string sqlType)
        {
            string sysType = "string";
            switch (sqlType.ToLower())
            {
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "Guid";
                    break;
                case "date":
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                    sysType = "DateTime";
                    break;
                case "datetimeoffset":
                    sysType = "DateTimeOffset";
                    break;
                case "time":
                    sysType = "TimeSpan";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                    sysType = "byte[]";
                    break;
            }
            return sysType;
        }

        public static DbType GetDbType(string sqlType)
        {
            switch (sqlType)
            {
                case "varchar":
                    return DbType.AnsiString;
                case "nvarchar":
                    return DbType.String;
                case "int":
                    return DbType.Int32;
                case "uniqueidentifier":
                    return DbType.Guid;
                case "datetime":
                    return DbType.DateTime;
                case "datetimeoffset":
                    return DbType.DateTimeOffset;
                case "bigint":
                    return DbType.Int64;
                case "binary":
                    return DbType.Binary;
                case "bit":
                    return DbType.Boolean;
                case "char":
                    return DbType.AnsiStringFixedLength;
                case "decimal":
                    return DbType.Decimal;
                case "float":
                    return DbType.Double;
                case "image":
                    return DbType.Binary;
                case "money":
                    return DbType.Currency;
                case "nchar":
                    return DbType.String;
                case "ntext":
                    return DbType.String;
                case "numeric":
                    return DbType.Decimal;
                case "real":
                    return DbType.Single;
                case "smalldatetime":
                    return DbType.DateTime;
                case "smallint":
                    return DbType.Int16;
                case "smallmoney":
                    return DbType.Currency;
                case "sql_variant":
                    return DbType.String;
                case "sysname":
                    return DbType.String;
                case "text":
                    return DbType.AnsiString;
                case "timestamp":
                    return DbType.Binary;
                case "tinyint":
                    return DbType.Byte;
                case "varbinary":
                    return DbType.Binary;
                default:
                    return DbType.AnsiString;
            }
        }

        #region Model Utility
        /// <summary>
        /// Gets plural of the specified name.
        /// </summary>
        /// <param name="name">The name to pluralize.</param>
        /// <returns>Plural name.</returns>
        public static string GetPluralName(string name)
        {
            string plural;

            if (SkipGetPluralName(name))
            {
                return name;
            }

            if (Regex.IsMatch(name, "([^aeiouy]|qu)y$", RegexOptions.IgnoreCase))
            {
                plural = Regex.Replace(name, "y$", "ies");
            }
            else if (name.EndsWith("Info") || name.EndsWith("Information"))
            {
                plural = name;
            }
            else if (WordsThatShouldEndInES(name))
            {
                plural = $"{name}es";
            }
            else
            {
                plural = $"{name}s";
            }

            return plural;
        }

        public static bool SkipGetPluralName(string name)
        {
            // ** Init
            var skipList = SkipPluralNamesList.Split(',');

            // ** Look for Ends with
            var itemFound = skipList.FirstOrDefault(name.EndsWith);

            // ** Return result
            return !string.IsNullOrEmpty(itemFound);
        }

        public static bool WordsThatShouldEndInES(string name)
        {
            // ** Init
            var skipList = WordsThatShouldEndInESList.Split(',');

            // ** Look for Ends with
            var itemFound = skipList.FirstOrDefault(name.EndsWith);

            // ** Return result
            return !string.IsNullOrEmpty(itemFound);
        }

        public static string CheckNullable(Column col)
        {
            // ** Init
            var result = "";

            // ** Check
            if (col.IsNullable && col.SysType != "byte[]" && col.SysType != "string")
                result = "?";

            // ** Return result
            return result;
        }

        public static bool IsHistoricalColumn(Column col)
        {
            // ** Check
            if (HistoricalColumns.Contains(col.Name))
                return true;

            // ** Return result
            return false;
        }

        public static bool IsAuditColumn(Column col)
        {
            // ** Check
            if (AuditColumns.Contains(col.Name))
                return true;
            // ** Return result
            return false;
        }

        /// <summary>
        /// This is used in T4T to not add a ForeignKey Property since it will not be rendered in the Models
        /// </summary>
        /// <param name="col"></param>
        /// <param name="tablesList"></param>
        /// <returns><c>bool</c></returns>
        // ReSharper disable once UnusedMember.Global
        public static bool IsForeignKeyTableInTablesList(Column col, IEnumerable<Table> tablesList)
        {
            // ** Init
            var foundItem = tablesList.FirstOrDefault(t => t.Schema.Equals(col.ForeignKeyTableSchemaName) && t.Name.Equals(col.ForeignKeyTableName));

            // ** Check and return
            return foundItem != null;
        }

        /// <summary>
        /// This is used in T4T to not add a collection item into the Models.
        /// </summary>
        /// <param name="col"></param>
        /// <param name="tablesList"></param>
        /// <returns><c>bool</c></returns>
        // ReSharper disable once UnusedMember.Global
        public static bool IsCollectionInTableList(NavigationCollection col, IEnumerable<Table> tablesList)
        {
            // ** Init
            var foundItem = tablesList.FirstOrDefault(t => t.Schema.Equals(col.CollectionName) && t.Name.Equals(col.Schema));

            // ** Check and return
            return foundItem != null;
        }
        #endregion Model Utility

    }

    #endregion //Utility
}