#region File Header
// <copyright file="Utilities.Logging.cs" company="Núvol 9">
//  Copyright (c) 2025 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Document created on ---
// </summary>
// <author>Andres Sosa</author>
#endregion
namespace RQI.POC.AutoGen
{
    using System.Collections.Generic;
    using System.Linq;

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

        public enum EntryLocationEnum
        {
            InsideUtility,
            AtTemplate,
            Other
        }

        #endregion Logging

        // ReSharper disable once UnusedMember.Global
        public static void LogT4TError(ClassTypes classTypes, string message)
        {
            ErrorLogs.Add(new LogEntry(classTypes, message, EntryLocationEnum.AtTemplate));
        }

        // ReSharper disable once UnusedMember.Global
        public static void LogT4TWarning(ClassTypes classTypes, string message)
        {
            WarningLogs.Add(new LogEntry(classTypes, message, EntryLocationEnum.AtTemplate));
        }

        public static void LogInternalError(ClassTypes classTypes, string message)
        {
            ErrorLogs.Add(new LogEntry(classTypes, message, EntryLocationEnum.InsideUtility));
        }

        public static void LogInternalWarning(ClassTypes classTypes, string message)
        {
            WarningLogs.Add(new LogEntry(classTypes, message, EntryLocationEnum.InsideUtility));
        }
    }

    public class LogEntry
    {
        public Utilities.ClassTypes ClassType { get; set; }

        public Utilities.EntryLocationEnum EntryLocation { get; private set; }

        public string Message { get; set; }

        public LogEntry(Utilities.ClassTypes classTypes, string message, Utilities.EntryLocationEnum entryLocation = Utilities.EntryLocationEnum.Other)
        {
            ClassType = classTypes;
            Message = message;
            EntryLocation = entryLocation;
        }
    }
}