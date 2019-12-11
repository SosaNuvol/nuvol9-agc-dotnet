#region File Header
// <copyright file="ForeignKeyRow.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-10-3:55 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    public class ForeignKeyRow
    {
        public string ThisTable { get; set; }
        public string ThisColumn { get; set; }
        public string ThisTableSchema { get; set; }

        public string OtherTableSchema { get; set; }
        public string OtherTable { get; set; }
        public string OtherColumn { get; set; }

        public string ConstraintName { get; set; }
        public string Owner { get; set; }
    }
}