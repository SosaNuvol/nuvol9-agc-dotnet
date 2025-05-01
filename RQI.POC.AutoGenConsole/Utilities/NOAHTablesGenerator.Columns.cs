namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System.Collections.Generic;
    using System.Text;

    public partial class NOAHTablesGenerator
    {
        public string BuildColumns(Table tableItem, out List<Column> foreignKeyColumns)
        {
            var sb = new StringBuilder();
            foreignKeyColumns = new List<Column>();

            return sb.ToString();
        }
    }
}