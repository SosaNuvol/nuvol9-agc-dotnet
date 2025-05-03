#region File Header
// <copyright file="RmViews.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2025-05-02-9:59 AM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models
{
    using RQI.POC.AutoGen.Models.Base;
    using System;
    using System.Collections.Generic;
    using System.Data;

    public class RmUserTypes : RmBase
    {

        #region Properties
        List<UserDefinedType> _userTypes;
        #endregion Properties
        public RmUserTypes(string sqlConnectionString) : base(sqlConnectionString)
        {
            // ** Init Class properties
            _userTypes = null;
            // ** Init
            InitInstance(Utilities.UserDefinedTypesSchemaList, SQLTypesEnum.UserDefinedTypes);
        }

        public List<UserDefinedType> LoadUserDefinedTypes()
        {
            // ** Check if initialized
            if (_userTypes != null) return _userTypes;
            // ** Init
            _userTypes = new List<UserDefinedType>();
            // ** Build User Defined UserDefinedTypes list
            using (IDataReader dataReader = GetReader(SQLStatement))
            {
                // Dynamically get the index of the SYSTEM_TYPE column
                int systemTypeIndex = dataReader.GetOrdinal("SYSTEM_TYPE");
                int maxLengthIndex = dataReader.GetOrdinal("MaxLength");

                // ** Root loop through reader
                while (dataReader.Read())
                {
                    // ** Init
                    var userType = new UserDefinedType
                    {
                        Schema = (string)dataReader["TYPE_SCHEMA"],
                        Name = (string)dataReader["TYPE_NAME"],
                        CleanName = Utilities.TransformName((string)dataReader["TYPE_NAME"]).Replace("udtt", string.Empty).Replace("uddt", string.Empty).Replace("udt", string.Empty),
                        Suffix = (bool)dataReader["IS_TABLE_TYPE"] ? "TableType" : "UserType",
                        IsTableType = (bool)dataReader["IS_TABLE_TYPE"],
                        MaxLength =  dataReader.IsDBNull(maxLengthIndex) ? (short)0 : (short)dataReader["MaxLength"],
                        Precision = (byte)dataReader["Precision"],
                        Scale = (byte)dataReader["Scale"],
                        IsNullable = (bool)dataReader["IsNullable"],
                        SystemType = dataReader.IsDBNull(systemTypeIndex) ? null : (string)dataReader["SYSTEM_TYPE"],
                    };
                    // ** Add to list
                    _userTypes.Add(userType);
                }

                // Build definitions
                foreach (var userType in _userTypes)
                {
                    // ** Init 
                    var key = $"{userType.Schema}.{userType.Name}";

                    // ** Get the definitions
                    var found = Utilities.UdttColumnDict.TryGetValue(key, out var uttDefinitions);
                    if (found)
                    {
                        // ** Loop through the definitions
                        foreach (var column in uttDefinitions)
                        {
                            // ** Add to the list
                            userType.Definitions.Add(column);
                        }
                    }
                    else
                    {
                        // ** Log error
                        Console.WriteLine($"User Defined Type {userType.Name} not found in dictionary");
                    }
                }
            }
            return _userTypes;
        }
    }
}
