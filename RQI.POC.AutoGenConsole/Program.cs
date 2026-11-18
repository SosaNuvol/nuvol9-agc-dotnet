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
    using System.Threading.Tasks.Sources;
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

            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** DELETE **********************************************");

            // Generated files are no longer deleted up front. Each file is written only when its content changed
            // (ignoring the header date), and files for objects that no longer exist are removed at the end.

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

            var sprocGenerator = new NOAHSPROCSGenerator(sprocResult, userDefinedTypesResult.Select(t => t.Schema));
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

            // ** Generate DbContext
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** DBCONTEXT **********************************************");

            var dbContextGenerator = new NOAHDbContextGenerator(tableResult);
            dbContextGenerator.Generate();

            // ** Generate Repository Classes with Interfaces
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** REPOSITORIES **********************************************");

            var repositoryGenerator = new NOAHRepositoryGenerator(tableResult);
            repositoryGenerator.Generate();            // ** Generate Repository Classes with Interfaces
            Console.ForegroundColor = defaultConsoleTextColor;

            Console.WriteLine("********************************************** UNIT OF WORK **********************************************");

            var unitOfWorkGenerator = new NOAHUnitOfWorkGenerator(tableResult);
            unitOfWorkGenerator.Generate();

            // ** Remove generated files for objects that no longer exist in the database
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** CLEANUP **********************************************");

            AutoGen.Utilities.AutoDALFileSystem.DeleteStaleGeneratedFiles();

            Console.WriteLine($"|** Files created: {AutoGen.Utilities.AutoDALFileSystem.CreatedCount} | updated: {AutoGen.Utilities.AutoDALFileSystem.UpdatedCount} | unchanged: {AutoGen.Utilities.AutoDALFileSystem.UnchangedCount} | removed: {AutoGen.Utilities.AutoDALFileSystem.RemovedCount}");

            // ** Show final done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** ALL GENERATION COMPLETE **********************************************");
        }
    }
}