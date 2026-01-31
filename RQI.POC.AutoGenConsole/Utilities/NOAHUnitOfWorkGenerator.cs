namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class NOAHUnitOfWorkGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<Table> _tables;

        public NOAHUnitOfWorkGenerator(IOrderedEnumerable<Table> tables)
        {
            _tables = tables;
        }

        public void Generate(int counter = 0)
        {
            var interfaceTemplate = _templateManager.GetUnitOfWorkInterfaceTemplatePrimer();
            var unitOfWorkTemplate = _templateManager.GetUnitOfWorkTemplatePrimer();

            if (string.IsNullOrEmpty(interfaceTemplate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: UnitOfWork Interface template not found!");
                return;
            }

            if (string.IsNullOrEmpty(unitOfWorkTemplate))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: UnitOfWork template not found!");
                return;
            }

            Console.WriteLine($"|*** Generating Unit of Work from {_tables.LongCount()} tables");

            // Generate the Unit of Work files
            GenerateUnitOfWork(interfaceTemplate, unitOfWorkTemplate);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|*** Generated UnitOfWork class and interface");
        }

        private void GenerateUnitOfWork(string interfaceTemplate, string unitOfWorkTemplate)
        {
            // Group tables by schema
            var schemaGroups = _tables.GroupBy(t => t.Schema).OrderBy(g => g.Key);

            // Build private repository fields
            var privateFields = BuildPrivateRepositoryFields(schemaGroups);

            // Build public repository properties for the class
            var publicProperties = BuildPublicRepositoryProperties(schemaGroups);

            // Build public repository properties for the interface
            var interfaceProperties = BuildInterfaceRepositoryProperties(schemaGroups);

            // Build the Unit of Work class
            var unitOfWorkString = unitOfWorkTemplate
                .Replace("<FILENAME>", "UnitOfWork.generated.cs")
                .Replace("<PRIVATE_DEF_REPOSITORIES>", privateFields)
                .Replace("<PUBLIC_REPOSITORY_PROPERTIES>", publicProperties);

            // Build the Unit of Work interface
            var interfaceString = interfaceTemplate
                .Replace("<FILENAME>", "IUnitOfWork.generated.cs")
                .Replace("<PUBLIC_REPOSITORY_PROPERTIES_INTERFACE>", interfaceProperties);

            // Save files
            SaveUnitOfWorkToFile(unitOfWorkString);
            SaveUnitOfWorkInterfaceToFile(interfaceString);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"|** Generated UnitOfWork with {_tables.Count()} repositories across {schemaGroups.Count()} schemas");
        }

        private string BuildPrivateRepositoryFields(IEnumerable<IGrouping<string, Table>> schemaGroups)
        {
            var sb = new StringBuilder();

            foreach (var schemaGroup in schemaGroups)
            {
                sb.AppendLine($"    // AUTO GEN-START: {schemaGroup.Key} Repositories");
                sb.AppendLine($"    #region {schemaGroup.Key} Repositories");
                sb.AppendLine();

                foreach (var table in schemaGroup.OrderBy(t => t.CleanName))
                {
                    var className = AutoGen.Utilities.Inflector.MakeSingular(table.CleanName);
                    var propertyName = AutoGen.Utilities.Inflector.MakePlural(className);
                    var privateName = $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";

                    sb.AppendLine($"    private I{className}Repository? {privateName};");
                }

                sb.AppendLine();
                sb.AppendLine($"    #endregion {schemaGroup.Key} Repositories");
                sb.AppendLine($"    // AUTO GEN-END: {schemaGroup.Key} Repositories");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string BuildPublicRepositoryProperties(IEnumerable<IGrouping<string, Table>> schemaGroups)
        {
            var sb = new StringBuilder();

            foreach (var schemaGroup in schemaGroups)
            {
                sb.AppendLine($"    #region {schemaGroup.Key} Repositories");
                sb.AppendLine();

                foreach (var table in schemaGroup.OrderBy(t => t.CleanName))
                {
                    var className = AutoGen.Utilities.Inflector.MakeSingular(table.CleanName);
                    var propertyName = AutoGen.Utilities.Inflector.MakePlural(className);
                    var privateName = $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";

                    sb.AppendLine($"    /// <inheritdoc />");
                    sb.AppendLine($"    public I{className}Repository {propertyName} => {privateName} ??= new {className}Repository(_context);");
                    sb.AppendLine();
                }

                sb.AppendLine($"    #endregion");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string BuildInterfaceRepositoryProperties(IEnumerable<IGrouping<string, Table>> schemaGroups)
        {
            var sb = new StringBuilder();

            foreach (var schemaGroup in schemaGroups)
            {
                sb.AppendLine($"    #region {schemaGroup.Key} Repositories");
                sb.AppendLine();

                foreach (var table in schemaGroup.OrderBy(t => t.CleanName))
                {
                    var className = AutoGen.Utilities.Inflector.MakeSingular(table.CleanName);
                    var propertyName = AutoGen.Utilities.Inflector.MakePlural(className);

                    sb.AppendLine($"    /// <summary>");
                    sb.AppendLine($"    /// Gets the {className} repository.");
                    sb.AppendLine($"    /// </summary>");
                    sb.AppendLine($"    I{className}Repository {propertyName} {{ get; }}");
                    sb.AppendLine();
                }

                sb.AppendLine($"    #endregion");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void SaveUnitOfWorkToFile(string unitOfWorkString)
        {
            var filePath = _templateManager.UnitOfWorkFilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: UnitOfWork file path not configured in App.config!");
                return;
            }

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.WriteAllText(filePath, unitOfWorkString);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|** Saved: {filePath}");
        }

        private void SaveUnitOfWorkInterfaceToFile(string interfaceString)
        {
            var filePath = _templateManager.UnitOfWorkInterfaceFilePath;

            if (string.IsNullOrEmpty(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: UnitOfWork Interface file path not configured in App.config!");
                return;
            }

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.WriteAllText(filePath, interfaceString);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|** Saved: {filePath}");
        }
    }
}
