namespace RQI.POC.AutoGen
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public static partial class Utilities
    {
        public static class AutoDALFileSystem
        {
            // Header lines whose values change on every run (the created and modified dates and the copyright year).
            // They are masked before comparing, so a file whose only difference is a header date is left untouched.
            private static readonly Regex VolatileHeaderLines = new Regex(
                @"^(\s*//\s*Document (?:created|Modified) on ).*$|^(\s*//\s*Copyright \(c\) )\d{4}",
                RegexOptions.Multiline | RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // The "Document created on" line, to carry the original created date over when a file is rewritten.
            private static readonly Regex CreatedOnLine = new Regex(
                @"^([ \t]*//[ \t]*Document created on )([^\r\n]*)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            // Every file produced in this run, so stale files can be removed at the end.
            private static readonly HashSet<string> WrittenFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public static int CreatedCount { get; private set; }

            public static int UpdatedCount { get; private set; }

            public static int UnchangedCount { get; private set; }

            public static int RemovedCount { get; private set; }

            /// <summary>
            /// Writes a generated file only when its content changed. A file that differs only in the header
            /// dates or copyright year keeps its existing content (and dates), so git does not see it as changed.
            /// When a file does change, it keeps its original "Document created on" date and gets a new
            /// "Document Modified on" date (when the template has that line).
            /// </summary>
            public static void WriteGeneratedFile(string filePath, string content)
            {
                var fullPath = Path.GetFullPath(filePath);
                WrittenFiles.Add(fullPath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!AgcSettings.DryRun && !string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(fullPath))
                {
                    var existing = File.ReadAllText(fullPath);
                    if (NormalizeForComparison(existing) == NormalizeForComparison(content))
                    {
                        UnchangedCount++;
                        return;
                    }

                    content = KeepCreatedDate(existing, content);
                    UpdatedCount++;
                    if (AgcSettings.DryRun) Console.WriteLine($"|** [dry run] Would update: {fullPath}");
                }
                else
                {
                    CreatedCount++;
                    if (AgcSettings.DryRun) Console.WriteLine($"|** [dry run] Would create: {fullPath}");
                }

                if (AgcSettings.DryRun) return;

                File.WriteAllText(fullPath, content);
            }

            private static string KeepCreatedDate(string existing, string content)
            {
                var existingMatch = CreatedOnLine.Match(existing);
                if (!existingMatch.Success) return content;

                // ** Drop a leftover template tag (e.g. "<DATE FULLasdf>5/14/2025 ...") and keep only the date
                var originalDate = Regex.Replace(existingMatch.Groups[2].Value, @"^<[^>]*>", string.Empty).Trim();
                if (string.IsNullOrEmpty(originalDate)) return content;
                var replaced = false;
                return CreatedOnLine.Replace(content, match =>
                {
                    if (replaced) return match.Value;
                    replaced = true;
                    return match.Groups[1].Value + originalDate;
                });
            }

            private static string NormalizeForComparison(string text)
            {
                if (text == null) return string.Empty;

                var normalized = text.Replace("\r\n", "\n").TrimStart('﻿').Trim();
                return VolatileHeaderLines.Replace(normalized, match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
            }

            /// <summary>
            /// Deletes generated files in the output folders that were not produced by this run
            /// (objects that were dropped or renamed in the database). Call once, after every generator ran.
            /// </summary>
            public static void DeleteStaleGeneratedFiles()
            {
                foreach (var folder in GetGeneratedFolders())
                {
                    if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) continue;

                    var autoGenFiles = Directory.GetFiles(folder, $"*{Utilities.AutogGenFileExtension}", SearchOption.AllDirectories);
                    foreach (var file in autoGenFiles.Where(f => !WrittenFiles.Contains(Path.GetFullPath(f))))
                    {
                        RemovedCount++;
                        if (AgcSettings.DryRun)
                        {
                            Console.WriteLine($"|** [dry run] Would remove stale file: {file}");
                            continue;
                        }

                        File.Delete(file);
                        Console.WriteLine($"|** Removed stale file: {file}");
                    }
                }
            }

            public static void DeletaAllAutoGeneratedFiles(string targetFolder)
            {
                // Check if folder exists.  If not return.
                if (!Directory.Exists(targetFolder)) return;

                var autoGenFiles = Directory.GetFiles(targetFolder, $"*{Utilities.AutogGenFileExtension}", SearchOption.AllDirectories);
                foreach (var file in autoGenFiles)
                {
                    File.Delete(file);
                }
            }

            public static void DeleteAllGeneratedFiles()
            {
                foreach (var folder in GetGeneratedFolders())
                {
                    DeletaAllAutoGeneratedFiles(folder);
                }
            }

            private static IEnumerable<string> GetGeneratedFolders()
            {
                return new[]
                {
                    Utilities.DataModelsRootFilePath,
                    Utilities.DataAccessRootFilePath,
                    Utilities.RepositoryRootFilePath,
                    Utilities.DataEntityRootFilePath,
                    Utilities.ProxyTypeRootFilePath,
                }
                .Where(f => !string.IsNullOrEmpty(f))
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
