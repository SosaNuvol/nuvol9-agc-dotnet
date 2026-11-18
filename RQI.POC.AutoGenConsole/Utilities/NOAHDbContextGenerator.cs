namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Text;

    public class NOAHDbContextGenerator : GeneratorBase, IGenerator
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
                        _dbContextClassName = "AppDbContext"; // Default value
                    }
                }
                return this._dbContextClassName;
            }
        }

        private string _connectionStringName = null;
        public string ConnectionStringName
        {
            get
            {
                if (this._connectionStringName == null)
                {
                    _connectionStringName = ConfigurationManager.AppSettings["RQI.GEN.DbContextConnectionStringName"];
                    if (string.IsNullOrEmpty(_connectionStringName))
                    {
                        _connectionStringName = "GBDCDbContext"; // Default value
                    }
                }
                return this._connectionStringName;
            }
        }

        public NOAHDbContextGenerator(IOrderedEnumerable<Table> tables)
        {
            _tables = tables;
        }

        public void Generate(int counter = 0)
        {
            // Get the DbContext template
            var template = _templateManager.GetDbContextTemplatePrimer();

            if (string.IsNullOrEmpty(template))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: DbContext template not found!");
                return;
            }

            // Group tables by schema to organize using statements
            var schemaGroups = _tables.GroupBy(t => t.Schema).OrderBy(g => g.Key);

            // Build using statements
            var usingStatements = BuildUsingStatements(schemaGroups);

            // Build DbSet properties
            var dbSetProperties = BuildDbSetProperties(schemaGroups);

            // Build model configurations (optional, for future use)
            var modelConfigurations = BuildModelConfigurations();

            // Replace placeholders
            var dbContextString = template
                .Replace("<NAMESPACE>", DbContextNameSpace)
                .Replace("<CLASS_NAME>", DbContextClassName)
                .Replace("<CONNECTION_STRING_NAME>", ConnectionStringName)
                .Replace("<DATABASE_NAME>", "Application Database")
                .Replace("<USING_STATEMENTS>", usingStatements)
                .Replace("<DBSET_PROPERTIES>", dbSetProperties)
                .Replace("<MODEL_CONFIGURATIONS>", modelConfigurations);

            // Save the DbContext file
            SaveDbContextToFile(dbContextString);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"|*** DbContext '{DbContextClassName}' generated with {_tables.Count()} DbSet properties");
        }

        private string BuildUsingStatements(IEnumerable<IGrouping<string, Table>> schemaGroups)
        {
            var sb = new StringBuilder();

            foreach (var schemaGroup in schemaGroups)
            {
                var namespaceName = $"{DataDataNameSpace}.Models.{schemaGroup.Key}";
                sb.AppendLine($"using {namespaceName};");
            }

            return sb.ToString().TrimEnd();
        }

        private string BuildDbSetProperties(IEnumerable<IGrouping<string, Table>> schemaGroups)
        {
            var sb = new StringBuilder();

            foreach (var schemaGroup in schemaGroups.OrderBy(g => g.Key))
            {
                //#region DbSets - ACC (Accounts)
                sb.AppendLine($"    #region DbSets - {schemaGroup.Key} (Schema)");
                sb.AppendLine();

                foreach (var table in schemaGroup.OrderBy(t => t.CleanName))
                {
                    var className = AutoGen.Utilities.Inflector.MakeSingular(table.CleanName);
                    var pluralClassName = AutoGen.Utilities.Inflector.MakePlural(className);

                    sb.AppendLine($"        /// <summary>");
                    sb.AppendLine($"        /// Gets or sets the {pluralClassName} DbSet.");
                    sb.AppendLine($"        /// </summary>");
                    sb.AppendLine($"        public virtual DbSet<{className}> {pluralClassName} {{ get; set; }}");
                    sb.AppendLine();
                }

                sb.AppendLine($"    #endregion DbSets - {schemaGroup.Key} (Schema)");
                sb.AppendLine();
            }

            return sb.ToString().TrimEnd();
        }

        private string BuildModelConfigurations()
        {
            // Placeholder for future entity configurations
            // You can add Fluent API configurations here if needed
            var sb = new StringBuilder();
            sb.AppendLine("            // Add custom model configurations here if needed");
            return sb.ToString().TrimEnd();
        }

        private void SaveDbContextToFile(string dbContextString)
        {
            var fileName = $"{DbContextClassName}.generated.cs";
            var targetDirectory = _templateManager.RepositoryRootFilePath;

            // Check if directory exists
            if (!System.IO.Directory.Exists(targetDirectory))
            {
                System.IO.Directory.CreateDirectory(targetDirectory);
            }

            var filePath = $"{targetDirectory}\\{fileName}";

            // Save to file
            RQI.POC.AutoGen.Utilities.AutoDALFileSystem.WriteGeneratedFile(filePath, dbContextString);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"|*** DbContext saved to: {filePath}");
        }
    }
}
