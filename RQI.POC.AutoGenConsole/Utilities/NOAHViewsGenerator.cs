namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class NOAHViewsGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<Table> _rmViews;
        private readonly List<string> _foreignModelsUsing;

        public NOAHViewsGenerator(IOrderedEnumerable<Table> rmViews)
        {
            _rmViews = rmViews;
            _foreignModelsUsing = new List<string>();
        }

        public void Generate(int counter = 0)
        {
            var headerTemplate = _templateManager.GetViewTemplatePrimer();
            // ** Print results
            Console.WriteLine($"|*** Count of views: {_rmViews.LongCount()}");

            // ** Loop through all tables
            foreach (var tableItem in _rmViews)
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
