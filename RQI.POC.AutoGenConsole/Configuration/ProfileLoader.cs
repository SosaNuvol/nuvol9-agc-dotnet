#region File Header
// <copyright file="ProfileLoader.cs" company="Núvol 9">
//  Copyright (c) 2026 Núvol 9.  All rights reserved.
// </copyright>
// <summary>
//  Loads a project profile and its connection, and turns them into the generator settings.
// </summary>
// <author>Andres Sosa</author>
#endregion

namespace RQI.POC.AutoGenConsole.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Builds the settings for one run from three sources:
    /// <list type="number">
    /// <item><c>agc.defaults.json</c> next to the exe: naming rules and filters shared by every project.</item>
    /// <item>The project profile (<c>agc.profile.json</c>): schemas, namespaces, templates and output folders.</item>
    /// <item>Connections, never committed: <c>%APPDATA%\Nuvol9\AGC\connections.json</c>,
    /// <c>connections.local.json</c> next to the exe, and <c>AGC_Connections__&lt;name&gt;</c> environment variables.</item>
    /// </list>
    /// </summary>
    public static class ProfileLoader
    {
        public const string ProfileFileName = "agc.profile.json";
        public const string DefaultsFileName = "agc.defaults.json";
        public const string ConnectionsFileName = "connections.local.json";

        private const string P = "RQI.POC.AutoGen.";
        private const string G = "RQI.GEN.";

        /// <summary>Profile keys (colon separated) that are lists, mapped to the generator setting they fill.</summary>
        private static readonly Dictionary<string, string> ListSettings = new Dictionary<string, string>
        {
            { "schemas:tables", P + "TableSchemaList" },
            { "schemas:views", P + "ViewsSchemaList" },
            { "schemas:procedures", P + "StPrcSchemaList" },
            { "schemas:userTypes", P + "StUDTSchemaList" },
            { "schemas:criticalDataTypes", P + "CriticalDataTypesSchemaList" },
            { "criticalDataTypeTables", P + "CriticalDataTypesTablesList" },
            { "include:tables", P + "IncludeTableList" },
            { "include:views", P + "IncludeViewList" },
            { "include:procedures", P + "IncludeProcedureList" },
            { "exclude:tables", P + "ExcludeTableList" },
            { "exclude:views", P + "ExcludeViewList" },
            { "exclude:procedures", P + "ExcludeProcedureList" },
            { "columns:historical", P + "HistoricalColumns" },
            { "columns:audit", P + "AuditColumns" },
            { "enumTables", P + "EnumTableList" },
            { "metaDataTables", P + "MetaDataTableList" },
            { "naming:skipPluralNames", P + "SkipPluralNamesList" },
            { "naming:wordsEndingInES", P + "WordsShouldEndES" },
            { "sql:systemColumnsToIgnore", P + "SQL.SystemColToIgnoreList" },
            { "sql:columnDataTypesToIgnore", P + "SQL.ColumnDataTypeToIgnoreList" },
        };

        /// <summary>Profile keys that are single values.</summary>
        private static readonly Dictionary<string, string> ValueSettings = new Dictionary<string, string>
        {
            { "fileExtension", P + "AutogGenFileExtension" },
            { "naming:regexIgnoreCase", P + "RegexIgnoreCase" },
            { "naming:regexMatchExpression", P + "RegexMatchExpression" },
            { "naming:regexReplaceExpression", P + "RegexReplaceExpression" },
            { "naming:regexDictionaryReplace", P + "RegexDictionaryReplace" },
            { "naming:stripText", P + "StripText" },
            { "namespaces:models", P + "NamespaceModels" },
            { "namespaces:dataAccessModels", P + "NamespaceDataAccess" },
            { "namespaces:data", G + "DataDataNameSpace" },
            { "namespaces:dbContext", G + "DbContextNamespance" },
            { "namespaces:unitOfWork", G + "UnitOfWorkNameSpace" },
            { "dbContext:className", G + "DbContextFileName" },
            { "dbContext:connectionStringName", G + "DbContextConnectionStringName" },
        };

        /// <summary>Output folders and files, resolved against projectRoot.</summary>
        private static readonly Dictionary<string, string> OutputSettings = new Dictionary<string, string>
        {
            { "output:dataModels", G + "DataModelsRootFilePath" },
            { "output:dataAccess", G + "DataAccessRootFilePath" },
            { "output:repository", G + "RepositoryRootFilePath" },
            { "output:dataEntity", G + "Data.EntityRootFilePath" },
            { "output:proxyTypes", G + "ProxyTypeRootFilePath" },
            { "output:unitOfWork", G + "UnitOfWorkFilePath" },
            { "output:unitOfWorkInterface", G + "UnitOfWorkInterfaceFilePath" },
        };

        /// <summary>Templates: profile key, generator setting, and the file name looked up when the profile names no file.</summary>
        private static readonly (string Key, string Setting, string FileName)[] TemplateSettings =
        {
            ("templates:table", G + "TableTemplateFilePath", "TableModel.txt"),
            ("templates:view", G + "ViewTemplateFilePath", "ViewModel.txt"),
            ("templates:userType", G + "UDDTTemplateFilePath", "UDDTModel.txt"),
            ("templates:sproc", G + "SPROCTemplateFilePath", "SPROC.txt"),
            ("templates:proxyType", G + "ProxyTypeTemplateFilePath", "ProxyTableLookupModel.txt"),
            ("templates:dbContext", G + "DbContextTemplateFilePath", "DbContextTemplate.txt"),
            ("templates:repositoryInterface", G + "RepositoryInterfaceTemplateFilePath", "RepositoryInterfaceTemplate.txt"),
            ("templates:repository", G + "RepositoryTemplateFilePath", "RepositoryTemplate.txt"),
            ("templates:unitOfWork", G + "UnitOfWorkTemplateFilePath", "UnitOfWorkTemplate.txt"),
            ("templates:unitOfWorkInterface", G + "UnitOfWorkInterfaceTemplateFilePath", "UnitOfWorkInterfaceTemplate.txt"),
        };

        private static string ExeDirectory => AppDomain.CurrentDomain.BaseDirectory;

        public static string BuiltInTemplatesDirectory => Path.Combine(ExeDirectory, "Templates", "Files");

        public static LoadedProfile Load(AgcOptions options)
        {
            // ** Find the profile
            var profileArg = options.Profile
                             ?? Environment.GetEnvironmentVariable("AGC_PROFILE")
                             ?? System.Configuration.ConfigurationManager.AppSettings["AGC.DefaultProfile"];
            if (string.IsNullOrWhiteSpace(profileArg))
                throw new AgcConfigurationException("No profile given. Pass --profile <name|path>." + Environment.NewLine + DescribeAvailableProfiles());

            var profilePath = ResolveProfilePath(profileArg.Trim());
            var profileDirectory = Path.GetDirectoryName(profilePath);

            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ExeDirectory, DefaultsFileName), optional: true, reloadOnChange: false)
                .AddJsonFile(profilePath, optional: false, reloadOnChange: false)
                .Build();

            var profileName = config["name"] ?? Path.GetFileName(profileDirectory);

            // ** Paths: projectRoot is relative to the profile file; outputs and named templates are relative to projectRoot
            var projectRoot = ResolvePath(profileDirectory, options.ProjectRoot ?? config["projectRoot"] ?? ".");
            if (!Directory.Exists(projectRoot))
                throw new AgcConfigurationException($"Project root not found: {projectRoot}. Set \"projectRoot\" in {profilePath} or pass --project-root.");

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // ** Connection
            var connectionName = options.Connection ?? config["connection"];
            if (string.IsNullOrWhiteSpace(connectionName))
                throw new AgcConfigurationException($"The profile {profilePath} has no \"connection\". Add one or pass --connection <name>.");
            values["ConnectionString"] = ResolveConnection(connectionName.Trim());

            // ** Lists and single values. Missing settings become "" (as an empty App.config entry did), so the generators' own defaults apply.
            var tableSchemas = GetList(config, "schemas:tables");
            foreach (var pair in ListSettings)
            {
                var list = GetList(config, pair.Key);

                // ** Other schema lists fall back to the table schemas
                if (list == null && pair.Key.StartsWith("schemas:")) list = tableSchemas;

                values[pair.Value] = list ?? string.Empty;
            }

            foreach (var pair in ValueSettings)
            {
                values[pair.Value] = config[pair.Key] ?? string.Empty;
            }

            foreach (var pair in OutputSettings)
            {
                var path = config[pair.Key];
                values[pair.Value] = string.IsNullOrWhiteSpace(path) ? string.Empty : ResolvePath(projectRoot, path);
            }

            // ** Templates: an explicit file (relative to projectRoot), else the profile's templatesDir, else the built-in templates
            var templatesDirectory = ResolvePath(profileDirectory, config["templatesDir"] ?? "templates");
            var templateSources = new List<string>();
            foreach (var template in TemplateSettings)
            {
                var path = ResolveTemplate(config[template.Key], template.FileName, projectRoot, templatesDirectory);
                values[template.Setting] = path;
                templateSources.Add($"{template.FileName,-34} {path}");
            }

            return new LoadedProfile(profileName, profilePath, projectRoot, connectionName, values, templateSources);
        }

        /// <summary>Joins a list setting. Accepts a JSON array or a comma-separated string. Returns null when absent.</summary>
        private static string GetList(IConfiguration config, string key)
        {
            var section = config.GetSection(key);
            var children = section.GetChildren().ToList();

            // ** A JSON array in a later file replaces a string from an earlier file
            if (children.Count > 0) return string.Join(",", children.Select(c => c.Value).Where(v => v != null).Select(v => v.Trim()));
            return section.Value;
        }

        private static string ResolveProfilePath(string profileArg)
        {
            var expanded = Environment.ExpandEnvironmentVariables(profileArg);

            // ** A file or folder path
            if (File.Exists(expanded)) return Path.GetFullPath(expanded);
            if (Directory.Exists(expanded) && File.Exists(Path.Combine(expanded, ProfileFileName))) return Path.GetFullPath(Path.Combine(expanded, ProfileFileName));

            // ** A profile name under a profiles folder
            foreach (var profilesDirectory in FindProfilesDirectories())
            {
                var candidate = Path.Combine(profilesDirectory, profileArg, ProfileFileName);
                if (File.Exists(candidate)) return candidate;
                candidate = Path.Combine(profilesDirectory, profileArg + ".json");
                if (File.Exists(candidate)) return candidate;
            }

            throw new AgcConfigurationException($"Profile '{profileArg}' not found." + Environment.NewLine + DescribeAvailableProfiles());
        }

        /// <summary>
        /// The profiles folders: next to the exe, then the nearest "profiles" folder walking up from it
        /// (the repo's profiles folder when running from bin\Debug), so edits apply without a rebuild.
        /// </summary>
        public static IEnumerable<string> FindProfilesDirectories()
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var directory = new DirectoryInfo(ExeDirectory);
            for (var depth = 0; directory != null && depth < 6; depth++, directory = directory.Parent)
            {
                var candidate = Path.Combine(directory.FullName, "profiles");
                if (Directory.Exists(candidate) && seen.Add(candidate)) yield return candidate;
            }
        }

        private static string DescribeAvailableProfiles()
        {
            var names = FindProfilesDirectories()
                .SelectMany(d => Directory.GetDirectories(d).Where(p => File.Exists(Path.Combine(p, ProfileFileName))).Select(Path.GetFileName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .ToList();
            return names.Count == 0 ? "No profiles folder was found." : "Available profiles: " + string.Join(", ", names);
        }

        private static string ResolveConnection(string connectionName)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var userFile = Path.Combine(appData, "Nuvol9", "AGC", "connections.json");
            var localFile = Path.Combine(ExeDirectory, ConnectionsFileName);

            var connections = new ConfigurationBuilder()
                .AddJsonFile(userFile, optional: true, reloadOnChange: false)
                .AddJsonFile(localFile, optional: true, reloadOnChange: false)
                .AddEnvironmentVariables("AGC_")
                .Build();

            var connectionString = connections["Connections:" + connectionName];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new AgcConfigurationException(
                    $"Connection '{connectionName}' is not defined. Add it to one of:" + Environment.NewLine +
                    $"  {userFile}" + Environment.NewLine +
                    $"  {localFile}" + Environment.NewLine +
                    $"  environment variable AGC_Connections__{connectionName}" + Environment.NewLine +
                    "See connections.sample.json for the format.");
            }

            return connectionString;
        }

        private static string ResolveTemplate(string explicitPath, string fileName, string projectRoot, string templatesDirectory)
        {
            if (!string.IsNullOrWhiteSpace(explicitPath))
            {
                var path = ResolvePath(projectRoot, explicitPath);
                if (!File.Exists(path)) throw new AgcConfigurationException($"Template not found: {path}");
                return path;
            }

            var profileTemplate = Path.Combine(templatesDirectory, fileName);
            if (File.Exists(profileTemplate)) return profileTemplate;

            return Path.Combine(BuiltInTemplatesDirectory, fileName);
        }

        private static string ResolvePath(string baseDirectory, string path)
        {
            var expanded = Environment.ExpandEnvironmentVariables(path.Trim());
            return Path.GetFullPath(Path.IsPathRooted(expanded) ? expanded : Path.Combine(baseDirectory, expanded));
        }

        /// <summary>Server and database of a connection string, without credentials, for the run summary.</summary>
        public static string DescribeConnection(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                return $"{builder.DataSource} / {builder.InitialCatalog}";
            }
            catch (ArgumentException)
            {
                return "(unreadable connection string)";
            }
        }
    }

    public class LoadedProfile
    {
        public LoadedProfile(string name, string profilePath, string projectRoot, string connectionName, IDictionary<string, string> settings, IList<string> templateSources)
        {
            Name = name;
            ProfilePath = profilePath;
            ProjectRoot = projectRoot;
            ConnectionName = connectionName;
            Settings = settings;
            TemplateSources = templateSources;
        }

        public string Name { get; }
        public string ProfilePath { get; }
        public string ProjectRoot { get; }
        public string ConnectionName { get; }
        public IDictionary<string, string> Settings { get; }
        public IList<string> TemplateSources { get; }
    }

    public class AgcConfigurationException : Exception
    {
        public AgcConfigurationException(string message) : base(message) { }
    }
}
