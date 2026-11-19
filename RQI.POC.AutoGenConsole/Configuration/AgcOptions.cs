#region File Header
// <copyright file="AgcOptions.cs" company="Núvol 9">
//  Copyright (c) 2026 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Command line options for the generator.
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGenConsole.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Command line options.
    /// <code>
    /// agc --profile solasta [--connection SolastaDev] [--project-root D:\Repos\x] [--only tables,sprocs] [--dry-run]
    /// </code>
    /// </summary>
    public class AgcOptions
    {
        public static readonly string[] Steps = { "tables", "views", "types", "sprocs", "critical", "dbcontext", "repositories", "unitofwork" };

        public string Profile { get; private set; }

        public string Connection { get; private set; }

        public string ProjectRoot { get; private set; }

        public bool DryRun { get; private set; }

        public bool ShowHelp { get; private set; }

        /// <summary>Steps to run. Empty means every step.</summary>
        public ISet<string> Only { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool ShouldRun(string step) => Only.Count == 0 || Only.Contains(step);

        public static AgcOptions Parse(string[] args)
        {
            var options = new AgcOptions();
            args = args ?? new string[0];

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                string value = null;

                // ** Support both "--name value" and "--name=value"
                var equals = arg.IndexOf('=');
                if (arg.StartsWith("--") && equals > 0)
                {
                    value = arg.Substring(equals + 1);
                    arg = arg.Substring(0, equals);
                }

                switch (arg.ToLowerInvariant())
                {
                    case "--profile":
                    case "-p":
                        options.Profile = NextValue(args, ref i, arg, value);
                        break;
                    case "--connection":
                    case "-c":
                        options.Connection = NextValue(args, ref i, arg, value);
                        break;
                    case "--project-root":
                        options.ProjectRoot = NextValue(args, ref i, arg, value);
                        break;
                    case "--only":
                        foreach (var step in NextValue(args, ref i, arg, value).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()))
                        {
                            if (!Steps.Contains(step, StringComparer.OrdinalIgnoreCase))
                                throw new ArgumentException($"Unknown step '{step}' in --only. Valid steps: {string.Join(", ", Steps)}.");
                            options.Only.Add(step);
                        }
                        break;
                    case "--dry-run":
                        options.DryRun = true;
                        break;
                    case "--help":
                    case "-h":
                    case "/?":
                        options.ShowHelp = true;
                        break;
                    default:
                        // ** A bare first argument is taken as the profile
                        if (!arg.StartsWith("-") && options.Profile == null)
                        {
                            options.Profile = arg;
                            break;
                        }
                        throw new ArgumentException($"Unknown argument '{args[i]}'. Use --help for usage.");
                }
            }

            return options;
        }

        private static string NextValue(string[] args, ref int i, string arg, string inlineValue)
        {
            if (inlineValue != null) return inlineValue;
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--")) throw new ArgumentException($"{arg} needs a value.");
            return args[++i];
        }

        public static string Usage =>
@"Usage: RQI.POC.AutoGenConsole [--profile <name|path>] [options]

  --profile, -p <name|path>  Project profile: a name under profiles\ (e.g. solasta),
                             a folder holding agc.profile.json, or a .json file.
                             Default: AGC_PROFILE environment variable, then
                             AGC.DefaultProfile in App.config.
  --connection, -c <name>    Connection name to use instead of the profile's.
  --project-root <path>      Override the profile's projectRoot.
  --only <steps>             Comma-separated steps: " + string.Join(",", Steps) + @".
                             Stale-file cleanup is skipped when --only is used.
  --dry-run                  Report what would be created, updated or removed; write nothing.
  --help, -h                 Show this help.";
    }
}
