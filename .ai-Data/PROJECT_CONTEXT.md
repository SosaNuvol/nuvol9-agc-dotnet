# RQI.POC.AutoGenConsole — AI Context Document

> **Purpose of this file:** Provide consuming projects and AI assistants with a complete understanding of what this tool is, what it produces, and how to interpret or use its output.
>
> → **See [GENERATED_EXAMPLES.md](./GENERATED_EXAMPLES.md)** for real, annotated code samples of every artifact type (entity models, views, SPROC wrappers, DbContext, repositories, Unit of Work, proxy lookups).

---

## Overview

**RQI.POC.AutoGenConsole** is a .NET Framework 4.8 console application built by **Núvol 9 (Andres Sosa)** that acts as a **DAL (Data Access Layer) code generator**. Given a SQL Server database connection, it introspects the schema and emits fully-formed, ready-to-compile C# source files targeting **Entity Framework Core**.

The tool removes the need to hand-author boilerplate EF entity models, repositories, a Unit of Work, and a `DbContext`. Every generated file is stamped with the extension `.generated.cs` to make it clear it is machine-produced and should not be edited by hand.

---

## Solution Structure

```
RQI.POC.AutoGenConsole.sln
├── RQI.POC.AutoGen/            # Core library — schema introspection & shared utilities
│   ├── Models/
│   │   ├── Base/               # Domain models: Table, Column, SP, SPParam, UserDefinedType, etc.
│   │   ├── RmTables.cs         # Reads table metadata from SQL Server
│   │   ├── RmViews.cs          # Reads view metadata
│   │   ├── RmProcedures.cs     # Reads stored procedure metadata + parses parameters/result sets
│   │   ├── RmUserTypes.cs      # Reads User-Defined Data Types (UDDTs)
│   │   └── RmCriticalDataTypes.cs  # Reads "lookup/enum" tables designated in config
│   └── Utilities.*.cs          # Config settings, SQL statements, inflector, file system helpers, logging
│
└── RQI.POC.AutoGenConsole/     # Console runner — orchestrates generation
    ├── Program.cs              # Entry point; runs all generators in sequence
    ├── App.config              # All runtime settings (connection string, namespaces, paths, schemas)
    ├── Templates/
    │   ├── TemplateManager.cs  # Loads .txt template files; binds header tokens
    │   └── Files/              # Template .txt files (one per artifact type)
    │       ├── TableModel.txt
    │       ├── ViewModel.txt
    │       ├── UDDTModel.txt
    │       ├── SPROC.txt
    │       ├── ProxyTableLookupModel.txt
    │       ├── DbContextTemplate.txt
    │       ├── RepositoryInterfaceTemplate.txt
    │       ├── RepositoryTemplate.txt
    │       ├── UnitOfWorkTemplate.txt
    │       └── UnitOfWorkInterfaceTemplate.txt
    └── Utilities/              # One generator class per artifact type
        ├── GeneratorBase.cs           # Shared: column building, FK navigation, file saving
        ├── GeneratorBase.Columns.cs   # Column-to-property rendering with EF data annotations
        ├── IGenerator.cs              # Common interface (Generate method)
        ├── AutoGenConsoleSettings.cs  # Typed wrappers around App.config keys
        ├── NOAHTablesGenerator.cs     # Generates EF entity classes for Tables and Views
        ├── NOAHSPROCSGenerator.cs     # Generates SPROC wrapper classes
        ├── NOAHUDTypesGenerator.cs    # Generates UDDT classes
        ├── NOAHCriticalDataGenerator.cs  # Generates proxy/enum lookup classes
        ├── NOAHDbContextGenerator.cs  # Generates the DbContext class
        ├── NOAHRepositoryGenerator.cs # Generates Repository + IRepository per table
        └── NOAHUnitOfWorkGenerator.cs # Generates UnitOfWork + IUnitOfWork
```

---

## What the Tool Generates

### 1. Entity Models (Tables & Views)
- One `.generated.cs` file per table/view, named after the **singularized** class name (e.g., `Account.generated.cs`).
- Decorated with `[Table("TableName", Schema = "SchemaName")]`.
- Each column becomes a `public virtual {CSharpType}? {PropertyName} { get; set; }` with:
  - `[Column("ColumnName", TypeName="sql_type")]`
  - `[Required]` when the column is NOT NULL
  - `[Key]` for primary keys
  - `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` for auto-increment PKs
  - `[ForeignKey("RelatedClassName")]` for FK columns
