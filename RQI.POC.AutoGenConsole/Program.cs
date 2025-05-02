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
    using System;
    using System.Configuration;
    using System.Linq;
    using AutoGen.Models;
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

            // ** Load from SQL
            var sprocResult = sprocs.Load().OrderBy(s => s.SchemaName);
            var tableResult = table.Load().OrderBy(s => s.Schema);
            var viewsResult = views.LoadViews().OrderBy(s => s.Schema);

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

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** SPROCS **********************************************");

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
        }

        //private static void ExecuteTest(string connectionString)
        //{
        //    // ** Init
        //    DataTable sprocs;
        //    using (var conn = new SqlConnection(connectionString))
        //    {
        //        conn.Open();
        //        sprocs = conn.GetSchema("Procedures");
        //        conn.Close();
        //    }

        //    foreach (DataRow sproc in sprocs.Rows)
        //    {
        //        Console.WriteLine($"SPROC Name: {sproc["ROUTINE_NAME"].ToString()}");
        //    }
        //}
    }
}