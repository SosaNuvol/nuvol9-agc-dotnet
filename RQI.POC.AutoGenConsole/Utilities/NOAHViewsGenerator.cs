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
            foreach (var viewItem in _rmViews)
            {
                // ** Init
                counter++;
                var viewName = viewItem.Name;
                var className = AutoGen.Utilities.Inflector.MakeSingular(viewItem.CleanName);
                var nameSpaceString = $"{DataDataNameSpace}.Models.{viewItem.Schema}";
                var classAnnotation = $"[Table(\"{viewName}\", Schema = \"{viewItem.Schema}\")]";
                var classInheretsFrom = TableIsHistorical(viewItem)
                    ? " : HistoricalEntity"
                    : null;

                var viewStringBuilder = headerTemplate
                    .Replace("<NAMESPACE>", nameSpaceString)
                    .Replace("<CLASS_ANNOTATION>", classAnnotation)
                    .Replace("<CLASS_NAME>", className)
                    .Replace("<INHERITANCE_PART>", classInheretsFrom);

                viewStringBuilder = viewStringBuilder.Replace("<TABLE_COLUMN_PROPERTIES>", BuildColumns(viewItem, out var foreignKeyColumns));
                viewStringBuilder = viewStringBuilder.Replace("<TABLE_FOREIGN_KEYS>", BuildForeighKeys(foreignKeyColumns, viewItem.Schema));

                SaveTableDefinitionToFile(viewItem, viewStringBuilder.ToString());

                // ** Loop though all Views
                viewName = $"[{viewItem.Schema}].[{viewItem.Name}]";

                // ** Print view information
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|=> View Name: {viewName,-50} | Columns: {viewItem.Columns.Count:D3}");

                // ** Print Columns
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var tableItemColumn in viewItem.Columns)
                {
                    Console.WriteLine($"|** ** Column Name {tableItemColumn.Name,-50} | Column C# Type: {tableItemColumn.DbType,-22} | Column DB Type: {tableItemColumn.DataType.ToUpper(),-22} | Column Length: {tableItemColumn.MaxLength}");
                }
            }
        }
    }
}