- Navigation properties are emitted in a `#region Foreign Keys` block.
- **Historical tables** — tables that contain all six audit columns (`IsActive`, `IsDeleted`, `CreatedById`, `CreatedDate`, `ModifiedById`, `ModifiedDate`) — inherit from `HistoricalEntity` and those columns are **omitted** from the generated properties (they live in the base class).
- Output path: `{RQI.GEN.DataModelsRootFilePath}\Models\{Schema}\`

### 2. SPROC Wrapper Classes
- One class per stored procedure.
- Contains a single async method whose signature mirrors the SPROC parameters (each `SqlParameter` is built from method arguments).
- If the SPROC returns rows, uses `FromSqlRaw(...).ToListAsync()` returning `List<{ReturnType}>`.
- If the SPROC returns nothing, uses `ExecuteSqlRawAsync(...)` returning `int` (rows affected).
- Output path: `{RQI.GEN.DataAccessRootFilePath}\{Schema}\`

### 3. UDDT (User-Defined Data Type) Classes
- One `[Keyless]` EF class per UDDT.
- Table types become classes with multiple `[Column]`-decorated properties.
- Scalar types become a class with a single `Value` property.
- Output path: `{RQI.GEN.DataModelsRootFilePath}\UDDTs\{Schema}\`

### 4. Proxy / Critical Data Type Lookup Classes
- Generated for tables explicitly listed in `RQI.POC.AutoGen.CriticalDataTypesTablesList`.
- Produces a `{TableNameSingular}ProxyLookup.generated.cs` containing an `enum` and a static `Dictionary` for fast in-memory lookups of reference/lookup table data.
- Output path: `{RQI.GEN.ProxyTypeRootFilePath}\{Schema}\`

### 5. DbContext
- A single `{DbContextClassName}.generated.cs` (default: `NOAHDbContext`).
- Contains one `DbSet<{EntityClass}>` property per table, organized in `#region DbSets - {Schema}` blocks.
- `using` statements are auto-generated for each schema namespace.
- Output path: `{RQI.GEN.DataModelsRootFilePath}\`

### 6. Repository Interface & Implementation
- Per table: `I{Entity}Repository.generated.cs` and `{Entity}Repository.generated.cs`.
- Interface exposes standard CRUD methods with the detected primary key type.
- Implementation extends a base `Repository<T>` class, injecting the DbContext.
- Organized by schema sub-folder.
- Output path: `{RQI.GEN.RepositoryRootFilePath}\{Schema}\`

### 7. Unit of Work
- Two files: `IUnitOfWork.generated.cs` and `UnitOfWork.generated.cs`.
- Aggregates **all** generated repositories as public properties.
- Groups repositories by schema in `#region` blocks.
- Provides a single `SaveChanges` / `SaveChangesAsync` entry point.
- Output path: resolved from `RQI.GEN.UnitOfWorkNameSpace` / `RQI.GEN.UnitOfWorkTemplateFilePath`.

---

## Template Token System

All `.txt` template files use `<TOKEN_NAME>` placeholders replaced at generation time:

| Token | Description |
|---|---|
| `<NAMESPACE>` | Fully-qualified C# namespace |
| `<CLASS_NAME>` | Singular PascalCase class name |
| `<PLURAL_CLASS_NAME>` | Plural form of the class name |
| `<CLASS_ANNOTATION>` | EF `[Table(...)]` or `[Keyless]` annotation |
| `<INHERITANCE_PART>` | ` : HistoricalEntity` or empty |
| `<TABLE_COLUMN_PROPERTIES>` | Auto-generated property block |
| `<TABLE_FOREIGN_KEYS>` | Navigation property block |
| `<DBSET_PROPERTIES>` | `DbSet<T>` declarations |
| `<USING_STATEMENTS>` | `using` directives for schema namespaces |
| `<SPROC_PARAM_LIST>` | SQL parameter placeholder string (e.g., `@Param1, @Param2`) |
| `<METHOD_PARAMS_WITH_TYPES>` | C# method signature parameters |
| `<PARAMETERS_INIT_LIST>` | `SqlParameter` variable declarations |
| `<METHOD_PARAMS_LIST>` | Variable references passed to `FromSqlRaw` |
| `<COMMAND_LINE_EXEC>` | The raw SQL execution line |
| `<@RETURN_TYPE@>` | EF entity type for `FromSqlRaw` return |
| `<SCHEMA>` | SQL schema name |
| `<REPOSITORY_USING_STATEMENTS>` | `using` for repository namespaces |
| `<PRIVATE_DEF_REPOSITORIES>` | Private repository field declarations in UoW |
| `<PUBLIC_REPOSITORY_PROPERTIES>` | Public repository properties in UoW class |
| `<PUBLIC_REPOSITORY_PROPERTIES_INTERFACE>` | Public repository properties in IUoW |
| `<FILENAME>`, `<COMPANY NAME>`, `<YEAR>`, `<AUTHOR FULLNAME>`, etc. | File header tokens injected by `TemplateManager` |

