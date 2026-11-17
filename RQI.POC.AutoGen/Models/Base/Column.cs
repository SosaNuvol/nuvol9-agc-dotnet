#region File Header
// <copyright file="Column.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:24 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Data;

    public class Column
    {
        public string Name { get; set; }

        public string CleanName { get; set; }

        public string SysType { get; set; }

        public string DataType { get; set; }

        public DbType DbType { get; set; }

        public bool AutoIncrement { get; set; }

        public bool IsPK { get; set; }

        public int MaxLength { get; set; }

        /// <summary>Precision of a decimal or numeric column (0 when not loaded).</summary>
        public int NumericPrecision { get; set; }

        /// <summary>Scale of a decimal or numeric column.</summary>
        public int NumericScale { get; set; }

        /// <summary>
        /// The store type for the [Column] attribute. Decimal and numeric columns carry their precision and scale,
        /// because a bare "decimal" makes EF send decimal(18,0) and round every fraction away.
        /// </summary>
        public string ColumnTypeName
        {
            get
            {
                if ((DataType == "decimal" || DataType == "numeric") && NumericPrecision > 0)
                {
                    return $"{DataType}({NumericPrecision},{NumericScale})";
                }

                return DataType;
            }
        }

        public bool IsNullable { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsEnum { get; set; }

        public bool IsComputed { get; set; }

        public string ColumnAttributeText { get; set; }

        public bool IsReadOnly { get; set; }

        public string DefaultSetting { get; set; }

        public string ForeignKeyTableName { get; set; }

        public string ForeignKeyTableSchemaName { get; set; }

        public string ForeignKeyClassName { get; set; }

        public string TableName { get; set; }

        public string TableSchema { get; set; }

        public bool IsHistorical { get; set; }

        public bool IsAuditColumn { get; set; }

        public string DataTypeComputed
        {
            get
            {
                return GenerateComputedDataType();
            }
        }

        private string GenerateComputedDataType()
        {
            switch (DataType)
            {
                case "varchar":
                    return $"VARCHAR({MaxLength})";

                case "nvarchar":
                    return $"NVARCHAR({GetStringLength(MaxLength)})";

                case "int":
                    return $"INT";

                case "bigint":
                    return $"BIGINT";

                default:
                    return DataType;
            }
        }

        private string GetStringLength(int MaxLength)
        {
            if (MaxLength == -1)
            {
                return "MAX";
            }
            else
            {
                return MaxLength.ToString();
            }
        }
    }
}