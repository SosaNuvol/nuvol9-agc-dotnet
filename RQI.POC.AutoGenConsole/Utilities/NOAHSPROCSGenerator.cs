namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System.Linq;
    using System.Text;
    using Unity.Strategies;
    using static RQI.POC.AutoGen.Utilities;

    public class NOAHSPROCSGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<SP> _rmSPROCS;

        public NOAHSPROCSGenerator(IOrderedEnumerable<SP> rmSPROCS)
        {
            _rmSPROCS = rmSPROCS;
        }

        public void Generate(int counter = 0)
        {
            var headerTemplate = _templateManager.GetSPROCTemplatePrimer();

            // ** Loop through SPROCs
            foreach (var sprocItem in _rmSPROCS)
            {
                // ** Init
                counter++;
                var sprocName = sprocItem.Name;
                var className = AutoGen.Utilities.Inflector.MakeSPROCClassName(sprocItem.CleanName);
                var nameSpaceString = $"{DataAccessNameSpace}.Models.{sprocItem.SchemaName}";

                var viewStringBuilder = headerTemplate
                    .Replace("<NAMESPACE>", nameSpaceString)
                    .Replace("<CLASS_NAME>", className)
                    .Replace("<INHERITANCE_PART>", string.Empty)
                    .Replace("<@RETURN_TYPE@>", sprocItem.ReturnsNothing ? "Void": "int")
                    .Replace("<SCHEMA.SPROC_NAME>", $"[{sprocItem.SchemaName}].[{sprocItem.Name}]")
                    .Replace("<PARAMETERS_INIT_LIST>", BuildParameters(sprocItem))
                    .Replace("<METHOD_PARAMS_LIST>", BuildParamList(sprocItem))
                    .Replace("<METHOD_PARAMS_WITH_TYPES>", BuildMethodParams(sprocItem))
                    .Replace("<SPROC_PARAM_LIST>", BuildSprocParamList(sprocItem));

                SaveSprocDefinitionToFile(sprocItem, viewStringBuilder.ToString());
            }
        }

        public string BuildParameters(SP sprocItem)
        {
            var paramConst = "\t\t\tvar <PARAM_NAME>Param = new SqlParameter(\"<SQL_PARAM_NAME>\", <PARAM_NAME>);";
            var sb = new StringBuilder();

            foreach (var param in sprocItem.Parameters)
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                var sqlParamName = param.Name;
                sb.AppendLine(paramConst.Replace("<PARAM_NAME>", paramName).Replace("<SQL_PARAM_NAME>", sqlParamName));
            }

            return sb.ToString();
        }

        public string BuildParamList(SP sprocItem)
        {
            var sb = new StringBuilder();

            foreach (var param in sprocItem.Parameters)
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                var sysType = param.SysType;
                if (sb.Length == 0)
                {
                    sb.Append($"{paramName}");
                    continue;
                }
                sb.Append($", {paramName}");
            }

            return sb.ToString();
        }

        public string BuildMethodParams(SP sprocItem)
        {
            var sb = new StringBuilder();

            foreach (var param in sprocItem.Parameters)
            {
                var paramName = Inflector.MakeCamelCase(param.CleanName);
                var sysType = param.SysType;
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
            foreach (var param in sprocItem.Parameters)
            {
                if (sb.Length == 0)
                {
                    sb.Append($"{param.Name}");
                    continue;
                }
                sb.Append($", {param.Name}");
            }
            return sb.ToString();
        }
    }
}