---

## Configuration Reference (`App.config`)

All settings live under `<appSettings>`.

### Connection
| Key | Description |
|---|---|
| `ConnectionString` | SQL Server connection string for the source database |

### Schema Filters (comma-separated schema names)
| Key | Description |
|---|---|
| `RQI.POC.AutoGen.TableSchemaList` | Schemas to scan for tables |
| `RQI.POC.AutoGen.ViewsSchemaList` | Schemas to scan for views |
| `RQI.POC.AutoGen.StPrcSchemaList` | Schemas to scan for stored procedures |
| `RQI.POC.AutoGen.StUDTSchemaList` | Schemas to scan for UDDTs |
| `RQI.POC.AutoGen.CriticalDataTypesSchemaList` | Schemas for proxy/enum lookup tables |

### Include / Exclude Filters (regex or `*` for all)
| Key | Description |
|---|---|
| `RQI.POC.AutoGen.IncludeTableList` | Tables to include (`*` = all) |
| `RQI.POC.AutoGen.ExcludeTableList` | Tables to exclude (comma-separated; regex supported) |
| `RQI.POC.AutoGen.IncludeViewList` | Views to include |
| `RQI.POC.AutoGen.ExcludeViewList` | Views to exclude |
| `RQI.POC.AutoGen.IncludeProcedureList` | SPROCs to include |
| `RQI.POC.AutoGen.ExcludeProcedureList` | SPROCs to exclude |

### Critical Data Types
| Key | Description |
|---|---|
| `RQI.POC.AutoGen.CriticalDataTypesTablesList` | Fully-qualified `[Schema].[Table]` list of lookup tables to generate proxy types for |

### Name Transformation
| Key | Description |
|---|---|
| `RQI.POC.AutoGen.RegexDictionaryReplace` | Semicolon-separated `pattern,replacement` pairs applied to SQL names |
| `RQI.POC.AutoGen.StripText` | Regex prefixes to strip from names |
| `RQI.POC.AutoGen.SkipPluralNamesList` | Words that should not be pluralized |
| `RQI.POC.AutoGen.WordsShouldEndES` | Words that pluralize with `-es` |

### Column Behavior
| Key | Description |
|---|---|
| `RQI.POC.AutoGen.HistoricalColumns` | Column names that identify a "historical" (audited) table |
| `RQI.POC.AutoGen.AuditorColumns` | Additional auditor columns to suppress from output |
| `RQI.POC.AutoGen.SQL.SystemColToIgnoreList` | System columns to always skip |

### Output Paths & Namespaces
| Key | Description |
|---|---|
| `RQI.GEN.OutputBaseDir` | Root output directory |
| `RQI.GEN.BaseNameSpace` | Top-level namespace prefix |
| `RQI.GEN.DataDataNameSpace` | Namespace for data models (e.g., `NOAH.Data`) |
| `RQI.GEN.DataAccessNameSpace` | Namespace for data access classes |
| `RQI.GEN.DbContextNamespance` | Namespace for the generated DbContext |
| `RQI.GEN.DbContextFileName` | Class name for the DbContext (e.g., `NOAHDbContext`) |
| `RQI.GEN.DbContextConnectionStringName` | Connection string name used in DbContext constructor |
| `RQI.GEN.UnitOfWorkNameSpace` | Namespace for the Unit of Work |
| `RQI.GEN.DataModelsRootFilePath` | Filesystem root for model output |
| `RQI.GEN.DataAccessRootFilePath` | Filesystem root for data access output |
| `RQI.GEN.RepositoryRootFilePath` | Filesystem root for repository output |
| `RQI.GEN.Data.EntityRootFilePath` | Filesystem root for entity output |
| `RQI.GEN.ProxyTypeRootFilePath` | Filesystem root for proxy/lookup output |

