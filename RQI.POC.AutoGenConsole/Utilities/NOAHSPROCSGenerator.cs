namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using static RQI.POC.AutoGen.Utilities;

    public class NOAHSPROCSGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<SP> _rmSPROCS;

        // Schemas that have generated UDDT classes. Null means unknown, so the UDDT using line is always kept.
        private readonly HashSet<string> _uddtSchemas;

        // Matches the "using <something>.UDDTs.<SCHEMA>;" line of the template, with its line break.
        private static readonly Regex UddtUsingLine = new Regex(@"^[ \t]*using[ \t]+[\w.]*UDDTs\.<SCHEMA>;[ \t]*\r?\n", RegexOptions.Multiline);

        private const string COMMAND_LINE_VOID = "\t\t\tvar result = await _dbContext.Database.ExecuteSqlRawAsync(\"EXEC <SCHEMA.SPROC_NAME> <SPROC_PARAM_LIST>\"<METHOD_PARAMS_LIST>);";
        private const string COMMAND_LINE_TABLE = @"			var result = await _dbContext.Set<<@RETURN_TYPE@>>()
				.FromSqlRaw(""EXEC <SCHEMA.SPROC_NAME> <SPROC_PARAM_LIST>;""<METHOD_PARAMS_LIST>)
				.ToListAsync();";
        private const string COMMAND_LINE_NON_QUERY = "\t\t\tawait _dbContext.Database.ExecuteSqlRawAsync(\"EXEC <SCHEMA.SPROC_NAME> <SPROC_PARAM_LIST>\"<METHOD_PARAMS_LIST>);";

        public NOAHSPROCSGenerator(IOrderedEnumerable<SP> rmSPROCS, IEnumerable<string> uddtSchemas = null)
        {
            _rmSPROCS = rmSPROCS;
            _uddtSchemas = uddtSchemas == null ? null : new HashSet<string>(uddtSchemas, StringComparer.OrdinalIgnoreCase);
        }

        public void Generate(int counter = 0)
        {
            var headerTemplate = _templateManager.GetSPROCTemplatePrimer();

            // ** Loop through SPROCs
            foreach (var sprocItem in _rmSPROCS)
            {
                // ** Init
                counter++;
                var className = Inflector.MakeSPROCClassName(sprocItem.CleanName);
                var nameSpaceString = $"{DataDataNameSpace}.Models.{sprocItem.SchemaName}";
                var outputParams = GetOutputParameters(sprocItem);

                // ** A result set is only read when a C# return type is configured for it in GEN.StoredProcedureReturnTypes.
                var readsResultSet = !sprocItem.ReturnsNothing && !string.IsNullOrEmpty(sprocItem.CSharpReturnType);
                if (!sprocItem.ReturnsNothing && !readsResultSet)
                {
                    LogInternalWarning(ClassTypes.Procedure, $"The SPROC: {sprocItem.SchemaName}.{sprocItem.Name} returns a result set but has no row in GEN.StoredProcedureReturnTypes, so the result set is ignored.");
                }

                // ** Pick the return type and the command that runs the SPROC
                string returnType;
                string commandLine;
                if (readsResultSet)
                {
                    returnType = $"List<{sprocItem.CSharpReturnType}>";
                    commandLine = COMMAND_LINE_TABLE.Replace("<@RETURN_TYPE@>", sprocItem.CSharpReturnType) + BuildOutputAssignments(outputParams);
                }
                else if (outputParams.Count == 1)
                {
                    // ** A single OUTPUT parameter is the value the call returns
                    var outputParam = outputParams[0];
                    var outputType = outputParam.SysType;
                    returnType = outputType;
                    commandLine = COMMAND_LINE_NON_QUERY
                        + BuildOutputAssignments(outputParams)
                        + Environment.NewLine
                        + (IsValueType(outputType)
                            ? $"\t\t\tvar result = {GetPropertyName(outputParam)} ?? default({outputType});"
                            : $"\t\t\tvar result = {GetPropertyName(outputParam)};");
                }
                else
                {
                    returnType = "int";
                    commandLine = COMMAND_LINE_VOID + BuildOutputAssignments(outputParams);
                }

                var template = headerTemplate;

                // ** Only keep the UDDTs using line when the schema has UDDTs (otherwise the namespace does not exist)
                if (_uddtSchemas != null && !_uddtSchemas.Contains(sprocItem.SchemaName))
                {
                    template = UddtUsingLine.Replace(template, string.Empty);
                }

                var viewStringBuilder = template
                    .Replace("<COMMAND_LINE_EXEC>", commandLine)
                    .Replace("<NAMESPACE>", nameSpaceString)
                    .Replace("<CLASS_NAME>", className)
                    .Replace("<INHERITANCE_PART>", string.Empty)
                    .Replace("<@RETURN_TYPE@>", returnType)
                    .Replace("<SCHEMA>", sprocItem.SchemaName)
                    .Replace("<SCHEMA.SPROC_NAME>", $"[{sprocItem.SchemaName}].[{sprocItem.Name}]")
                    .Replace("<PARAMETERS_INIT_LIST>", BuildParameters(sprocItem))
                    .Replace("<METHOD_PARAMS_LIST_INIT>", BuildParamList(sprocItem, null, true, includeOutputOnly: false))
                    .Replace("<METHOD_PARAMS_LIST>", BuildParamList(sprocItem, "Param"))
                    .Replace("<METHOD_PARAMS_WITH_TYPES>", BuildMethodParams(sprocItem))
                    .Replace("<SPROC_PARAM_LIST>", BuildSprocParamList(sprocItem));

                viewStringBuilder = AddOutputProperties(viewStringBuilder, className, outputParams);

                SaveSprocDefinitionToFile(sprocItem, viewStringBuilder);
            }
        }

        public string BuildParameters(SP sprocItem)
        {
            var sb = new StringBuilder();

            foreach (var param in GetOrderedParameters(sprocItem))
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                var sqlParamName = param.Name;

                if (IsOutputOnly(param))
                {
                    // ** OUTPUT parameter: typed, no value, Direction = Output
                    sb.AppendLine($"\t\t\tvar {paramName}Param = new SqlParameter(\"{sqlParamName}\", {GetSqlDbType(param)}) {{ {BuildOutputParameterSettings(param, System.Data.ParameterDirection.Output)} }};");
                    continue;
                }

                var value = NeedsDbNullFallback(param) ? $"(object){paramName} ?? DBNull.Value" : paramName;

                if (IsOutput(param))
                {
                    // ** OUTPUT parameter the caller also passes a value for
                    sb.AppendLine($"\t\t\tvar {paramName}Param = new SqlParameter(\"{sqlParamName}\", {GetSqlDbType(param)}) {{ {BuildOutputParameterSettings(param, System.Data.ParameterDirection.InputOutput)}, Value = {value} }};");
                    continue;
                }

                sb.AppendLine($"\t\t\tvar {paramName}Param = new SqlParameter(\"{sqlParamName}\", {value});");
            }

            return sb.ToString();
        }

        public string BuildParamList(SP sprocItem, string suffix = null, bool showPrecedingComma = false, bool includeOutputOnly = true)
        {
            var sb = new StringBuilder();

            foreach (var param in GetOrderedParameters(sprocItem).Where(p => includeOutputOnly || !IsOutputOnly(p)))
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                if (sb.Length == 0 && showPrecedingComma)
                {
                    sb.Append($"{paramName}{suffix}");
                    continue;
                }
                sb.Append($", {paramName}{suffix}");
            }

            return sb.ToString();
        }

        public string BuildMethodParams(SP sprocItem)
        {
            var sb = new StringBuilder();

            // ** OUTPUT-only parameters are not arguments: their values come back from the call
            foreach (var param in GetOrderedParameters(sprocItem).Where(p => !IsOutputOnly(p)))
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                var sysType = param.IsOptional ? $"{param.SysType}?" : param.SysType;
                if (sb.Length == 0)
                {
                    sb.Append($"{sysType} {paramName}");
                    continue;
                }
                sb.Append($", {sysType} {paramName}");
            }

            return sb.ToString();
        }

        public string BuildSprocParamList(SP sprocItem)
        {
            var sb = new StringBuilder();
            foreach (var param in GetOrderedParameters(sprocItem))
            {
                var paramText = IsOutput(param) ? $"{param.Name} OUTPUT" : param.Name;
                if (sb.Length == 0)
                {
                    sb.Append(paramText);
                    continue;
                }
                sb.Append($", {paramText}");
            }
            return sb.ToString();
        }

        private static IEnumerable<SPParam> GetOrderedParameters(SP sprocItem)
        {
            // ** RETURN values are not parameters of the EXEC statement
            return sprocItem.Parameters
                .Where(p => p.Mode != System.Data.ParameterDirection.ReturnValue)
                .OrderBy(p => p.OrdinalPosition);
        }

        private static List<SPParam> GetOutputParameters(SP sprocItem)
        {
            return GetOrderedParameters(sprocItem).Where(IsOutput).ToList();
        }

        /// <summary>
        /// SQL Server reports every OUTPUT parameter as INOUT.
        /// </summary>
        private static bool IsOutput(SPParam param)
        {
            return param.Mode == System.Data.ParameterDirection.Output || param.Mode == System.Data.ParameterDirection.InputOutput;
        }

        /// <summary>
        /// An OUTPUT parameter with a default (e.g. "@Id BIGINT = NULL OUTPUT") only returns a value, so it is not a method argument.
        /// An OUTPUT parameter without a default must be passed in, so it stays an argument and is sent as InputOutput.
        /// </summary>
        private static bool IsOutputOnly(SPParam param)
        {
            return IsOutput(param) && (param.IsOptional || !string.IsNullOrEmpty(param.DefaultValue));
        }

        private static bool NeedsDbNullFallback(SPParam param)
        {
            // ** A null value would drop the parameter from the call, so send DBNull instead
            return param.IsOptional || !IsValueType(param.SysType);
        }

        private static bool IsValueType(string sysType)
        {
            return sysType != "string" && sysType != "byte[]" && sysType != "object";
        }

        private static string GetPropertyName(SPParam param)
        {
            var name = param.CleanName;
            return string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);
        }

        private static string GetPropertyType(SPParam param)
        {
            // ** Nullable for both value and reference types: the value is null until the SPROC runs
            return $"{param.SysType}?";
        }

        private static string GetSqlDbType(SPParam param)
        {
            var dbType = (param.DbType ?? string.Empty).ToLowerInvariant();
            switch (dbType)
            {
                case "numeric":
                    return "System.Data.SqlDbType.Decimal";
                case "sysname":
                    return "System.Data.SqlDbType.NVarChar";
            }

            return Enum.TryParse(dbType, true, out SqlDbType sqlDbType)
                ? $"System.Data.SqlDbType.{sqlDbType}"
                : "System.Data.SqlDbType.Variant";
        }

        private static string BuildOutputParameterSettings(SPParam param, System.Data.ParameterDirection direction)
        {
            var settings = new List<string> { $"Direction = System.Data.ParameterDirection.{direction}" };
            var dbType = (param.DbType ?? string.Empty).ToLowerInvariant();

            if (param.MaxLength.HasValue && param.MaxLength.Value != 0 && (dbType.Contains("char") || dbType.Contains("binary")))
            {
                settings.Add($"Size = {param.MaxLength.Value}");
            }

            if ((dbType == "decimal" || dbType == "numeric") && param.Precision.HasValue)
            {
                settings.Add($"Precision = {param.Precision.Value}");
                settings.Add($"Scale = {param.Scale ?? 0}");
            }

            return string.Join(", ", settings);
        }

        private static string BuildOutputAssignments(List<SPParam> outputParams)
        {
            if (outputParams.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var param in outputParams)
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                sb.Append(Environment.NewLine);
                sb.Append($"\t\t\t{GetPropertyName(param)} = {paramName}Param.Value == DBNull.Value ? null : ({param.SysType}){paramName}Param.Value;");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Adds a property per OUTPUT parameter right after the class constructor, holding the value of the last call.
        /// </summary>
        private static string AddOutputProperties(string classText, string className, List<SPParam> outputParams)
        {
            if (outputParams.Count == 0) return classText;

            var constructorRegex = new Regex($@"(public {Regex.Escape(className)}\(DbContext dbContext\)\s*\{{[^}}]*\}}\r?\n)");
            var match = constructorRegex.Match(classText);
            if (!match.Success)
            {
                LogInternalWarning(ClassTypes.Procedure, $"The SPROC class {className} has OUTPUT parameters but the constructor was not found in the template, so no OUTPUT properties were added.");
                return classText;
            }

            var newLine = match.Value.EndsWith("\r\n") ? "\r\n" : "\n";
            var sb = new StringBuilder();
            foreach (var param in outputParams)
            {
                sb.Append(newLine);
                sb.Append($"    /// <summary>Value of the {param.Name} OUTPUT parameter from the last call.</summary>{newLine}");
                sb.Append($"    public {GetPropertyType(param)} {GetPropertyName(param)} {{ get; private set; }}{newLine}");
            }

            return classText.Substring(0, match.Index + match.Length) + sb + classText.Substring(match.Index + match.Length);
        }
    }
}
