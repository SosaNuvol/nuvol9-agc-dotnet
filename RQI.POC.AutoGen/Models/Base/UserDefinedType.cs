#region File Header
// <copyright file="Table.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:20 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

using System.Collections.Generic;
using System.Data;

namespace RQI.POC.AutoGen.Models.Base
{
    public class UserDefinedType
    {
        public List<Column> Definitions { get; set; } = new List<Column>();

        public string Name { get; set; }

        public string CleanName { get; set; }

        public string Suffix { get; set; }

        public string Schema { get; set; }

        public bool IsTableType { get; set; }

        public short MaxLength { get; set; }
        
        public byte Precision { get; set; }

        public byte Scale { get; set; }

        public bool IsNullable { get; set; }

        public string SystemType { get; set; }

        public string ClassName
        {
            get
            {
                return $"{Utilities.Inflector.MakeSingular(CleanName)}{Suffix}";
            }
        }

        public string DataType
        {
            get
            {
                var precision = "";
                if (SystemType.Equals("VARCHAR") || SystemType.Equals("NVARCHAR"))
                    precision = $"({MaxLength})";

                return IsTableType 
                    ? "DataTable" 
                    : $"{SystemType}{precision}";
            }
        }

        public string UTSysType
        {
            get
            {
                return Utilities.GetSysType(SystemType);
            }
        }
    }
}
