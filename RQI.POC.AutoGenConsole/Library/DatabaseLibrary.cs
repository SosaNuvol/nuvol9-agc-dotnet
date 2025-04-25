namespace RQI.POC.AutoGenConsole.Library
{
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System;
    using System.Linq;

    public class DatabaseLibrary
    {
        const string ConnectionStringName = "Nuvol9ConnString";
        const string DatabaseName = "Nuvol9";

        const string Namespace = "Nuvol9.Models";
        const string Baseclass = "BaseModel";

        const string IncludeProcedureList = "^cust*";
        const string ExcludeProcedureList = "^custFx*";

        const string IncludeTableList = "*";
        const string ExcludeTableList =
            @"sysdiagrams,dtproperty,MSreplication_objects,MSreplication_subscriptions"
            + ",MSsavedforeignkeycolumns,MSsavedforeignkeyextendedproperties,MSsavedforeignkeys"
            + ",MSsnapshotdeliveryprogress,MSsubscription_agents,MSsubscription_properties,^temp$,__RefactorLog,AccessLevels,TableAccessLevels,UserLogin,TableInnerJoins,SeasonSummerSeasonMaps,ProductCategoryProductsRef";

        const string IncludeViewList = "*";
        const string ExcludeViewList = "^vwGen_*,vwUserExperienceSchedules";

        const string RegexReplaceExpression = "";
        const string RegexMatchExpression = "";
        const string RegexDictionaryReplace = @"^vw_(.*),$1View;^vw([A-Z]{2})_(.*),$1\_$2View;vw(.*),$1View;^VW_(.*),$1View;^cust([A-Z][A-Z][A-Z]?)_,$1\_;^proc_([A-Z][A-Z][A-Z]?)_,$1\_;^([A-Z][A-Z][A-Z]?)_,$1\_";
        const bool RegexIgnoreCase = false;

        const string StripText = "^cust_|^cust";

        const string EnumTableList = "VRG.EnvironmentTypes";
        const string MetaDataTableList = "";

        string[] _includeProcedures;
        public string[] IncludeProcedures
        {
            get
            {
                SetList(ref _includeProcedures, IncludeProcedureList);
                return _includeProcedures;
            }
        }
        string[] _excludeProcedures;
        public string[] ExcludeProcedures
        {
            get
            {
                SetList(ref _excludeProcedures, ExcludeProcedureList);
                return _excludeProcedures;
            }
        }

        string[] _includeTables;
        public string[] IncludeTables
        {
            get
            {
                SetList(ref _includeTables, IncludeTableList);
                return _includeTables;
            }
        }
        string[] _excludeTables;
        public string[] ExcludeTables
        {
            get
            {
                SetList(ref _excludeTables, ExcludeTableList);
                return _excludeTables;
            }
        }

        string[] _includeViews;
        public string[] IncludeViews
        {
            get
            {
                SetList(ref _includeViews, IncludeViewList);
                return _includeViews;
            }
        }
        string[] _excludeViews;
        public string[] ExcludeViews
        {
            get
            {
                SetList(ref _excludeViews, ExcludeViewList);
                return _excludeViews;
            }
        }

        Dictionary<string, string> _enumTables;
        public Dictionary<string, string> EnumTables
        {
            get
            {
                if (_enumTables == null)
                {
                    _enumTables = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                    string[] list = null;
                    SetList(ref list, EnumTableList);

                    foreach (string g in list)
                    {
                        _enumTables.Add(g, null);
                    }
                }
                return _enumTables;
            }
        }

        private static void SetList(ref string[] ray, string textList)
        {
            if (ray == null)
            {
                if (!string.IsNullOrEmpty(textList))
                {
                    ray = textList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    ray = new string[0];
                }
            }
        }

        Dictionary<string, string> _metaDataTables;
        public Dictionary<string, string> MetaDataTables
        {
            get
            {
                if (_metaDataTables == null)
                {
                    _metaDataTables = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    string[] szaList = null;
                    SetList(ref szaList, MetaDataTableList);

                    foreach (string szTableName in szaList)
                    {
                        _metaDataTables.Add(szTableName, null);
                    }
                }
                return _metaDataTables;
            }
        }

        string CheckNullable(Column col)
        {
            string result = "";
            if (col.IsNullable && col.SysType != "byte[]" && col.SysType != "string")
                result = "?";
            return result;
        }

        public class EnumTable
        {
            public string IDName;
            public string EnumName;
            public string BaseClassName;
            public Dictionary<string, int> Values;
        }

        public class MetaDataTable
        {
            public string IDName;
            public string MetaDataName;
            public string BaseClassName;
            public Dictionary<string, string> Values;
        }

        public class Table
        {
            public List<Column> Columns;
            public List<FKTable> FKTables;
            public List<FKTable> SingleFKTables;
            public List<FKTable> QueryableFKTables;
            public string Name;
            public string CleanName;
            public string ClassName;
            public string PrimaryKey;
            public bool IsManyToManyTable;
            public List<Column> CompositeKeyColumns;
            public List<NavigationCollection> Collections;
            public string Schema;
            public string QueryableName;
            public EnumTable Enum;
            public MetaDataTable MetaData;
            public bool IsHistorical;

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

        public class Column
        {
            public string Name;
            public string CleanName;
            public string SysType;
            public string DataType;
            public DbType DbType;
            public bool AutoIncrement;
            public bool IsPK;
            public int MaxLength;
            public bool IsNullable;
            public bool IsForeignKey;
            public bool IsEnum;
            public bool IsComputed;
            public string ColumnAttributeText;

            public bool IsReadOnly;
            public string DefaultSetting;
            public string ForeignKeyTableName;
            public string TableName;
            public string TableSchema;
        }

        public class NavigationCollection
        {
            public string EntityType;
            public string CollectionName;
            public bool IsManyToMany;
        }

        public class FKTable
        {
            public string ThisSchema;
            public string ThisTable;
            public string ThisColumn;
            public string OtherSchema;
            public string OtherTable;
            public string OtherColumn;
            public string OtherClass;
            public string OtherQueryable;
            public string PropName;
            public string ConstraintName;
        }

        public class SP
        {
            public string Name;
            public string CleanName;
            public string ClassName;
            public List<SPParam> Parameters;
            public SP()
            {
                Parameters = new List<SPParam>();
            }
            public string ArgList
            {
                get
                {
                    StringBuilder sb = new StringBuilder();
                    int loopCount = 1;
                    foreach (var par in Parameters)
                    {
                        sb.AppendFormat("{0} {1}", par.SysType, par.CleanName);
                        if (loopCount < Parameters.Count)
                            sb.Append(",");
                        loopCount++;
                    }
                    return sb.ToString();
                }
            }
        }
        public class SPParam
        {
            public string Name;
            public string CleanName;
            public string SysType;
            public string DbType;
            public ParameterDirection Mode;
            public int Precision;
            public int Scale;
        }

        public Column GetMatchingColumn(Table tbl, params string[] columns)
        {
            Column result = null;
            foreach (string columnName in columns)
            {
                result = tbl.Columns.SingleOrDefault(x => String.Equals(x.Name, columnName, StringComparison.InvariantCultureIgnoreCase));
                if (result != null)
                {
                    break;
                }
            }
            return result;
        }


        public bool ShouldGenerateView(string viewName)
        {
            return ShouldGenerate(viewName, IncludeViews, ExcludeViews);
        }

        public bool ShouldGenerateTable(string tableName)
        {
            return ShouldGenerate(tableName, IncludeTables, ExcludeTables);
        }
        public bool ShouldGenerateSproc(string tableName)
        {
            return ShouldGenerate(tableName, IncludeProcedures, ExcludeProcedures);
        }
        public static bool ShouldGenerate(string objectName, string[] includeList, string[] excludeList)
        {
            bool result = true;
            bool generateAll = false;

            // first, check to see if the includeList says to include all tables
            // this is the default
            if (includeList.Length == 1)
            {
                if (includeList[0] == "*")
                    generateAll = true;
            }

            // if we need to generate all tables, then we need to check the excludeList
            if (generateAll)
            {
                foreach (string s in excludeList)
                {
                    if (Utility.IsRegexMatch(objectName, s.Trim()))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                // IncludeList TRUMPs excludeList
                //	in case of confusion what this means is that if there is an includeList,
                //	be definition there's an excludeList of all tables not included
                // yep, confusing.

                // this means that tables were specifically requested in the includeList
                // need to make them prove themselves
                result = false;

                foreach (string s in includeList)
                {
                    if (Utility.IsRegexMatch(objectName, s.Trim()))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public string TransformName(string name)
        {
            if (String.IsNullOrEmpty(name))
                return String.Empty;

            string newName = name;
            newName = Utility.RegexTransform(newName, RegexIgnoreCase, RegexMatchExpression, RegexDictionaryReplace, RegexReplaceExpression);
            newName = Utility.StripText(newName, StripText);
            //newName = Inflector.ToPascalCase(newName, false);
            newName = Utility.IsStringNumeric(newName) ? "_" + newName : newName;
            newName = Utility.StripNonAlphaNumeric(newName);
            newName = newName.Trim();
            newName = Utility.KeyWordCheck(newName, String.Empty);
            return newName;
        }


    }
}
