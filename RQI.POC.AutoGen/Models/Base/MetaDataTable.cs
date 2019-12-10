#region File Header
// <copyright file="MetaDataTable.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:22 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Collections.Generic;

    public class MetaDataTable
    {
        public string IDName { get; set; }

        public string MetaDataName { get; set; }

        public string BaseClassName { get; set; }

        public Dictionary<string, string> Values { get; set; }
    }
}