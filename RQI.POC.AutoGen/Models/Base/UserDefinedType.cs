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

namespace RQI.POC.AutoGen.Models.Base
{
    public class UserDefinedType
    {
        public List<Column> Properties { get; set; }

        public string Name { get; set; }

        public string Schema { get; set; }

    }
}
