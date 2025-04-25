namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using RQI.POC.AutoGenConsole.Library;
    using RQI.POC.AutoGenConsole.Templates;
    using System;
    using System.Linq;

    public class NOAHTablesGenerator : GeneratorBase, IGenerator
    {
        private readonly IOrderedEnumerable<AutoGen.Models.Base.Table> _rmTables;
        private readonly TemplateManager _templateManager;

        public NOAHTablesGenerator(IOrderedEnumerable<AutoGen.Models.Base.Table> rmTables)
        {
            _rmTables = rmTables;
            _templateManager = new TemplateManager();
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
                var className = Inflector.MakeSingular(tableItem.CleanName);
                var nameSpaceString = $"{DataAccessNameSpace}.Models.{tableItem.Schema}";
                var tableStringBuilder = headerTemplate.Replace("<NAMESPACE>", nameSpaceString);
                counter++;
                var tableName = $"[{tableItem.Schema}].[{tableItem.Name}]";

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
