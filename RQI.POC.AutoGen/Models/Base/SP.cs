#region File Header
// <copyright file="SP.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on 2019-12-06-2:27 PM
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen.Models.Base
{
    using System.Collections.Generic;
    using System.Text;

    public class SP
    {
        public string Name { get; set; }

        public string CleanName { get; set; }

        public string ClassName { get; set; }

        public List<SPParam> Parameters { get; set; }

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
}