namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using RQI.POC.AutoGenConsole.Templates;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class NOAHTablesGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<Table> _rmTables;
        private readonly TemplateManager _templateManager;
        private readonly List<string> _foreignModelsUsing;

        public NOAHTablesGenerator(IOrderedEnumerable<Table> rmTables)
        {
            _rmTables = rmTables;
            _templateManager = new TemplateManager();
            _foreignModelsUsing = new List<string>();
        }

        public void Generate(int counter = 0)
        {
            // Read in headerTemplate
            var headerTemplate = _templateManager.GetTableTemplatePrimer();

            Console.Write(headerTemplate);

            // ** Print results
            Console.WriteLine($"|*** Count of tables: {_rmTables.LongCount()}");

            // ** Loop through all tables
            foreach (var tableItem in _rmTables)
            {
                // ** Init
                counter++;
                var tableName = tableItem.Name;
                var className = AutoGen.Utilities.Inflector.MakeSingular(tableItem.CleanName);
                var nameSpaceString = $"{DataAccessNameSpace}.Models.{tableItem.Schema}";
                var classAnnotation = $"[Table(\"{tableName}\", Schema = \"{tableItem.Schema}\")]";
                var classInheretsFrom = TableIsHistorical(tableItem)
                    ? " : HistoricalEntity"
                    : null;

                var tableStringBuilder = headerTemplate
                    .Replace("<NAMESPACE>", nameSpaceString)
                    .Replace("<CLASS_ANNOTATION>", classAnnotation)
                    .Replace("<CLASS_NAME>", className)
                    .Replace("<INHERITANCE_PART>", classInheretsFrom);

                tableStringBuilder = tableStringBuilder.Replace("<TABLE_COLUMN_PROPERTIES>", BuildColumns(tableItem, out var foreignKeyColumns));
                tableStringBuilder = tableStringBuilder.Replace("<TABLE_FOREIGN_KEYS>", BuildForeighKeys(foreignKeyColumns));

                SaveTableDefinitionToFile(tableItem, tableStringBuilder.ToString());

                //// ** Print table info
                //Console.ForegroundColor = ConsoleColor.Green;
                //Console.WriteLine($"|** {counter:D4}|-> Table Name: {tableName,-50} | Columns: {tableItem.Columns.Count:D3}");

                //// ** Print Columns
                //Console.ForegroundColor = ConsoleColor.Blue;
                //foreach (var tableItemColumn in tableItem.Columns)
                //{
                //    Console.WriteLine($"|** ** Column Name {tableItemColumn.Name,-50} | Column C# Type: {tableItemColumn.DbType,-22} | Column DB Type: {tableItemColumn.DataType.ToUpper(),-22} | Column Length: {tableItemColumn.MaxLength}");
                //}
            }
        }

        private void SaveTableDefinitionToFile(Table table, string tableString)
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

        public bool TableIsHistorical(Table table)
        {
            return table.Columns.Any(column => column.Name.Equals("IsActive", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("ModifiedDate", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("ModifiedById", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("CreatedDate", StringComparison.OrdinalIgnoreCase))
                && table.Columns.Any(column => column.Name.Equals("CreatedById", StringComparison.OrdinalIgnoreCase));
        }
    }
}
