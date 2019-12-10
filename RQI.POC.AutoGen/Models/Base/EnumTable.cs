#region File Header
// <copyright file="EnumTable.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:20 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines the ability to create Enum Types in the Stack
    /// </summary>
    public class EnumTable
    {
        public string IDName { get; set; }

        public string EnumName { get; set; }

        public string BaseClassName { get; set; }

        public Dictionary<string, int> Values { get; set; }
    }
}