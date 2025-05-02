#region File Header
// <copyright file="Table.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:20 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Collections.Generic;
    using System.Linq;

    public class Table
    {
        public List<Column> Columns { get; set; }

        public List<FKTable> FKTables { get; set; }

        public List<FKTable> SingleFKTables { get; set; }

        public List<FKTable> QueryableFKTables { get; set; }

        public string Name { get; set; }

        public string CleanName { get; set; }

        public string ClassName { get; set; }

        public string PrimaryKey { get; set; }

        public bool IsManyToManyTable { get; set; }

        public List<Column> CompositeKeyColumns { get; set; }

        public List<NavigationCollection> Collections { get; set; }

        public string Schema { get; set; }

        public string QueryableName { get; set; }

        public EnumTable Enum { get; set; }

        public MetaDataTable MetaData { get; set; }

        public bool IsHistorical { get; set; }

        public Table()
        {
            CompositeKeyColumns = new List<Column>();
            Collections = new List<NavigationCollection>();
        }

        public bool HasLogicalDelete()
        {
            return this.Columns.Any(x => x.Name.ToLower() == "deleted" || x.Name.ToLower() == "isdeleted");
        }
        public Column DeleteColumn
        {
            get
            {
                Column result = null;
                if (this.Columns.Any(x => x.Name.ToLower() == "deleted"))
                    result = this.Columns.Single(x => x.Name.ToLower() == "deleted");
                if (this.Columns.Any(x => x.Name.ToLower() == "isdeleted"))
                    result = this.Columns.Single(x => x.Name.ToLower() == "isdeleted");
                return result;
            }
        }
        public Column PK
        {
            get
            {
                return this.Columns.SingleOrDefault(x => x.IsPK) ?? this.Columns[0];
            }
        }
        public Column Descriptor
        {
            get
            {
                Column result = null;
                if (this.Columns.Count > 0)
                {

                    if ((this.Columns.Count > 1) && (this.Columns[1].SysType.ToLower().Trim() == "string"))
                    {

                        //use second column if it's text
                        result = this.Columns[1];
                    }
                    else
                    {

                        //else use ID, which should be the first column
                        result = this.Columns[0];
                    }
                }
                return result;
            }
        }
    }
}