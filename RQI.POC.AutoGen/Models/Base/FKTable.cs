#region File Header
// <copyright file="FKTable.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:26 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    public class FKTable
    {
        public string ThisSchema { get; set; }

        public string ThisTable { get; set; }

        public string ThisColumn { get; set; }

        public string OtherSchema { get; set; }

        public string OtherTable { get; set; }

        public string OtherColumn { get; set; }

        public string OtherClass { get; set; }

        public string OtherQueryable { get; set; }

        public string PropName { get; set; }

        public string ConstraintName { get; set; }

    }
}