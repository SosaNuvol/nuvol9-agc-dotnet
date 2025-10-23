namespace RQI.POC.AutoGen.Models.Base
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ProxyTypeTable
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public string ProxyTypeName { get; set; }

        public Dictionary<string, int> IdPrefixMappings { get; set; }
    }
}
