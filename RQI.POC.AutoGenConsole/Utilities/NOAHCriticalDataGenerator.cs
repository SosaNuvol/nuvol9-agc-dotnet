namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System.Collections.Generic;
    using System.Linq;

    internal class NOAHCriticalDataGenerator : GeneratorBase, IGenerator
    {
        private List<CriticalDataType> _criticalDataTypesList;

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
                ProcessSchemaGroup(schemaName, criticalDataTypesInSchema);
            }
        }

        private void ProcessSchemaGroup(string schemaName, List<CriticalDataType> criticalDataTypes)
        {
            // Read in headerTemplate
            var headerTemplate = _templateManager.GetTableTemplatePrimer();

            System.Console.WriteLine($"Processing {criticalDataTypes.Count} critical data types in schema: {schemaName}");
        }
    }
}
