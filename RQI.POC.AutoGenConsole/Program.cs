#region File Header

// <copyright file="Program.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:10 PM
// </summary>
// <author>Andres Sosa</author>

#endregion

namespace RQI.POC.AutoGenConsole
{
    using System;
    using System.Configuration;
    using System.Linq;
    using RQI.POC.AutoGen.Models;
    using RQI.POC.AutoGenConsole.Utilities;

    internal class Program
    {
        private static void Main(/*string[] args*/)
        {
            // ** Init
            var defaultConsoleTextColor = Console.ForegroundColor;
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var table = new RmTables(connectionString);
            var views = new RmViews(connectionString);
            var sprocs = new RmProcedures(connectionString);
            var utypes = new RmUserTypes(connectionString);
            var criticalDataTables = new RmCriticalDataTypes(connectionString);

            // ** Load from SQL
            var sprocResult = sprocs.Load().OrderBy(s => s.SchemaName);
            var tableResult = table.Load().OrderBy(s => s.Schema);
            var viewsResult = views.LoadViews().OrderBy(s => s.Schema);
            var userDefinedTypesResult = utypes.LoadUserDefinedTypes().OrderBy(s => s.Schema);
            var criticalDataTypesResult = criticalDataTables.LoadCriticalDataTypes().OrderBy(s => s.Schema).ToList();

            var counter = 0;

            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** TABLES **********************************************");

            var tableGenerator = new NOAHTablesGenerator(tableResult);
            tableGenerator.Generate(counter);

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** VIEWS **********************************************");

            var viewGenerator = new NOAHTablesGenerator(viewsResult);
            viewGenerator.Generate(counter);

            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** TYPES **********************************************");

            var udTypesGenerator = new NOAHUDTypesGenerator(userDefinedTypesResult);
            udTypesGenerator.Generate(counter);

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** SPROCS **********************************************");

            var sprocGenerator = new NOAHSPROCSGenerator(sprocResult);
            sprocGenerator.Generate(counter);

            // ** Loop though all Views
            counter = 0;
            foreach (var sprocItem in sprocResult)
            {
                // ** Init
                counter++;
                
                // ** Print view information
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|=> View Name: {sprocItem.Name,-50} | Parameters: {sprocItem.Parameters.Count:D3}");

                // ** Print Columns
                Console.ForegroundColor = ConsoleColor.Yellow;
                foreach (var sprocItemParam in sprocItem.Parameters)
                {
                    Console.WriteLine($"|** ** Param Name {sprocItemParam.Name,-50} | Parameter C# Type: {sprocItemParam.SysType,-22} | Parameter DB Type: {sprocItemParam.DbType?.ToUpper(),-22} | IsOptional: {sprocItemParam.IsOptional} | Default Value: {sprocItemParam.DefaultValue}");
                }
            }

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** DONE **********************************************");

            // ** Generate Critical Data
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** CRITICAL DATA TYPES **********************************************");

            // ** Loop though all Critical Data Types
            counter = 0;
            foreach (var criticalDataType in criticalDataTypesResult)
            {
                // ** Init
                counter++;

                // ** Print critical data type information
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|=> Critical Data Type: {criticalDataType.Schema}.{criticalDataType.Name,-40} | Prefix: {criticalDataType.Schema} ");
            }

            var criticalDataGenerator = new NOAHCriticalDataGenerator(criticalDataTypesResult);
            criticalDataGenerator.Generate(counter);
        }
    }
}