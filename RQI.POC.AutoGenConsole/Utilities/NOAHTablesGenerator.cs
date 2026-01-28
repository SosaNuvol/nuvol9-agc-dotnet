namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using RQI.POC.AutoGenConsole.Templates;
    using System;
    using System.Linq;

    public partial class NOAHTablesGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<Table> _rmTables;

        public NOAHTablesGenerator(IOrderedEnumerable<Table> rmTables)
        {
            _rmTables = rmTables;
        }

        public void Generate(int counter = 0)
        {
            // Read in headerTemplate
            var headerTemplate = _templateManager.GetTableTemplatePrimer();

            // ** Print results
            Console.WriteLine($"|*** Count of tables: {_rmTables.LongCount()}");

            // ** Loop through all tables
            foreach (var tableItem in _rmTables)
            {
                // ** Init
                counter++;
                var tableName = tableItem.Name;
                var className = AutoGen.Utilities.Inflector.MakeSingular(tableItem.CleanName);
                var nameSpaceString = $"{DataDataNameSpace}.Models.{tableItem.Schema}";
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
                tableStringBuilder = tableStringBuilder.Replace("<TABLE_FOREIGN_KEYS>", BuildForeighKeys(foreignKeyColumns, tableItem.Schema));

                SaveTableDefinitionToFile(tableItem, tableStringBuilder.ToString());

                // ** Print table info
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|-> Table Name: {tableName,-50} | Columns: {tableItem.Columns.Count:D3}");

                // ** Print Columns
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var tableItemColumn in tableItem.Columns)
                {
                    Console.WriteLine($"|** ** Column Name {tableItemColumn.Name,-50} | Column C# Type: {tableItemColumn.DbType,-22} | Column DB Type: {tableItemColumn.DataType.ToUpper(),-22} | Column Length: {tableItemColumn.MaxLength}");
                }
            }
        }
    }
}
