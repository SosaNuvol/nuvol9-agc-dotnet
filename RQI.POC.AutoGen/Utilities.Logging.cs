#region File Header
// <copyright file="Utilities.Logging.cs" company="Cotiviti">
//  Copyright (c) 2019 Cotiviti.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
    using System.Collections.Generic;

    public static partial class Utilities
    {
        #region Logging

        // ** Properties
        public static List<LogEntry> ErrorLogs = new List<LogEntry>();

        public static List<LogEntry> WarningLogs = new List<LogEntry>();

        // ** Methods
        public enum ClassTypes
        {
            BaseTable,
            View,
            Procedure,
            Function
        }

        #endregion Logging

        public static void LogError(ClassTypes classTypes, string message)
        {
            ErrorLogs.Add(new LogEntry(classTypes, message));
        }

        public static void LogWarning(ClassTypes classTypes, string message)
        {
            WarningLogs.Add(new LogEntry(classTypes, message));
        }
    }

    public class LogEntry
    {
        public Utilities.ClassTypes ClassType { get; set; }

        public string Message { get; set; }

        public LogEntry(Utilities.ClassTypes classTypes, string message)
        {
            ClassType = classTypes;
            Message = message;
        }
    }
}