### Template File Paths
| Key | Points to |
|---|---|
| `RQI.GEN.TableTemplateFilePath` | `TableModel.txt` |
| `RQI.GEN.ViewTemplateFilePath` | `ViewModel.txt` |
| `RQI.GEN.UDDTTemplateFilePath` | `UDDTModel.txt` |
| `RQI.GEN.SPROCTemplateFilePath` | `SPROC.txt` |
| `RQI.GEN.ProxyTypeTemplateFilePath` | `ProxyTableLookupModel.txt` |
| `RQI.GEN.DbContextTemplateFilePath` | `DbContextTemplate.txt` |
| `RQI.GEN.RepositoryInterfaceTemplateFilePath` | `RepositoryInterfaceTemplate.txt` |
| `RQI.GEN.RepositoryTemplateFilePath` | `RepositoryTemplate.txt` |
| `RQI.GEN.UnitOfWorkTemplateFilePath` | `UnitOfWorkTemplate.txt` |
| `RQI.GEN.UnitOfWorkInterfaceTemplateFilePath` | `UnitOfWorkInterfaceTemplate.txt` |

---

## Generation Pipeline (Program.cs execution order)

```
1. Delete all previously generated files  (AutoDALFileSystem.DeleteAllGeneratedFiles)
2. Load from SQL Server:
   - Tables   → RmTables.Load()
   - Views    → RmViews.LoadViews()
   - SPROCs   → RmProcedures.Load()
   - UDDTs    → RmUserTypes.LoadUserDefinedTypes()
   - Critical → RmCriticalDataTypes.LoadCriticalDataTypes()
3. Generate Table entity models          (NOAHTablesGenerator)
4. Generate View entity models           (NOAHTablesGenerator, reused with viewsResult)
5. Generate UDDT classes                 (NOAHUDTypesGenerator)
6. Generate SPROC wrapper classes        (NOAHSPROCSGenerator)
7. Generate Proxy / Enum lookup classes  (NOAHCriticalDataGenerator)
8. Generate DbContext                    (NOAHDbContextGenerator)
9. Generate Repositories + Interfaces    (NOAHRepositoryGenerator)
10. Generate Unit of Work + Interface    (NOAHUnitOfWorkGenerator)
```

---

## Key Concepts for Consuming Projects

### Generated file conventions
- All generated files end in `.generated.cs` — **do not edit them manually**.
- They are `partial class` declarations so you can extend them in a non-generated partial file.
- Re-running the tool will **delete and regenerate** all `.generated.cs` files.

### Schema namespacing
The tool organizes output by SQL schema. For a `BaseNameSpace` of `NOAH` and a schema `ACC`:
- Models live under `NOAH.Data.Models.ACC`
- Repositories under the configured `RepositoryRootFilePath\ACC`
- SPROC wrappers under `NOAH.Data.Models.ACC` (or the data access path)

### Historical / Audited entities
If a table has all six audit columns it is considered "historical" and the generated entity inherits from `HistoricalEntity`. Those six columns are not re-declared on the class. The consuming project must supply the `HistoricalEntity` base class.

### Repository pattern
Each generated `I{Entity}Repository` extends a generic base repository. The consuming project must provide that base `Repository<T>` class and the `DbContext` injection infrastructure.

### Unit of Work
`IUnitOfWork` / `UnitOfWork` are single-file aggregates. Register `UnitOfWork` in DI scoped to request lifetime; inject `IUnitOfWork` wherever repository access is needed.

### Proxy Lookup Types
Tables listed under `CriticalDataTypesTablesList` are treated as static reference data. The generator emits a `{TableNameSingular}ProxyLookup` class containing an `enum` and a `Dictionary<int, string>` (or similar) for zero-query lookups at runtime.

---

## Technology Stack

| Component | Technology |
|---|---|
| Language | C# (.NET Framework 4.8) |
| DB introspection | ADO.NET (`IDataReader` over SQL Server) |
| Target ORM | Entity Framework Core |
| JSON parsing (SPROC params) | Newtonsoft.Json |
| DI (internal) | Unity |
| Configuration | `System.Configuration` / `App.config` |
| Name inflection | Custom `Inflector` utility (singularize, pluralize, camelCase, PascalCase) |

---

## Typical Usage Workflow

1. Point `ConnectionString` in `App.config` at the target SQL Server database.
2. Set the schema lists (`TableSchemaList`, `StPrcSchemaList`, etc.) to the schemas you want to cover.
3. Set the output path keys (`DataModelsRootFilePath`, `RepositoryRootFilePath`, etc.) to the target project folders.
4. Set the namespace keys to match the target project's namespace conventions.
5. Verify include/exclude filter lists.
6. Run the console application.
7. Review the console output (color-coded: green = success item, blue = column detail, yellow = SPROC params, red = error).
8. The target project now contains fresh `.generated.cs` files ready to compile with EF Core.
