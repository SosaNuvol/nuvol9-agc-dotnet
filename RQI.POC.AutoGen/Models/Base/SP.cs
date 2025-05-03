#region File Header
// <copyright file="SP.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:27 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Newtonsoft.Json;

    public class SP
    {
        #region Properties
        
        private readonly string _definition;
        private const string _REG_EX_FIND_HEADER = "CREATE[ ]*PROCEDURE[ \\[]*@SCHEMA_NAME@[\\].[]*@SPROC_NAME@[\\]]*";
        private const string _REG_EX_ARGUMENT = "\\@[\\S+]* [\\S+\\(\\)]*[ ]*=[ ]*([\\S+]*)";

        public string SchemaName { get; set; }

        public string Name { get; set; }

        public string CleanName { get; set; }

        public string ClassName { get; set; }

        public bool ReturnsNothing { get; set; } = true;

        public bool IsSingleColumnResultSet { get; set; }

        public SPResultSetColumn SingleColumnResultSet { get; private set; }

        public List<SPParam> Parameters { get; set; }

        public List<SPResultSetColumn> ResultSetColumns { get; set; }

        #endregion Properties

        public SP(string definition, string jsonParameterList, string schema, string sprocName)
        {
            // ** Bind Definitions
            SchemaName = schema;
            Name = sprocName;
            Parameters = string.IsNullOrEmpty(jsonParameterList) ? new List<SPParam>() : JsonConvert.DeserializeObject<List<SPParam>>(jsonParameterList);
            _definition = definition;

            // ** Initialize SPROC
            InitializeSPROC();
        }

        #region Methods

        public void InitializeSPROC()
        {
            // ** Check that there is a definition
            if (string.IsNullOrEmpty(_definition)) return;

            // ** Process SPROC from Definition
            InitSPROCDefinition();

            // ** Process SPROC Result Set
            InitSPROCResultSet();
        }

        /// <summary>
        /// Take the SPROC name and pull from the database the output of it.
        /// </summary>
        private void InitSPROCResultSet()
        {
            // ** Init
            var sql = Utilities.SPROC_RESULT_SET_SQL_STATEMENT.Replace("@SPROC_FULLNAME@", $"[{SchemaName}].[{Name}]");
            ResultSetColumns = new List<SPResultSetColumn>();

            // ** Execute
            using (IDataReader rdr = Utilities.GetReader(sql))
            {
                // ** Build the dataSet.
                while (rdr.Read())
                {
                    // ** Init
                    ReturnsNothing = false;

                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var row = new SPResultSetColumn();

                    row.IsNullable = (bool) rdr["IsNullable"];

                    row.Name = rdr["ColName"] == DBNull.Value ? null : (string)rdr["ColName"];
                    row.Name = row.Name?.Trim();
                    row.ClassName = rdr["ColName"] == DBNull.Value ? null : (string)rdr["ColName"];
                    row.DataType = (string)rdr["DataTypeName"];

                    row.MaxLength = (short)rdr["MaxLength"];
                    row.Precision = (byte)rdr["Precision"];
                    row.Scale = (byte)rdr["Scale"];

                    row.SysType = Utilities.GetSysTypeWithNullable(row.DataType, row.IsNullable);
                    row.DbType = Utilities.GetDbType(row.DataType);
                    row.ColumnOrdinal = (int) rdr["ColumnOrdinal"];

                    // ** Check if the column name is repeated.
                    var foundCol = ResultSetColumns.FirstOrDefault(c => c.Name.Equals(row.Name?.Trim()));
                    if (foundCol != null)
                    {
                        Utilities.LogInternalWarning(Utilities.ClassTypes.Procedure, $"The SPROC: {Name} in the result set has a duplicate column named '{row.Name}'.");
                        continue;
                    }

                    ResultSetColumns.Add(row);
                }

                // ** Check if there is not resultset
                if (ReturnsNothing) return;

                // ** Check for a single value return with no columns
                if (ResultSetColumns.Count == 1 && string.IsNullOrEmpty(ResultSetColumns[0].Name))
                {
                    // ** Pull the DataType and flag it as a scalar value return type
                    IsSingleColumnResultSet = true;
                    SingleColumnResultSet = ResultSetColumns[0];

                    // ** DONE.  So exit
                    return;
                }

                // ** Make sure that all columns have names
                foreach (var spResultSetColumn in ResultSetColumns.Where(c => string.IsNullOrEmpty(c.Name.Trim())))
                {
                    spResultSetColumn.Name = $"Col{spResultSetColumn.ColumnOrdinal}";
                    spResultSetColumn.ClassName = $"Col{spResultSetColumn.ColumnOrdinal}";
                }
            }
        }

        /// <summary>
        /// Take the SPROC definition and parse it's information for the purpose of stripping default values.
        /// </summary>
        private void InitSPROCDefinition()
        {
            // ** Parse Argument Body
            var parameterBody = ParseOutParameterBody();

            // ** Check that there is parameter body
            if (string.IsNullOrEmpty(parameterBody)) return;

            // ** Init the Regex object to loop through parameters body
            var rx = new Regex(_REG_EX_ARGUMENT);

            // ** Loop through arguments
            foreach (Match match in rx.Matches(parameterBody))
            {
                // ** Init
                var argumentDefinition = match.Value;

                // ** Capture values
                var operatorArray = argumentDefinition.Split('=');
                var parameterName = operatorArray[0];
                var valueString = operatorArray[1];

                // ** Init parameterName
                parameterName = parameterName.Trim();
                parameterName = parameterName.Substring(0, parameterName.IndexOf(' ')); // Remove the Type definition

                // ** Init valueString
                valueString = valueString.Trim();

                // ** Find the parameter in the list
                var foundParam = Parameters.Find(s => s.Name.Equals(parameterName));

                // ** Check if found
                if (foundParam == null)
                {
                    Utilities.LogInternalError(Utilities.ClassTypes.Procedure, $"|** The SPROC: {Name} with Parameter {parameterName}");
                    continue;
                }

                // ** Test for default value first
                if (valueString.ToUpper().Equals("NULL"))
                {
                    foundParam.IsOptional = true;
                    continue;
                }

                // ** Capture default value
                foundParam.DefaultValue = valueString;

                // ** Print
                Console.WriteLine("SPROC: {0} | This is the Argument : {1} = {2}", Name, parameterName, valueString);
            }
        }

        /// <summary>
        /// Using Regex it parses out the Parameter body from the SPROC definition
        /// </summary>
        /// <returns><c>string</c>the Argument body</returns>
        private string ParseOutParameterBody()
        {
            // ** Init
            var headerRegEx = _REG_EX_FIND_HEADER.Replace("@SCHEMA_NAME@", SchemaName);
            headerRegEx = headerRegEx.Replace("@SPROC_NAME@", Name);
            var rx = new Regex(headerRegEx);
            var startIndex = rx.Match(_definition).Index;
            // ** ** Check that we found the header
            if (startIndex == -1)
            {
                Utilities.LogInternalError(Utilities.ClassTypes.Procedure, $"|** The SPROC: {SchemaName}.{Name} is unable to find the argument list");
                return null;
            }

            // ** ** Find End of Argument List
            var endIndex = _definition.IndexOf("AS", startIndex, StringComparison.CurrentCulture);
            // ** ** Check that it was found
            if (endIndex == -1)
            {
                Utilities.LogInternalError(Utilities.ClassTypes.Procedure, $"|** The SPROC: {SchemaName}.{Name} is unable to find the end of the argument list");
                return null;
            }

            // ** Capture the Argument Definition block
            var argumentBody = _definition.Substring(startIndex, endIndex - startIndex);

            // ** Return result
            return argumentBody;
        }

        // ReSharper disable once UnusedMember.Global
        public List<SPParam> GetParametersList(bool? hideDefaultValue = null)
        {
            // Check value
            if (hideDefaultValue == null) return Parameters;

            // ** Return hidden default value parameters
            if (hideDefaultValue.Value)
                return Parameters.Where(p => string.IsNullOrEmpty(p.DefaultValue)).OrderBy(p => p.OrdinalPosition)
                    .ToList();

            // ** Return default values parameters only
            return Parameters.Where(p => !string.IsNullOrEmpty(p.DefaultValue)).OrderBy(p => p.OrdinalPosition)
                .ToList();

        }

        #endregion Methods
    }
}