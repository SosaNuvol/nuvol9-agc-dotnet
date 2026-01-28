namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Linq;

    public class NOAHUDTypesGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<UserDefinedType> _rmTypes;

        public NOAHUDTypesGenerator(IOrderedEnumerable<UserDefinedType> rmTypes)
        {
            _rmTypes = rmTypes;
        }

        public void Generate(int counter = 0)
        {
            // Read in headerTemplate
            var headerTemplate = _templateManager.GetUddtTemplatePrimer();

            // ** Print results
            Console.WriteLine($"|*** Count of uddts: {_rmTypes.LongCount()}");

            foreach (var uddType in _rmTypes)
            {
                // ** Init
                counter++;
                var viewName = uddType.Name;
                var className = uddType.ClassName;
                var nameSpaceString = $"{DataDataNameSpace}.UDDTs.{uddType.Schema}";

                var viewStringBuilder = headerTemplate
                    .Replace("<NAMESPACE>", nameSpaceString)
                    .Replace("<CLASS_ANNOTATION>", "[Keyless]")
                    .Replace("<CLASS_NAME>", className)
                    .Replace("<INHERITANCE_PART>", string.Empty);

                var definitionString = BuildColumns(uddType);

                viewStringBuilder = viewStringBuilder.Replace("<TYPE_DEFINITIONS>", definitionString);

                SaveUDDTDefinitionToFile(uddType, viewStringBuilder.ToString());
            }
        }
    }
}
