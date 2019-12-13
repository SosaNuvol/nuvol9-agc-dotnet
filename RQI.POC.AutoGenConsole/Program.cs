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
    using AutoGen.Models;

    internal class Program
    {
        private static void Main(/*string[] args*/)
        {
            // ** Init
            var defaultConsoleTextColor = Console.ForegroundColor;
            var connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            var table = new RmTables(connectionString);
            var views = new RmViews(connectionString);

            // ** Load from SQL
            var tableResult = table.Load();
            var viewsResult = views.LoadViews();

            // ** Print results
            Console.WriteLine($"|*** Count of tables: {tableResult.Count}");

            // ** Loop through all tables
            var counter = 0;
            foreach (var tableItem in tableResult)
            {
                // ** Init
                counter++;
                var tableName = $"[{tableItem.Schema}].[{tableItem.Name}]";

                // ** Print table info
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|-> Table Name: {tableName,-50} | Columns: {tableItem.Columns.Count:D3}");

                // ** Print Columns
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var tableItemColumn in tableItem.Columns)
                {
                    Console.WriteLine($"|** ** Column Name {tableItemColumn.Name,-50} | Column C# Type: {tableItemColumn.DbType,-22} | Column DB Type: {tableItemColumn.DataType.ToUpper(), -22} | Column Length: {tableItemColumn.MaxLength}");
                }
            }

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** VIEWS **********************************************");

            // ** Loop though all Views
            counter = 0;
            foreach (var viewItem in viewsResult)
            {
                // ** Init
                counter++;
                var viewName = $"[{viewItem.Schema}].[{viewItem.Name}]";

                // ** Print view information
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"|** {counter:D4}|=> View Name: {viewName,-50} | Columns: {viewItem.Columns.Count:D3}");

                // ** Print Columns
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var tableItemColumn in viewItem.Columns)
                {
                    Console.WriteLine($"|** ** Column Name {tableItemColumn.Name,-50} | Column C# Type: {tableItemColumn.DbType,-22} | Column DB Type: {tableItemColumn.DataType.ToUpper(),-22} | Column Length: {tableItemColumn.MaxLength}");
                }
            }

            // ** Show done
            Console.ForegroundColor = defaultConsoleTextColor;
            Console.WriteLine("********************************************** DONE **********************************************");
        }
    }
}