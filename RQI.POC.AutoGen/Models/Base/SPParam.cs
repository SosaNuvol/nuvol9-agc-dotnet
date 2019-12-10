#region File Header
// <copyright file="SPParam.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:28 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Data;

    public class SPParam
    {
        public string Name { get; set; }

        public string CleanName { get; set; }

        public string SysType { get; set; }

        public string DbType { get; set; }

        public ParameterDirection Mode { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }
    }
}