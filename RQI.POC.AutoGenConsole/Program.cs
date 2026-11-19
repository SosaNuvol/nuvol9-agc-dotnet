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
    using System.Linq;
    using RQI.POC.AutoGen;
    using RQI.POC.AutoGen.Models;
    using RQI.POC.AutoGenConsole.Configuration;
    using RQI.POC.AutoGenConsole.Utilities;

    internal class Program
    {
        private static int Main(string[] args)
        {
            // ** Init
            var defaultConsoleTextColor = Console.ForegroundColor;

            AgcOptions options;
            LoadedProfile profile;
            try
            {
                options = AgcOptions.Parse(args);
                if (options.ShowHelp)
                {
                    Console.WriteLine(AgcOptions.Usage);
                    return 0;
                }

                profile = ProfileLoader.Load(options);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is AgcConfigurationException || ex is System.IO.InvalidDataException || ex is FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = defaultConsoleTextColor;
                Console.WriteLine();
                Console.WriteLine(AgcOptions.Usage);
                return 1;
            }

            AgcSettings.Load(profile.Name, profile.Settings);
            AgcSettings.DryRun = options.DryRun;

            var connectionString = profile.Settings["ConnectionString"];

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("********************************************** PROFILE **********************************************");
            Console.WriteLine($"|** Profile:      {profile.Name} ({profile.ProfilePath})");
            Console.WriteLine($"|** Project root: {profile.ProjectRoot}");
            Console.WriteLine($"|** Connection:   {profile.ConnectionName} => {ProfileLoader.DescribeConnection(connectionString)}");
            Console.WriteLine($"|** Steps:        {(options.Only.Count == 0 ? "all" : string.Join(",", options.Only))}{(options.DryRun ? "  [DRY RUN: nothing is written]" : string.Empty)}");
            foreach (var templateSource in profile.TemplateSources)
            {
                Console.WriteLine($"|** Template:     {templateSource}");
            }
            Console.ForegroundColor = defaultConsoleTextColor;

            var table = new RmTables(connectionString);
            var views = new RmViews(connectionString);
            var sprocs = new RmProcedures(connectionString);
            var utypes = new RmUserTypes(connectionString);
            var criticalDataTables = new RmCriticalDataTypes(connectionString);

            // Generated files are not deleted up front. Each file is written only when its content changed
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

            if (options.ShouldRun("tables"))
            {
                var tableGenerator = new NOAHTablesGenerator(tableResult);
                tableGenerator.Generate(counter);
            }

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** VIEWS **********************************************");

            if (options.ShouldRun("views"))
            {
                var viewGenerator = new NOAHTablesGenerator(viewsResult);
                viewGenerator.Generate(counter);
            }

            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** TYPES **********************************************");

            if (options.ShouldRun("types"))
            {
                var udTypesGenerator = new NOAHUDTypesGenerator(userDefinedTypesResult);
                udTypesGenerator.Generate(counter);
            }

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** SPROCS **********************************************");

            if (options.ShouldRun("sprocs"))
            {
                var sprocGenerator = new NOAHSPROCSGenerator(sprocResult, userDefinedTypesResult.Select(t => t.Schema));
                sprocGenerator.Generate(counter);
            }

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

            if (options.ShouldRun("critical"))
            {
                var criticalDataGenerator = new NOAHCriticalDataGenerator(criticalDataTypesResult);
                criticalDataGenerator.Generate(counter);
            }

            // ** Generate DbContext
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** DBCONTEXT **********************************************");

            if (options.ShouldRun("dbcontext"))
            {
                var dbContextGenerator = new NOAHDbContextGenerator(tableResult);
                dbContextGenerator.Generate();
            }

            // ** Generate Repository Classes with Interfaces
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** REPOSITORIES **********************************************");

            if (options.ShouldRun("repositories"))
            {
                var repositoryGenerator = new NOAHRepositoryGenerator(tableResult);
                repositoryGenerator.Generate();
            }

            // ** Generate Unit of Work
            Console.ForegroundColor = defaultConsoleTextColor;

            Console.WriteLine("********************************************** UNIT OF WORK **********************************************");

            if (options.ShouldRun("unitofwork"))
            {
                var unitOfWorkGenerator = new NOAHUnitOfWorkGenerator(tableResult);
                unitOfWorkGenerator.Generate();
            }

            // ** Remove generated files for objects that no longer exist in the database
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** CLEANUP **********************************************");

            // ** Only after a full run: with --only, files from the skipped steps would look stale
            if (options.Only.Count == 0)
            {
                AutoGen.Utilities.AutoDALFileSystem.DeleteStaleGeneratedFiles();
            }
            else
            {
                Console.WriteLine("|** Skipped: stale-file cleanup only runs when every step runs.");
            }

            Console.WriteLine($"|** Files created: {AutoGen.Utilities.AutoDALFileSystem.CreatedCount} | updated: {AutoGen.Utilities.AutoDALFileSystem.UpdatedCount} | unchanged: {AutoGen.Utilities.AutoDALFileSystem.UnchangedCount} | removed: {AutoGen.Utilities.AutoDALFileSystem.RemovedCount}");

            // ** Show final done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** ALL GENERATION COMPLETE **********************************************");
            return 0;
        }
    }
}