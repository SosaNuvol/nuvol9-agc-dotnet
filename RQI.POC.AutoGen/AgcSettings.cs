#region File Header
// <copyright file="AgcSettings.cs" company="Núvol 9">
//  Copyright (c) 2026 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  The settings for one generator run: the selected project profile plus its connection.
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGen
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Holds the settings for the current run. The console loads a project profile, resolves its
    /// connection and paths, and hands the result to <see cref="Load"/> keyed by the setting names
    /// the generators already use (e.g. <c>RQI.POC.AutoGen.TableSchemaList</c>).
    /// Nothing is read from App.config, so one build can serve any number of projects.
    /// </summary>
    public static class AgcSettings
    {
        private static Dictionary<string, string> _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Name of the loaded profile, for logging.</summary>
        public static string ProfileName { get; private set; }

        /// <summary>When true, generated files are compared but nothing is written or deleted.</summary>
        public static bool DryRun { get; set; }

        public static void Load(string profileName, IDictionary<string, string> values)
        {
            ProfileName = profileName;
            _values = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>Returns the setting, or null when the profile does not define it.</summary>
        public static string Get(string key)
        {
            return key != null && _values.TryGetValue(key, out var value) ? value : null;
        }
    }
}
