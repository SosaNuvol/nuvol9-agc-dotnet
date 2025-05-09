namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System.Linq;

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

                var viewStringBuilder = headerTemplate;
                //.Replace("<NAMESPACE>", nameSpaceString)
                //.Replace("<CLASS_ANNOTATION>", classAnnotation)
                //.Replace("<CLASS_NAME>", className)
                //.Replace("<INHERITANCE_PART>", classInheretsFrom);

                SaveSprocDefinitionToFile(sprocItem, viewStringBuilder.ToString());
            }
        }
    }
}
