#region File Header
// <copyright file="RmViews.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-12-9:52 AM
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

    public class RmViews : RmBase
    {
        #region Properties
        List<Table> _views;

        #endregion Properties

        [InjectionConstructor]
        public RmViews([Dependency("SqlConnectionString")] string sqlConnectionString): base(sqlConnectionString)
        {
            _views = null;

            // ** Init
            InitInstance(Utilities.SchemaList, "VIEW");
        }

        #region Methods

        public List<Table> LoadViews()
        {
            // ** Check if initialized
            if (_views != null) return _views;

            // ** Init
            _views = new List<Table>();

            // ** Build Views list
            using (IDataReader dataReader = GetReader(sqlStatement))
            {
                // ** Root loop through reader
                while (dataReader.Read())
                {
                    // ** Init
                    var tbl = new Table
                    {
                        Name = (string)dataReader["TABLE_NAME"],
                        Schema = (string)dataReader["TABLE_SCHEMA"]
                    };

                    // ** Check if view should be added
                    if (Utilities.ShouldGenerateView($"{tbl.Schema}.{tbl.Name}"))
                    {
                        // ** Init
                        tbl.Schema = (string)dataReader["TABLE_SCHEMA"];
                        tbl.CleanName = Utilities.TransformName(tbl.Name);
                        tbl.ClassName = Utilities.Inflector.MakeSingular(tbl.CleanName);
                        tbl.QueryableName = Utilities.Inflector.MakePlural(tbl.ClassName);

                        // ** Add to list
                        _views.Add(tbl);
                    }
                }
            }

            // ** Build columns list for each view
            foreach (var tbl in _views)
            {
                // ** Init 
                var key = $"{tbl.Schema}.{tbl.Name}";

                if (Utilities.TableColumnDict.TryGetValue(key, out var columns))
                {
                    // ** Init
                    tbl.Columns = columns;

                    // ** Build column information
                    Column pkColumn = columns.FirstOrDefault(p => p.IsPK);
                    if (pkColumn != null)
                    {
                        tbl.PrimaryKey = pkColumn.Name;
                        pkColumn.IsPK = true;
                    }
                    else
                    {
                        tbl.PrimaryKey = "";
                    }

                    tbl.SingleFKTables = Utilities.LoadSingleFKTables(tbl.Name, tbl.Schema);
                    tbl.QueryableFKTables = Utilities.LoadManyFKTables(tbl.Name, tbl.Schema);

                    tbl.FKTables = Utilities.LoadFKTables(tbl.Name, tbl.Schema);

                    // ** loop the FK tables and see if there's a match for our FK columns
                    foreach (Column col in tbl.Columns)
                    {
                        // ** Init
                        FKTable fkt = tbl.FKTables.FirstOrDefault(
                            x => x.ThisColumn.Equals(col.Name, StringComparison.InvariantCultureIgnoreCase)
                        );

                        col.IsForeignKey = !col.IsPK && fkt != null;
                        col.ForeignKeyTableName = (!col.IsPK && fkt != null) ? fkt.OtherSchema + "." + fkt.OtherTable : "";
                    }
                }
                else
                {
                    throw new Exception($"No columns for View: {tbl.Schema}.{tbl.Name}");
                }
            }

            // ** Return result
            return _views;
        }
        #endregion Methods
    }
}