# Núvol 9 AutoGenerator (AGC)

Generates the data access layer for a SQL Server database: EF Core entity models for tables and views, user-defined type classes, stored procedure wrappers, critical data type (lookup) proxies, the DbContext, repositories and a unit of work. Generated files end in `.generated.cs` and are written only when their content changes.

One build serves any number of projects. Each project is described by a **profile**; the database connection is looked up by name from a file or environment variable that is never committed.

## Quick start

1. Build `RQI.POC.AutoGenConsole.sln` (Debug or Release).
2. Create `%APPDATA%\Nuvol9\AGC\connections.json` from [`connections.sample.json`](connections.sample.json) and fill in the real connection strings.
3. Run it:

```
RQI.POC.AutoGenConsole.exe --profile solasta --dry-run   # report what would change, write nothing
RQI.POC.AutoGenConsole.exe --profile solasta             # generate
```

Running with no arguments (F5 in Visual Studio) uses `AGC.DefaultProfile` from `App.config`, or the `AGC_PROFILE` environment variable.

## Command line

| Option | Meaning |
| --- | --- |
| `--profile`, `-p <name\|path>` | A profile name under `profiles\`, a folder that holds `agc.profile.json`, or a `.json` file. |
| `--connection`, `-c <name>` | Use this connection instead of the profile's `connection`. |
| `--project-root <path>` | Override the profile's `projectRoot`, e.g. for a second clone. |
| `--only <steps>` | Run only some steps: `tables,views,types,sprocs,critical,dbcontext,repositories,unitofwork`. Stale-file cleanup is skipped. |
| `--dry-run` | Count and list files that would be created, updated or removed, without writing. |

The run starts by printing the profile, project root, server/database and the template used for each artifact, so you can see where output will go before anything is written.

## Profiles

A profile is `profiles/<name>/agc.profile.json` (comments allowed). The profile can also live in the target repo itself (e.g. `.agc/agc.profile.json`) and be passed by path.

| Setting | Meaning |
| --- | --- |
| `name` | Display name. |
| `projectRoot` | Folder that `output` and `templates` paths are relative to. Relative to the profile file, or absolute. `%ENV%` variables are expanded. |
| `connection` | Connection name (see below). |
| `schemas.tables` | Schemas to generate. `views`, `procedures`, `userTypes` and `criticalDataTypes` default to the same list. |
| `criticalDataTypeTables` | `[Schema].[Table]` lookup tables to generate proxy types for. |
| `namespaces.data` / `dbContext` / `unitOfWork` | Namespaces used in the generated code. |
| `dbContext.className` / `connectionStringName` | DbContext class name and the connection string name it reads. |
| `templates.<kind>` | Explicit template file, relative to `projectRoot`. Kinds: `table`, `view`, `userType`, `sproc`, `proxyType`, `dbContext`, `repositoryInterface`, `repository`, `unitOfWork`, `unitOfWorkInterface`. |
| `templatesDir` | Folder (relative to the profile, default `templates`) searched for any template not listed in `templates`. After that, AGC's built-in `Templates\Files` are used. |
| `output.<kind>` | Output folders, relative to `projectRoot`: `dataModels`, `dataAccess`, `repository`, `dataEntity`, `proxyTypes`, plus the files `unitOfWork` and `unitOfWorkInterface`. |
| `include`, `exclude`, `columns`, `sql`, `naming`, `fileExtension` | Filters and naming rules. Defaults come from `RQI.POC.AutoGenConsole/agc.defaults.json`; a profile overrides any of them. |

Lists can be JSON arrays or comma-separated strings. An array in a profile replaces the default list completely.

Profiles in this repo:

- [`profiles/solasta`](profiles/solasta/agc.profile.json): Solasta ERP API (`solasta-lighting-est-api`), with its DbContext, repository and unit of work templates in `profiles/solasta/templates`.
- [`profiles/noah`](profiles/noah/agc.profile.json): NOAH / GBDC.

### Adding a project

1. Copy `profiles/solasta` to `profiles/<new>` (or into the target repo).
2. Set `projectRoot`, `connection`, `schemas`, `namespaces`, `dbContext` and `output`.
3. Put customized templates in its `templates` folder, or point `templates.<kind>` at files in the target repo. Anything not customized uses the built-in templates.
4. Add the connection, then run with `--dry-run` first.

## Connections

Connection strings are never stored in this repo. A connection name is resolved from, last one winning:

1. `%APPDATA%\Nuvol9\AGC\connections.json` (recommended)
2. `connections.local.json` next to the exe (git-ignored)
3. Environment variable `AGC_Connections__<name>`

All files use the format in [`connections.sample.json`](connections.sample.json).
