#region File Header
// <copyright file="Utilities.Libraries.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
    using System;
    using System.Collections.Generic;

    public static partial class Utilities
    {
        #region Libraries

        private static Dictionary<string, string> _enumTables;

        public static Dictionary<string, string> EnumTables
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

        private static Dictionary<string, string> _metaDataTables;
        public static Dictionary<string, string> MetaDataTables
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
        #endregion Libraries
    }
}