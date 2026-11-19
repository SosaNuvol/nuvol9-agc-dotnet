# CLAUDE.md: Núvol 9 AutoGenerator (AGC)

AGC reads a SQL Server database's schema and writes C# data-access code into a target project: EF Core entity classes for tables and views, user-defined table type classes, stored procedure wrappers, critical data type (lookup) proxies, a DbContext, repositories and a unit of work. One build serves several projects. The project is chosen at run time by a **profile**.

User-facing setup (profiles, connections, command line) is in [README.md](README.md). What the generated code looks like is in [.ai-Data/PROJECT_CONTEXT.md](.ai-Data/PROJECT_CONTEXT.md) and [.ai-Data/GENERATED_EXAMPLES.md](.ai-Data/GENERATED_EXAMPLES.md). This file is for changing AGC itself or running it for a project.

## Ground rules

- **Work tracking:** open a GitHub issue first, then a PR that says `Closes #<issue>`. The repo is `SosaNuvol/nuvol9-agc-dotnet` and the default branch is `main`.
- **No secrets in git.** Connection strings live only in `%APPDATA%\Nuvol9\AGC\connections.json`, `connections.local.json` (git-ignored) or `AGC_Connections__<name>`. The old Bitbucket history had an `sa` password, and it was scrubbed when the repo moved here.
- **Target framework and language:** .NET Framework 4.8, old-style csproj with `packages.config`, C# 7.3. No nullable reference types, switch expressions, `using var` or target-typed `new`.
- **New files must be listed in the csproj.** Add every new `.cs` file as a `<Compile Include>` in its csproj. Add content files (templates, JSON) as `<Content>` with `CopyToOutputDirectory`.
- **Treat generated output as the contract.** For a change that should not alter the output, the proof is a `--dry-run` against the project's database reporting `created: 0 | updated: 0 | removed: 0`.

## Layout

```
RQI.POC.AutoGenConsole.sln
├── RQI.POC.AutoGen/                    Library: schema reading, settings, naming, file writing
│   ├── AgcSettings.cs                  The run's settings (key → value), filled by the console
│   ├── Utilities.ConfigSettings.cs     Typed getters over AgcSettings (TableSchemaList, *TemplateFilePath, ...)
│   ├── Utilities.SqlStatements.cs      The SQL that reads tables, views, FKs, UDTTs, SPROCs
│   ├── Utilities.FileSystem.cs         WriteGeneratedFile (write-if-changed), stale cleanup, dry run
│   ├── Utilities.Inflector.cs          Singular/plural, SPROC class names
│   ├── Utilities.cs / .Tools.cs        Include/exclude matching, name transforms, type mapping
│   └── Models/Rm*.cs                   Loaders: RmTables, RmViews, RmProcedures, RmUserTypes, RmCriticalDataTypes
├── RQI.POC.AutoGenConsole/             Exe: options, profile loading, generators
│   ├── Program.cs                      Parses options, loads profile, runs the steps in order
│   ├── Configuration/AgcOptions.cs     Command line (--profile, --connection, --project-root, --only, --dry-run)
│   ├── Configuration/ProfileLoader.cs  Profile JSON + defaults + connection → AgcSettings keys
│   ├── agc.defaults.json               Filters and naming rules shared by every profile
│   ├── Templates/TemplateManager.cs    Reads template files, fills header tokens
│   ├── Templates/Files/*.txt           Built-in templates (fallback when a profile has none)
│   └── Utilities/NOAH*Generator.cs     One generator per artifact (names are historical, not NOAH-specific)
├── profiles/<name>/agc.profile.json    One per target project (solasta, noah)
├── profiles/<name>/templates/*.txt     That project's template overrides
└── connections.sample.json             Format for the git-ignored connections file
```

## How a run works

