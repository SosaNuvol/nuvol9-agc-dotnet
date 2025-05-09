#region File Header
// <copyright file="RmProcedureFunction.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-13-8:56 AM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Base;

    public class RmProcedures : RmBase
    {
        #region Properties
        private List<SP> _storedProcedures;

        #endregion Properties

        public RmProcedures(string sqlConnectionString) : base(sqlConnectionString)
        {
            // ** Init Class properties
            _storedProcedures = null;

            // ** Init
            InitInstance(Utilities.ProceduresSchemaList, SQLTypesEnum.Procedures);
        }

        public List<SP> Load()
        {
            // ** Check if already loaded
            if (_storedProcedures != null) return _storedProcedures;

            // ** Build Sproc list
            _storedProcedures = new List<SP>();
            using (IDataReader dataReader = GetReader(SQLStatement))
            {
                while (dataReader.Read())
                {
                    // ** Init
                    var name = DBNull.Value.Equals(dataReader["Name"]) ? null : (string) dataReader["Name"];
                    var schema = DBNull.Value.Equals(dataReader["Schema"]) ? null : (string)dataReader["Schema"];
                    var jsonParameterList = DBNull.Value.Equals(dataReader["JsonParametersList"]) ? null : (string)dataReader["JsonParametersList"];
                    var definition = DBNull.Value.Equals(dataReader["Definition"]) ? null : (string)dataReader["Definition"];
                    var returnType = DBNull.Value.Equals(dataReader["CSharpReturnType"]) ? null : (string)dataReader["CSharpReturnType"];

                    // ** Check that there is a name
                    if (string.IsNullOrEmpty(name)) continue;

                    // ** Build SPROC
                    var sprocItem = new SP(definition
                        , jsonParameterList
                        , schema
                        , name
                        , returnType);

                    // ** Bind Data
                    sprocItem.CleanName = Utilities.TransformName(sprocItem.Name);
                    sprocItem.ClassName = Utilities.Inflector.MakeSingular(sprocItem.CleanName);

                    // ** Load to list
                    _storedProcedures.Add(sprocItem);
                }
            }

            // ** Default response
            return _storedProcedures;
        }
    }
}