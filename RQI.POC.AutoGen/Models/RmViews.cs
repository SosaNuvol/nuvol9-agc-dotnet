#region File Header
// <copyright file="RmViews.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-12-9:52 AM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using Base;
    using Unity;

    public class RmViews : RmBase
    {
        #region Properties
        List<Table> _views;

        #endregion Properties

        [InjectionConstructor]
        public RmViews([Dependency("SqlConnectionString")] string sqlConnectionString) : base(sqlConnectionString)
        {
            // ** Init Class properties
            _views = null;

            // ** Init
            InitInstance(Utilities.ViewsSchemaList, SQLTypesEnum.Views);
        }

        #region Methods

        public List<Table> LoadViews()
        {
            // ** Check if initialized
            if (_views != null) return _views;

            // ** Init
            _views = new List<Table>();

            // ** Build Views list
            using (IDataReader dataReader = GetReader(SQLStatement))
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

                if (Utilities.ViewsColumnDict.TryGetValue(key, out var columns))
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

                }
                else
                {
                    Utilities.LogInternalError(Utilities.ClassTypes.View, $"No columns for View: {tbl.Schema}.{tbl.Name}");
                }
            }

            // ** Return result
            return _views;
        }
        #endregion Methods
    }
}