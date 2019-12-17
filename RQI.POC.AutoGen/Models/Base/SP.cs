#region File Header
// <copyright file="SP.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
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

        public List<SPParam> Parameters { get; set; }

        #endregion Properties

        public SP(string definition, string jsonParameterList, string schema, string sprocName)
        {
            // ** Bind Properties
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

        #endregion Methods
    }
}