#region File Header

// <copyright file="Program.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:10 PM
// </summary>
// <author>Andres Sosa</author>

#endregion

namespace RQI.POC.AutoGenConsole
{
    using System.Configuration;
    using AutoGen.Models;

    internal class Program
    {
        private static void Main(string[] args)
        {
            // ** Init
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var table = new RmTables(connectionString);

            table.Load();
        }
    }
}