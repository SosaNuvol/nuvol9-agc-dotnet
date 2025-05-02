namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using RQI.POC.AutoGenConsole.Templates;
    using System;
    using System.Configuration;
    using System.Linq;

    public partial class GeneratorBase
    {
        protected readonly TemplateManager _templateManager;

        private string _dataAccessNameSpace = null;
        public string DataAccessNameSpace
        {
            get
            {
                if (this._dataAccessNameSpace == null)
                {
                    _dataAccessNameSpace = ConfigurationManager.AppSettings["RQI.GEN.DataAccessNameSpace"];
                }
                return this._dataAccessNameSpace;
            }
        }

        public GeneratorBase()
        {
            _templateManager = new TemplateManager();
        }

        public bool TableIsHistorical(Table table)
        {
            return table.Columns.Any(column => column.Name.Equals("IsActive", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("ModifiedById", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("CreatedById", StringComparison.OrdinalIgnoreCase));
        }

        protected void SaveTableDefinitionToFile(Table table, string tableString)
        {
            var fileName = $"{AutoGen.Utilities.Inflector.MakeSingular(table.CleanName)}.generated.cs";
            var targetDirectory = $"{_templateManager.DataAccessRootFilePath}\\Models\\{table.Schema}";

            // ** Check if directory exists
            if (!System.IO.Directory.Exists(targetDirectory))
            {
                // ** Create directory
                System.IO.Directory.CreateDirectory(targetDirectory);
            }

            var filePath = $"{targetDirectory}\\{fileName}";

            // ** Save to file
            System.IO.File.WriteAllText(filePath, tableString);
        }

    }
}
