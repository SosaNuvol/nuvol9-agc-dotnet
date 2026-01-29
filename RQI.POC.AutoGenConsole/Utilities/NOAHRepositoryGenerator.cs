namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public class NOAHRepositoryGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<Table> _tables;

        private string _dbContextClassName = null;
        public string DbContextClassName
        {
            get
            {
                if (this._dbContextClassName == null)
                {
                    _dbContextClassName = ConfigurationManager.AppSettings["RQI.GEN.DbContextFileName"];
                    if (string.IsNullOrEmpty(_dbContextClassName))
                    {
                        _dbContextClassName = "AppDbContext";
                    }
                }
                return this._dbContextClassName;
            }
        }

        public NOAHRepositoryGenerator(IOrderedEnumerable<Table> tables)
        {
            _tables = tables;
        }

        public void Generate(int counter = 0)
        {
            var interfaceTemplate = _templateManager.GetRepositoryInterfaceTemplatePrimer();
            var repositoryTemplate = _templateManager.GetRepositoryTemplatePrimer();

            if (string.IsNullOrEmpty(interfaceTemplate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Repository Interface template not found!");
                return;
            }

            if (string.IsNullOrEmpty(repositoryTemplate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Repository template not found!");
                return;
            }

            Console.WriteLine($"|*** Count of tables: {_tables.LongCount()}");

            foreach (var table in _tables)
            {
                counter++;
                GenerateRepository(table, interfaceTemplate, repositoryTemplate, counter);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|*** Generated {counter} Repository classes with interfaces");
        }

        private void GenerateRepository(Table table, string interfaceTemplate, string repositoryTemplate, int counter)
        {
            var className = AutoGen.Utilities.Inflector.MakeSingular(table.CleanName);
            var pluralClassName = AutoGen.Utilities.Inflector.MakePlural(className);
            var primaryKey = GetPrimaryKeyColumn(table);
            var primaryKeyType = primaryKey?.SysType ?? "int";

            // Build Interface
            var interfaceNamespace = $"{DataDataNameSpace}.Interfaces.{table.Schema}";
            var interfaceString = interfaceTemplate
                .Replace("<NAMESPACE>", interfaceNamespace)
                .Replace("<CLASS_NAME>", className)
                .Replace("<PLURAL_CLASS_NAME>", pluralClassName)
                .Replace("<PRIMARY_KEY_TYPE>", primaryKeyType)
                .Replace("<MODEL_NAMESPACE>", $"{DataDataNameSpace}.Models.{table.Schema}")
                .Replace("<SCHEMA>", table.Schema);

            // Build Repository Implementation
            var repositoryNamespace = $"{DataDataNameSpace}.Repositories.{table.Schema}";
            var dbContextNamespace = DataDataNameSpace;
            var repositoryString = repositoryTemplate
                .Replace("<NAMESPACE>", repositoryNamespace)
                .Replace("<CLASS_NAME>", className)
                .Replace("<PLURAL_CLASS_NAME>", pluralClassName)
                .Replace("<PRIMARY_KEY_TYPE>", primaryKeyType)
                .Replace("<MODEL_NAMESPACE>", $"{DataDataNameSpace}.Models.{table.Schema}")
                .Replace("<INTERFACE_NAMESPACE>", interfaceNamespace)
                .Replace("<DBCONTEXT_NAMESPACE>", dbContextNamespace)
                .Replace("<DBCONTEXT_CLASS_NAME>", DbContextClassName)
                .Replace("<SCHEMA>", table.Schema);

            // Save files
            SaveRepositoryInterfaceToFile(table, interfaceString, className);
            SaveRepositoryToFile(table, repositoryString, className);

            // Print info
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|** {counter:D4}|-> Repository: {className}Repository | Schema: {table.Schema} | PK Type: {primaryKeyType}");
        }

        private Column GetPrimaryKeyColumn(Table table)
        {
            return table.Columns.FirstOrDefault(c => c.IsPK);
        }

        private void SaveRepositoryInterfaceToFile(Table table, string interfaceString, string className)
        {
            var fileName = $"I{className}Repository.generated.cs";
            var targetDirectory = $"{_templateManager.DataEntityRootFilePath}\\Repositories\\{table.Schema}";

            if (!System.IO.Directory.Exists(targetDirectory))
            {
                System.IO.Directory.CreateDirectory(targetDirectory);
            }

            var filePath = $"{targetDirectory}\\{fileName}";
            System.IO.File.WriteAllText(filePath, interfaceString);
        }

        private void SaveRepositoryToFile(Table table, string repositoryString, string className)
        {
            var fileName = $"{className}Repository.generated.cs";
            var targetDirectory = $"{_templateManager.DataAccessRootFilePath}\\Repositories\\{table.Schema}";

            if (!System.IO.Directory.Exists(targetDirectory))
            {
                System.IO.Directory.CreateDirectory(targetDirectory);
            }

            var filePath = $"{targetDirectory}\\{fileName}";
            System.IO.File.WriteAllText(filePath, repositoryString);
        }
    }
}
