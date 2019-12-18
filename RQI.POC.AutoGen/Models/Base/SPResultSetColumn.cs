#region File Header
// <copyright file="SPResultSetColumn.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-18-2:14 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Data;

    public class SPResultSetColumn
    {
        public string Name { get; set; }

        public string ClassName { get; set; }

        public string SysType { get; set; }

        public string DataType { get; set; }

        public DbType DbType { get; set; }

        public short MaxLength { get; set; }

        public short Precision { get; set; }

        public short Scale { get; set; }

        public int ColumnOrdinal { get; set; }

        public bool IsNullable { get; set; }
    }
}