namespace RQI.POC.AutoGenConsole.Utilities
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public partial class NOAHTablesGenerator
    {
        public string BuildColumns(Table tableItem, out List<Column> foreignKeyColumns)
        {
            var sb = new StringBuilder();
            foreignKeyColumns = new List<Column>();

            foreach (var column in tableItem.Columns)
            {
                if (column.IsHistorical) continue;

                sb.AppendLine("");
                if (column.IsForeignKey)
                {
                    foreignKeyColumns.Add(column);
                    sb.AppendLine($"\t[ForeignKey(\"{column.ForeignKeyClassName}\")]");
                }
                if (column.AutoIncrement)
                {
                    sb.AppendLine($"\t[DatabaseGenerated(DatabaseGeneratedOption.Identity){AddKey(column.IsPK)}]");
                }
                else if (column.IsPK)
                {
                    sb.AppendLine($"\t[Key]");
                }
                sb.AppendLine($"\t[Column(\"{column.Name}\", TypeName=\"{column.DataType}\"){AddRequired(!column.IsNullable)}]");
                sb.AppendLine($"\tpublic virtual {column.SysType} {column.CleanName} {{ get; set; }}");
            }

            return sb.ToString();
        }

        private string AddKey(bool isPK)
        {
            return isPK ? ", Key" : string.Empty;
        }

        private string AddRequired(bool isRequired)
        {
            return isRequired ? ", Required" : string.Empty;
        }

        public string BuildForeighKeys(List<Column> foreignKeyColumns, string tableSchema)
        {
            var sb = new StringBuilder();
            if (foreignKeyColumns.Count > 0)
            {
                sb.AppendLine("\t#region Foreign Keys");
                foreach (var foreignKeyColumn in foreignKeyColumns)
                {
                    sb.AppendLine($"\tpublic virtual {GetSchemaName(tableSchema, foreignKeyColumn.ForeignKeyTableSchemaName)}{foreignKeyColumn.ForeignKeyClassName} {foreignKeyColumn.ForeignKeyClassName} {{ get; set; }}");
                }
                sb.AppendLine("\t#endregion");
            }
            return sb.ToString();
        }

        private string GetSchemaName(string tableSchema, string fkSchema)
        {
            if (tableSchema.Equals(fkSchema, StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }
            else
            {
                return $"{fkSchema}.";
            }
        }
    }
}