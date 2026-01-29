namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using RQI.POC.AutoGenConsole.Templates;
    using System;
    using System.Linq;

    public partial class GeneratorBase
    {
        protected readonly TemplateManager _templateManager;

        private string _dataDataNameSpace = null;
        public string DataDataNameSpace
        {
            get
            {
                if (this._dataDataNameSpace == null)
                {
                    _dataDataNameSpace = _templateManager.DataDataNameSpace;
                }
                return this._dataDataNameSpace;
            }
        }

        private string _dbContextNameSpace = null;
        public string DbContextNameSpace
        {
            get
            {
                if (this._dbContextNameSpace == null)
                {
                    _dbContextNameSpace = _templateManager.DbContextNamespance;
                }
                return this._dbContextNameSpace;
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
            var targetDirectory = GetTargetDirectory(table);

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

        protected void SaveProxyTypeDefinitionToFile(string typeName, string schema, string proxyTypeString)
        {
            var fileName = $"{typeName}.generated.cs";
             var targetDirectory = $"{_templateManager.ProxyTypeRootFilePath}\\{schema}";
            // ** Check if directory exists
            if (!System.IO.Directory.Exists(targetDirectory))
            {
                // ** Create directory
                System.IO.Directory.CreateDirectory(targetDirectory);
            }
            var filePath = $"{targetDirectory}\\{fileName}";
            // ** Save to file
            System.IO.File.WriteAllText(filePath, proxyTypeString);
        }

        protected void SaveUDDTDefinitionToFile(UserDefinedType userDefinedType, string userDefinedTypeString)
        {
            var fileName = $"{userDefinedType.ClassName}.generated.cs";
            var targetDirectory = GetTargetDirectory(userDefinedType);
            
            // ** Check if directory exists
            if (!System.IO.Directory.Exists(targetDirectory))
            {
                // ** Create directory
                System.IO.Directory.CreateDirectory(targetDirectory);
            }
         
            var filePath = $"{targetDirectory}\\{fileName}";
            
            // ** Save to file
            System.IO.File.WriteAllText(filePath, userDefinedTypeString);
        }

        protected void SaveSprocDefinitionToFile(SP sproc, string sprocString)
        {
            var fileName = $"{AutoGen.Utilities.Inflector.MakeSPROCClassName(sproc.CleanName)}.generated.cs";
            var targetDirectory = GetTargetDirectory(sproc);
            // ** Check if directory exists
            if (!System.IO.Directory.Exists(targetDirectory))
            {
                // ** Create directory
                System.IO.Directory.CreateDirectory(targetDirectory);
            }
            var filePath = $"{targetDirectory}\\{fileName}";
            // ** Save to file
            System.IO.File.WriteAllText(filePath, sprocString);
        }

        protected string GetTargetDirectory(object value, string schema = null)
        {
            var folder1 = "\\Models";
            var targetDirectory = $"{_templateManager.DataModelsRootFilePath}";

            if (value is Table)
            {
                var table = (Table)value;
                schema = table.Schema;
                folder1 = "\\Models";
            }
            else if (value is UserDefinedType)
            {
                var userDefinedType = (UserDefinedType)value;
                schema = userDefinedType.Schema;
                folder1 = "\\UDDTs";
            }
            else if (value is SP)
            {
                var sproc = (SP)value;
                schema = sproc.SchemaName;
                folder1 = "\\SPROCS";
                targetDirectory = $"{_templateManager.DataAccessRootFilePath}";
            }

            return $"{targetDirectory}{folder1}\\{schema}";
        }

    }
}
