#region File Header
// <copyright file="SPParam.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:28 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Data;
    using Newtonsoft.Json;

    public class SPParam
    {
        #region Parameters

        [JsonProperty(propertyName:"name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(propertyName: "cleanName", Required = Required.Always)]
        public string CleanName { get; set; }

        private string _sysType; 
        [JsonProperty(propertyName: "sysType")]
        public string SysType {
            get
            {
                // ** Init
                _sysType = Utilities.GetSysType(DbType);

                // ** Return result
                return _sysType;
            }
            set => _sysType = value;
        }

        [JsonProperty(propertyName: "dbType", Required = Required.Always)]
        public string DbType { get; set; }

        [JsonProperty(propertyName: "mode", Required = Required.Always)]
        public ParameterDirection Mode { get; set; }

        [JsonProperty(propertyName: "numericPrecision")]
        public int? Precision { get; set; }

        [JsonProperty(propertyName: "numericScale")]
        public int? Scale { get; set; }

        [JsonProperty(propertyName: "ordinalPosition", Required = Required.Always)]
        public int OrdinalPosition { get; set; }

        [JsonProperty(propertyName: "defaultValue")]
        public string DefaultValue { get; set; }

        public bool IsOptional { get; set; }

        #endregion Parameters
    }
}