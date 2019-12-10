#region File Header
// <copyright file="NavigationCollection.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:25 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    public class NavigationCollection
    {
        public string EntityType { get; set; }

        public string CollectionName { get; set; }

        public bool IsManyToMany { get; set; }
    }
}