namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class NOAHCriticalDataGenerator : GeneratorBase, IGenerator
    {
        private readonly List<CriticalDataType> _criticalDataTypesList;

        public NOAHCriticalDataGenerator(List<CriticalDataType> criticalDataTypesList)
        {
            _criticalDataTypesList = criticalDataTypesList;
        }

        public void Generate(int counter = 0)
        {
            // Group critical data types by schema
            var groupedBySchema = _criticalDataTypesList
                .GroupBy(cdt => cdt.Schema)
                .ToDictionary(group => group.Key, group => group.ToList());

            // Process each schema group
            foreach (var schemaGroup in groupedBySchema)
            {
                var schemaName = schemaGroup.Key;
                var criticalDataTypesInSchema = schemaGroup.Value;

                // Process the critical data types for this schema
                ProcessSchemaGroup(counter, schemaName, criticalDataTypesInSchema);
            }
        }

        private void ProcessSchemaGroup(int counter, string schemaName, List<CriticalDataType> criticalDataTypes)
        {
            // Group by tables
            var groupedByTable = criticalDataTypes
                .GroupBy(cdt => cdt.TableName)
                .ToDictionary(group => group.Key, group => group.ToList());

            Console.WriteLine($"Processing {criticalDataTypes.Count} critical data types in schema: {schemaName}");

            foreach (var proxyDataTypeCriticalDataType in groupedByTable)
            {
                ProcessTableGroup(counter, proxyDataTypeCriticalDataType.Value);
            }
        }

        private void ProcessTableGroup(int counter, List<CriticalDataType> proxyDataTypeCriticalDataType)
        {
            // Read in Proxy Type template
            var proxyTypeTemplate = _templateManager.GetProxyTypeTemplatePrimer();
            var fileName = string.Empty;
            var schema = string.Empty;
            var proxyTypeStringBuilder = string.Empty;
            var enumStringBuilder = new StringBuilder().Append("\r\n");
            var dictionaryStringBuilder = new StringBuilder().Append("\r\n");

            foreach (var criticalDataType in proxyDataTypeCriticalDataType)
            {
                fileName = $"{criticalDataType.TableNameSingular}ProxyLookup";

                // Build proxy type string
                if (string.IsNullOrEmpty(proxyTypeStringBuilder))
                {
                    schema = criticalDataType.Schema;
                    proxyTypeStringBuilder = proxyTypeTemplate
                    .Replace("<FILENAME>", $"{fileName}.generated.cs")
                    .Replace("<NAMESPACE>", $"{DataDataNameSpace}.Utilities.{criticalDataType.Schema}")
                    .Replace("<PROXY_TYPE_NAME>", criticalDataType.Name)
                    .Replace("<PROXY_TYPE_DESCRIPTION>", criticalDataType.Description)
                    .Replace("<PROXY_TYPE_SCHEMA>", criticalDataType.Schema)
                    .Replace("<PROXY_TYPE_TABLE_NAME>", criticalDataType.TableName)
                    .Replace("<CLASS_NAME>", criticalDataType.TableNameSingular);

                    // ** Print proxy type info
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"|** {counter:D4}|-> Proxy Type Name: {criticalDataType.Name,-50} | Table Name: {criticalDataType.TableName,-50}");
                }

                // Build enum string
                enumStringBuilder.Append($"\t\t{criticalDataType.Prefix} = {criticalDataType.Id},\r\n");

                // Build dictionary string
                dictionaryStringBuilder.Append($"\t\t{{ \"{criticalDataType.Prefix}\", {criticalDataType.Id} }}, // {criticalDataType.Description}\r\n");
            }

            // Finalize proxy type string
            proxyTypeStringBuilder = proxyTypeStringBuilder
                .Replace("<ENUM_DEFINITIONS>", enumStringBuilder.ToString().TrimEnd())
                .Replace("<DICTIONARY_DEFINITIONS>", dictionaryStringBuilder.ToString().TrimEnd());

            SaveProxyTypeDefinitionToFile(fileName, schema, proxyTypeStringBuilder.ToString());
        }
    }
}
