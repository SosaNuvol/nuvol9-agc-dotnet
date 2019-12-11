#region File Header
// <copyright file="Utilities.Logging.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
    using System.Collections.Generic;

    public static partial class Utilities
    {
        #region Logging

        public static List<string> errorLogs = new List<string>();
        public static List<string> warningLogs = new List<string>();

        #endregion Logging
    }
}