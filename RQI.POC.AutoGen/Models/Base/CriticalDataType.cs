#region File Header
// <copyright file="CriticalDataType.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2025-01-03
// </summary>
// <author>GitHub Copilot</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Collections.Generic;

    public class CriticalDataType
    {
        #region Properties
        public int Id { get; set; }
        public string Prefix { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Schema { get; set; }
        public string TableName { get; set; }
        #endregion Properties
    }
}