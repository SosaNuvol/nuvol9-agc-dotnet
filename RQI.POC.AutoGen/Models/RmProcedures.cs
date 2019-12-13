#region File Header
// <copyright file="RmProcedureFunction.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
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
                    var name = dataReader["Definition"] == typeof(DBNull) ? null : (string) dataReader["Definition"];
                    var schema = dataReader["Schema"] == typeof(DBNull) ? null : (string)dataReader["Schema"];
                    var jsonParameterList = dataReader["JsonParametersList"] == typeof(DBNull) ? null : (string)dataReader["JsonParametersList"];
                    var definition = dataReader["Definition"] == typeof(DBNull) ? null : (string)dataReader["Definition"];

                    // ** Build SPROC
                    var sprocItem = new SP(definition
                        , jsonParameterList
                        , schema
                        , name);

                    // ** Bind Data

                    // ** Load to list
                    _storedProcedures.Add(sprocItem);
                }
            }

            // ** Default response
            return _storedProcedures;
        }
    }
}