1. `AgcOptions.Parse(args)`. With no `--profile`, it uses `AGC_PROFILE`, then `AGC.DefaultProfile` in App.config (`solasta`).
2. `ProfileLoader.Load` resolves the profile. The argument can be a file, a folder holding `agc.profile.json`, or a name looked up in `profiles\` next to the exe and then in each parent folder, so the repo's `profiles\` is found from `bin\Debug` without a rebuild.
3. It layers `agc.defaults.json` under the profile, using `Microsoft.Extensions.Configuration`. JSON comments are allowed.
4. It flattens everything into the **legacy setting keys** (`RQI.POC.AutoGen.*`, `RQI.GEN.*`, `ConnectionString`) through the mapping tables at the top of `ProfileLoader`: `ListSettings`, `ValueSettings`, `OutputSettings` and `TemplateSettings`. Paths become absolute here.
5. `AgcSettings.Load(...)`. From then on every `Utilities.X` getter reads `AgcSettings.Get(key)`. Nothing reads App.config except `AGC.DefaultProfile`.
6. The `Rm*` loaders query SQL Server, filtered by the schema lists and include/exclude regexes.
7. The generators run in this order, each one skippable with `--only`: `tables`, `views`, `types`, `sprocs`, `critical`, `dbcontext`, `repositories`, `unitofwork`.
8. `AutoDALFileSystem.DeleteStaleGeneratedFiles()` removes `*.generated.cs` files under the output folders that this run didn't produce. It runs only when every step runs.

### Path rules (ProfileLoader)

| Value | Resolved against |
| --- | --- |
| `projectRoot` | The profile file's folder (or absolute). `--project-root` overrides it. |
| `output.*` | `projectRoot` |
| `templates.<kind>` (explicit file) | `projectRoot`. It must exist or loading fails. |
| `templatesDir` | The profile file's folder (default `templates`) |
| Template with no explicit file | `templatesDir/<FileName>`, then `<exe>\Templates\Files\<FileName>` |

`%VAR%` environment variables are expanded in every path.

### Lists

A list is either a JSON array or a comma-separated string. Defaults use strings on purpose: `Microsoft.Extensions.Configuration` merges arrays by index, so a shorter array would leave stray default items. `GetList` prefers array children over a string value, so an array in a profile fully replaces a default string. If `schemas.views`, `procedures`, `userTypes` or `criticalDataTypes` is missing, it falls back to `schemas.tables`. Any missing setting becomes `""`, which matches the old empty App.config entries, so each getter's own default still applies.

## Output layout (where each generator writes)

| Step | Template kind | File |
| --- | --- | --- |
| tables | `table` | `output.dataModels\Models\<Schema>\<SingularName>.generated.cs` |
| views | `table` (see pitfalls) | `output.dataModels\Models\<Schema>\<SingularName>.generated.cs` |
| types | `userType` | `output.dataModels\UDDTs\<Schema>\<ClassName>.generated.cs` |
| sprocs | `sproc` | `output.dataAccess\SPROCS\<Schema>\SP<Name>.generated.cs` |
| critical | `proxyType` | `output.proxyTypes\<Schema>\<TypeName>.generated.cs` |
| dbcontext | `dbContext` | `output.repository\<dbContext.className>.generated.cs` |
| repositories | `repositoryInterface`, `repository` | `output.repository\<Schema>\I<Class>Repository.generated.cs`, `<Class>Repository.generated.cs` |
| unitofwork | `unitOfWork`, `unitOfWorkInterface` | the exact files `output.unitOfWork` and `output.unitOfWorkInterface` |

The folders swept for stale files are `dataModels`, `dataAccess`, `repository`, `dataEntity` and `proxyTypes`. Nothing is written to `dataEntity`, but it is still cleaned.

## Templates

Templates are plain text with `<TOKEN>` placeholders filled by `string.Replace` in each generator. `TemplateManager.BindHeaderInformation` fills these header tokens for every template: `<COMPANY NAME>`, `<YEAR>`, `<WEBSITEURL>`, `<DATE FULL>`, `<DATE MODIFIED>`, `<AUTHOR FULLNAME>`, `<AUTHOR EMAIL>`. The values come from `Templates/Models/HeaderModel.cs`.

| Template | Tokens the generator fills |
| --- | --- |
| table (also views) | `<NAMESPACE> <CLASS_ANNOTATION> <CLASS_NAME> <INHERITANCE_PART> <TABLE_COLUMN_PROPERTIES> <TABLE_FOREIGN_KEYS>` |
| userType | `<NAMESPACE> <CLASS_ANNOTATION> <CLASS_NAME> <INHERITANCE_PART> <TYPE_DEFINITIONS>` |
| sproc | `<NAMESPACE> <SCHEMA> <CLASS_NAME> <INHERITANCE_PART> <SPROC_PARAM_LIST> <METHOD_PARAMS_LIST> <METHOD_PARAMS_LIST_INIT> <METHOD_PARAMS_WITH_TYPES> <PARAMETERS_INIT_LIST> <COMMAND_LINE_EXEC>` |
| proxyType | `<FILENAME> <NAMESPACE> <CLASS_NAME> <PROXY_TYPE_NAME> <PROXY_TYPE_SCHEMA> <PROXY_TYPE_TABLE_NAME> <PROXY_TYPE_DESCRIPTION> <ENUM_DEFINITIONS> <DICTIONARY_DEFINITIONS>` |
| dbContext | `<NAMESPACE> <USING_STATEMENTS> <CLASS_NAME> <CONNECTION_STRING_NAME> <DATABASE_NAME> <DBSET_PROPERTIES> <MODEL_CONFIGURATIONS>` |
| repository / repositoryInterface | `<NAMESPACE> <SCHEMA> <CLASS_NAME> <PLURAL_CLASS_NAME> <PRIMARY_KEY_TYPE> <MODEL_NAMESPACE> <INTERFACE_NAMESPACE> <DBCONTEXT_CLASS_NAME> <DBCONTEXT_NAMESPACE>` |
| unitOfWork / unitOfWorkInterface | `<FILENAME> <NAMESPACE> <REPOSITORY_USING_STATEMENTS> <PRIVATE_DEF_REPOSITORIES> <PUBLIC_REPOSITORY_PROPERTIES> <PUBLIC_REPOSITORY_PROPERTIES_INTERFACE>` |

A template can hard-code project names instead of using tokens. For example, `profiles/solasta/templates/RepositoryTemplate.txt` writes `Solasta.Data.Repository` and `SolastaDbContext` directly. When copying a profile for a new project, check its templates for literal names.

**Solasta's templates are split.** The table, view, userType, sproc and proxyType templates live in the Solasta API repo (`Source/Solasta/Solasta.Data/Templates`, `Solasta.DataAccess/Templates`). The dbContext, repository and unit of work templates live here in `profiles/solasta/templates`. Change whichever copy the run summary prints.

## Common changes

**Add a project.** Copy `profiles/solasta`. Set `projectRoot`, `connection`, `schemas`, `namespaces`, `dbContext` and `output`, then fix literal names in the copied templates. Add the connection to the user's connections file and run `--dry-run`. A profile can also live in the target repo (for example `.agc/agc.profile.json` with `"projectRoot": ".."`) and be passed by path.

**Add a setting.**
1. Add a getter in `Utilities.ConfigSettings.cs` that reads `AgcSettings.Get("RQI.POC.AutoGen.MySetting")` and applies a default.
2. Map a profile key to it in `ProfileLoader`. Use `ListSettings` for lists, `ValueSettings` for scalars, `OutputSettings` for paths under `projectRoot`, and `TemplateSettings` for templates.
3. If every project shares it, put the default in `agc.defaults.json`.
4. Document it in README.md.

**Add a template token.** Add the `.Replace("<TOKEN>", value)` in the generator, then add the token to every template copy that should use it: `Templates/Files`, `profiles/*/templates`, and the Solasta repo's templates.

**Add a generator step.**
1. Add a `NOAH*Generator : GeneratorBase, IGenerator` class.
2. Add a template kind to `TemplateSettings`, a getter to `Utilities` and a property to `TemplateManager`.
3. Add an output key if the step needs a new folder, and add that folder to `GetGeneratedFolders()` in `Utilities.FileSystem.cs` so stale files get cleaned.
4. Add the step name to `AgcOptions.Steps` and wrap it in `options.ShouldRun("<step>")` in `Program.cs`.
5. Write files only through `AutoDALFileSystem.WriteGeneratedFile`, so write-if-changed, dry run and stale cleanup keep working.

**Change what is read from SQL.** The queries are in `Utilities.SqlStatements.cs`. The schema lists are injected as `@..._SCHEMA_LIST@` placeholders. Each loader in `Models/Rm*.cs` maps reader columns to `Models/Base/*`. SPROC parameter defaults come from parsing the procedure's `CREATE [OR ALTER] PROCEDURE` text, because SQL Server doesn't expose them.

## Generating code for a project (the usual task)

1. Make sure the database has the schema change, and that the schema is listed in the profile's `schemas.tables`. A new schema, such as MSG, has to be added there.
2. Run `RQI.POC.AutoGenConsole.exe --profile <name> --dry-run` and read the summary and the "Would create/update/remove" lines.
3. Run it without `--dry-run`. Only changed files are rewritten. They keep their original "Document created on" date and get a new "Document Modified on" date. Files for dropped objects are deleted.
4. Build the target project and commit the `.generated.cs` changes there, following that repo's rules.

A file that differs only in its header dates or copyright year is left untouched, so `git status` in the target repo shows only real changes.

## Building and verifying

- **On Windows:** open the solution in Visual Studio 2022 and build Debug or Release. The old "Dev" configuration and SlowCheetah transforms were removed. Run from `RQI.POC.AutoGenConsole\bin\Debug`. F5 uses the default profile.
- **On Linux (no dotnet or msbuild for net48):** a compile check is possible with Mono's `mcs` (`apt-get install mono-mcs mono-devel`). Download the `net462` builds of the referenced NuGet packages and compile each project's `<Compile Include>` files with `-langversion:latest`. `mcs` is stricter than Roslyn: it rejects C# 7 local functions, so use private static methods instead. To check settings without a database, call `ProfileLoader.Load(AgcOptions.Parse(new[]{"-p","solasta","--project-root",dir}))` and print `Settings`.
- **Regression check:** run `--profile <name> --dry-run` against the real database and confirm the last line reports 0 created, 0 updated and 0 removed.

## Pitfalls

- **Views use the table template.** `Program.cs` generates views with `NOAHTablesGenerator`, so the `view` template (`ViewModel.txt`) and `NOAHViewsGenerator` are currently unused.
- **The file extension is hard-coded.** File names hard-code `.generated.cs`, while `fileExtension` (`AutogGenFileExtension`) is only used to find stale files. Keep the two in sync.
- **Paths are Windows-only.** Generators build paths with `\\` string concatenation.
- **Audit columns have a known mismatch.** The getter reads `RQI.POC.AutoGen.AuditColumns` (profile `columns.audit`). The old App.config set `AuditorColumns`, which nothing read, so no profile sets audit columns today. Setting them would change the output.
- **`--only` never deletes.** Stale cleanup is skipped with `--only`, so files for dropped objects stay until a full run.
- **Repositories are tables only.** `repositories`, `dbcontext` and `unitofwork` are built from tables, not views.
- **Several namespaces are hard-coded in generators.** For example, `NOAHTablesGenerator` uses `"{namespaces.data}.Models.{Schema}"`. Grep the generator before assuming a namespace comes from the profile.
- **Some naming is unused or misspelled.** `RQI.GEN.DbContextNamespance` is a long-standing typo that is used as the key, so keep it. The `NOAH` prefixes and `RQI.POC` names are historical.
