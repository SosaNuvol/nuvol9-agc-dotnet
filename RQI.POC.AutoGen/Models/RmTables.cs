#region File Header
// <copyright file="RmTables.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:16 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Base;
    using Microsoft.Practices.Unity;

    public class RmTables
    {
        private readonly List<Table> _tables = new List<Table>();
        private readonly List<Table> _manyToManyTables = new List<Table>();
        private readonly string _sqlConnectionString;
        private const string _SQL_STATEMENT = @"
SELECT
	T.TABLE_CATALOG
	, T.TABLE_SCHEMA
	, T.TABLE_NAME
	, T.TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES AS T
WHERE
	(T.TABLE_TYPE='BASE TABLE')
	AND (t.TABLE_SCHEMA IN (@SCHEMA_LIST@))
ORDER BY
	T.TABLE_SCHEMA,
	T.TABLE_NAME
";

        private string _sqlStatement;

        [InjectionConstructor]
        public RmTables([Dependency("SqlConnectionString")]string sqlConnectionString)
        {
            // ** Bind to Properties
            _sqlConnectionString = sqlConnectionString;

            // ** Init
            InitInstance(Utilities.SchemaList);
        }

        public void Load()
        {
            // ** Init
            var tableLookup  = new Dictionary<string, Table>();
            using (IDataReader dataReader = GetReader(_sqlStatement))
            {
                while (dataReader.Read())
                {
                    // ** Init
                    var tbl = new Table
                    {
                        Name = (string) dataReader["TABLE_NAME"], 
                        Schema = (string) dataReader["TABLE_SCHEMA"]
                    };

                    // ** Check if table is included
                    // ReSharper disable once UseStringInterpolation
                    if (Utilities.ShouldGenerateTable(string.Format("{0}.{1}", tbl.Schema, tbl.Name)))
                    {
                        // ** Set needed properties to Table.
                        tbl.CleanName = Utilities.TransformName(tbl.Name);
                        tbl.ClassName = Utilities.Inflector.MakeSingular(tbl.CleanName);
                        tbl.QueryableName = Utilities.Inflector.MakePlural(tbl.ClassName);

                        // ** Add to tables list
                        _tables.Add(tbl);
                    }
                }
            }

            // ** Create Dictionary with table and column information
            foreach (var tableItem in _tables)
            {
                // ** Init 
                var key = tableItem.Schema + "." + tableItem.Name;
                // ReSharper disable once InlineOutVariableDeclaration
                List<Column> columns;

                if (Utilities.TableColumnDict.TryGetValue(key, out columns))
                {
                    // ** Init
                    tableItem.Columns = columns;

                    #region Build Primary Key Information
                    // ** ** Init
                    var pkColumns = columns.Where(p => p.IsPK).ToList();
                    // ** ** Check if this is a composite PK (more than 1 column)
                    if (pkColumns.Count >= 2)
                    {
                        // ** Init composite properties of the tableItem
                        tableItem.CompositeKeyColumns = pkColumns;
                        tableItem.IsManyToManyTable = true;
                        _manyToManyTables.Add(tableItem);

                    } 
                    else if (pkColumns.Count == 1)
                    {
                        // ** Init for a single PK Column
                        var pkColumn = pkColumns.First();
                        tableItem.PrimaryKey = pkColumn.Name;

                        // ** Set the column to PK flag be true
                        pkColumn.IsPK = true;
                    }
                    else
                    {
                        // ** Flag this table with no Primary key
                        tableItem.PrimaryKey = string.Empty;
                    }
                    #endregion Build Primary Key Information

                    #region Flag and load ancilary properties of table
                    // ** Init
                    tableItem.IsHistorical = columns.Any(c => c.Name.Equals("IsDeleted") && c.Name.Equals("IsActive"));
                    tableItem.SingleFKTables = Utilities.LoadSingleFKTables(tableItem.Name, tableItem.Schema);
                    tableItem.QueryableFKTables = Utilities.LoadManyFKTables(tableItem.Name, tableItem.Schema);

                    tableItem.FKTables = Utilities.LoadFKTables(tableItem.Name, tableItem.Schema);

                    //loop the FK tables and see if there's a match for our FK columns
                    foreach (Column col in tableItem.Columns)
                    {
                        FKTable fkt = tableItem.FKTables.FirstOrDefault(
                            x => x.ThisColumn.Equals(col.Name, StringComparison.InvariantCultureIgnoreCase)
                        );

                        col.IsForeignKey = fkt != null;
                        col.ForeignKeyTableName = fkt != null ? fkt.OtherSchema + "." + fkt.OtherTable : "";
                        col.IsEnum = fkt != null && Utilities.EnumTables.Keys.Any(eKey => eKey.EndsWith("." + col.ForeignKeyTableName));
                    }
                    #endregion Flag and load ancilary properties of table
                }
                else
                {
                    // ReSharper disable once UseStringInterpolation
                    throw new Exception(string.Format("No columns for Table: {0}.{1}", tableItem.Schema, tableItem.Name));
                }

                // ** Check if this is a Types table that needs to be stubbed out.
                // ReSharper disable once UseStringInterpolation
                if (Utilities.EnumTables.ContainsKey(string.Format("{0}.{1}", tableItem.Schema, tableItem.Name)))
                {
                    //set enum table on table
                    tableItem.Enum = Utilities.GetEnumTable(tableItem.Schema, tableItem.Name);
                }


                // ** Check to see if this table is a MetaDataTable
                if (Utilities.MetaDataTables.ContainsKey(tableItem.Name))
                {
                    // Set MetaData table on table
                    tableItem.MetaData = Utilities.GetMetaDataTable(tableItem.Schema, tableItem.Name);
                }

                // ** Add to Dictionary
                if (!tableLookup.ContainsKey(key)) tableLookup.Add(key, tableItem);
            }

            // ** Create many to many navigation collections
            foreach (var manyToManyTable in _manyToManyTables)
            {
                
            }
        }

        private void InitInstance(string schemaListString)
        {
            // ** Return result
            _sqlStatement = _SQL_STATEMENT.Replace("@SCHEMA_LIST@", schemaListString);
        }

        private IDataReader GetReader(string sql)
        {
            SqlConnection conn = new SqlConnection(_sqlConnectionString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        private SqlCommand GetCommand(string sql)
        {
            SqlConnection conn = new SqlConnection(_sqlConnectionString);
            SqlCommand cmd = new SqlCommand(sql, conn);
            conn.Open();
            return cmd;
        }

    }
}