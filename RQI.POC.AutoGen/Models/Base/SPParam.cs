#region File Header
// <copyright file="SPParam.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
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

        [JsonProperty(propertyName: "dataLength")]
        public int? MaxLength { get; set; }

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

        // ReSharper disable once UnusedMember.Global
        public string GetArgumentForCSharp()
        {
            // ** Init
            var result = (IsOptional && !SysType.Equals("string")) ? $"Nullable<{SysType}> {CleanName}" : $"{SysType} {CleanName}";

            // ** Check if default value
            if (!string.IsNullOrEmpty(DefaultValue)) result = result + $" = {GetDefaultValueForCSharp()}";

            // ** Return result
            return result;
        }

        private string GetDefaultValueForCSharp()
        {
            // ** Check that the SysType is string
            return !SysType.Equals("string") ? DefaultValue : $"\"{DefaultValue}\"";
        }
    }
}