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
    using System.Linq;
    using Base;
    using Microsoft.Practices.Unity;

    public class RmTables : RmBase
    {
        private List<Table> _tables;
        private readonly List<Table> _manyToManyTables;
        private const string _BASE_TABLE = "BASE TABLE";
//        private const string _SQL_STATEMENT = @"
//SELECT
//	T.TABLE_CATALOG
//	, T.TABLE_SCHEMA
//	, T.TABLE_NAME
//	, T.TABLE_TYPE
//FROM INFORMATION_SCHEMA.TABLES AS T
//WHERE
//	(T.TABLE_TYPE='BASE TABLE')
//	AND (t.TABLE_SCHEMA IN (@SCHEMA_LIST@))
//ORDER BY
//	T.TABLE_SCHEMA,
//	T.TABLE_NAME
//";

        [InjectionConstructor]
        public RmTables([Dependency("SqlConnectionString")]string sqlConnectionString) : base(sqlConnectionString)
        {
            // ** Bind to Properties
            _tables = null;
            _manyToManyTables = new List<Table>();

            // ** Init
            InitInstance(Utilities.TableSchemaList, _BASE_TABLE);
        }

        public List<Table> Load()
        {
            // ** Check that there is a tables generated
            if (_tables != null) return _tables;

            // ** Init
            _tables = new List<Table>();
            var tableLookup  = new Dictionary<string, Table>();
            using (IDataReader dataReader = GetReader(SQLStatement))
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
                    if (Utilities.ShouldGenerateTable($"{tbl.Schema}.{tbl.Name}"))
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

                // ** Look for the columns for the given table key.
                if (Utilities.TableColumnDict.TryGetValue(key, out var columns))
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
                        col.ForeignKeyTableName = fkt != null ? fkt.OtherTable : "";
                        col.ForeignKeyTableSchemaName = fkt != null ? fkt.OtherSchema : "";
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
                // TODO:  Not sure that I need this line
                if (!tableLookup.ContainsKey(key)) tableLookup.Add(key, tableItem);
            }

            // ** Create many to many navigation collections
            foreach (var table in _manyToManyTables)
            {
                // ** Init
                var fromColumn = table.CompositeKeyColumns[0];
                var toColumn = table.CompositeKeyColumns[1];

                // ReSharper disable once InlineOutVariableDeclaration
                Table fromTable;
                if (!tableLookup.TryGetValue(fromColumn.ForeignKeyTableSchemaName + "." + fromColumn.ForeignKeyTableName, out fromTable))
                {
                    Utilities.LogError(Utilities.ClassTypes.BaseTable, $"Unable to lookup foreign from table for column \"{fromColumn.Name}\" on many to many table \"{table.Name}\", verify constraints are correct.");
                    // throw new Exception(string.Format("Unable to lookup foreign from table for column \"{0}\" on many to many table \"{1}\", verify constraints are correct.", fromColumn.Name, table.Name));
                }

                // ReSharper disable once InlineOutVariableDeclaration
                Table toTable;
                if (!tableLookup.TryGetValue(toColumn.ForeignKeyTableSchemaName + "." + toColumn.ForeignKeyTableName, out toTable))
                {
                    Utilities.LogError(Utilities.ClassTypes.BaseTable, $"Unable to lookup foreign to table for column \"{toColumn.Name}\" on many to many table \"{table.Name}\", verify constraints are correct.");
                    // throw new Exception(string.Format("Unable to lookup foreign to table for column \"{0}\" on many to many table \"{1}\", verify constraints are correct.", toColumn.Name, table.Name));
                }

                // ** Check that the to and from tables are present
                if (toTable == null || fromTable == null) continue;

                // ** Add to fromTable
                fromTable.Collections.Add(new NavigationCollection
                {
                    EntityType = toColumn.CleanName.Replace("Id", ""),
                    CollectionName = toTable.Name,
                    IsManyToMany = true
                });

                // ** Add to toTable
                toTable.Collections.Add(new NavigationCollection
                {
                    EntityType = fromColumn.CleanName.Replace("Id", ""),
                    CollectionName = fromTable.Name
                });
            }

            // ** Create one to many navigation collections.
            _tables = _tables.Where(t => !t.IsManyToManyTable).ToList();
            foreach (var table in _tables)
            {
                foreach (var fkTable in table.SingleFKTables)
                {
                    // ** Init
                    if (!tableLookup.TryGetValue(fkTable.OtherSchema + "." + fkTable.OtherTable, out var toTable))
                    {
                        Utilities.LogError(Utilities.ClassTypes.BaseTable, $"Unable to lookup foreign to table for column \"{fkTable.ThisTable}.{fkTable.ThisColumn}\" on one to many table \"{table.Schema}.{table.Name}\", verify constraints are correct.");
                        continue;
                        // throw new Exception($"Unable to lookup foreign to table for column \"{fkTable.ThisTable}.{fkTable.ThisColumn}\" on one to many table \"{table.Schema}.{table.Name}\", verify constraints are correct.");
                    }

                    if (!toTable.Collections.Any(c => c.CollectionName.Equals(table.Name)))
                    {
                        // Check to see if last two characters is 'ID' case insensitive.
                        var lastPos = table.PrimaryKey.ToLower().LastIndexOf("id", StringComparison.CurrentCultureIgnoreCase);
                        if (lastPos > -1)
                        {
                            toTable.Collections.Add(new NavigationCollection
                            {
                                EntityType = table.PrimaryKey.Substring(0, lastPos),
                                CollectionName = table.Name
                            });
                        }
                    }
                }
            }

            // ** Return result
            return _tables;
        }
    }
}