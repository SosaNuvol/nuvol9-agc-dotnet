# Generated Code Examples

> **Purpose:** Real, annotated examples of every artifact type produced by `RQI.POC.AutoGenConsole`. All samples are drawn from the **Solasta** project output (Dev config target). Use these to understand the shape, naming conventions, and dependencies of generated files before consuming or extending the DAL.
>
> **Parent document:** [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md)

---

## Table of Contents

- [Generated Code Examples](#generated-code-examples)
  - [Table of Contents](#table-of-contents)
  - [1. Table Entity Model — with FK](#1-table-entity-model--with-fk)
  - [2. View Entity Model](#2-view-entity-model)
  - [3. Stored Procedure Wrapper](#3-stored-procedure-wrapper)
  - [4. DbContext (partial)](#4-dbcontext-partial)
  - [5. Repository Interface](#5-repository-interface)
  - [6. Repository Implementation](#6-repository-implementation)
  - [7. Unit of Work Interface (excerpt)](#7-unit-of-work-interface-excerpt)
  - [8. Unit of Work Implementation (excerpt)](#8-unit-of-work-implementation-excerpt)
  - [9. Proxy Lookup](#9-proxy-lookup)
  - [Cross-Cutting Conventions](#cross-cutting-conventions)

---

## 1. Table Entity Model — with FK

**File:** `Solasta.Data\Models\MAC\Estimate.generated.cs`  
**Schema:** `MAC` | **Table:** `Estimates`  
**Inherits:** `HistoricalEntity` (provides audit columns — CreatedAt, CreatedBy, ModifiedAt, ModifiedBy, etc.)

Key observations:
- Namespace pattern: `{RootNamespace}.Models.{Schema}`
- `[Table("Estimates", Schema = "MAC")]` maps the class to the correct schema-qualified table.
- The PK column carries both `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` and `[Key]`.
- FK columns get a `[ForeignKey("NavigationPropertyName")]` attribute on the scalar column (not the nav property).
- Nullable columns map to nullable C# types (`string?`, `decimal?`).
- Required columns use `[Required]` and non-nullable types.
- `TypeName` in `[Column]` reflects the raw SQL Server type.
- The navigation property appears in a separate `#region Foreign Keys` block.

```csharp
namespace Solasta.Data.Models.MAC;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Solasta.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Strongly-typed model for the Estimate class.
/// </summary>
[Table("Estimates", Schema = "MAC")]
public partial class Estimate : HistoricalEntity
{
    #region Columns

    [ForeignKey("MACCustomer")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
    [Column("Id", TypeName="bigint"), Required]
    public virtual long Id { get; set; }

    [Column("Proxy", TypeName="nvarchar"), Required]
    public virtual string Proxy { get; set; }

    [Column("PartKey", TypeName="int"), Required]
    public virtual int PartKey { get; set; }

    [Column("TypeId", TypeName="int"), Required]
    public virtual int TypeId { get; set; }

    [Column("CustomerId", TypeName="bigint"), Required]
    public virtual long CustomerId { get; set; }

    [Column("EstimateNumber", TypeName="nvarchar"), Required]
    public virtual string EstimateNumber { get; set; }

    [Column("EstimateDate", TypeName="date"), Required]
    public virtual DateTime EstimateDate { get; set; }

    [Column("ProjectName", TypeName="nvarchar")]
    public virtual string? ProjectName { get; set; }

    [Column("ProjectDescription", TypeName="nvarchar")]
    public virtual string? ProjectDescription { get; set; }

    [Column("TotalAmount", TypeName="decimal")]
    public virtual decimal? TotalAmount { get; set; }

    [Column("Status", TypeName="nvarchar")]
    public virtual string? Status { get; set; }

    [Column("Notes", TypeName="nvarchar")]
    public virtual string? Notes { get; set; }

    #endregion Columns

    #region Foreign Keys
    public virtual MACCustomer MACCustomer { get; set; }
    #endregion
}
```

---

## 2. View Entity Model

**File:** `Solasta.Data\Models\RSU\Gen_UserResourcesView.generated.cs`  
**Schema:** `RSU` | **View:** `vwGen_UserResources`

Key observations:
- Views follow the same namespace/attribute pattern as tables.
- Class name convention for views: `{SqlViewName}View` (the `vw` prefix is stripped).
- Views do **not** inherit `HistoricalEntity` — they map the view columns directly.
- No `[Key]` or `[DatabaseGenerated]` on view columns (no PK concept).
- No FK navigation properties — views are read-only flattened projections.
- Nullable view columns map to nullable C# types.

```csharp
namespace Solasta.Data.Models.RSU;

using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Solasta.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Strongly-typed model for the Gen_UserResourcesView class.
/// </summary>
[Table("vwGen_UserResources", Schema = "RSU")]
public partial class Gen_UserResourcesView
{
    #region Columns

    [Column("UserResourceID", TypeName="int"), Required]
    public virtual int UserResourceID { get; set; }

    [Column("DealerId", TypeName="int"), Required]
    public virtual int DealerId { get; set; }

    [Column("UserId", TypeName="uniqueidentifier")]
    public virtual Guid? UserId { get; set; }

    [Column("UserEmployeeTypeId", TypeName="varchar"), Required]
    public virtual string UserEmployeeTypeId { get; set; }

    [Column("UserResourceAddressId", TypeName="int")]
    public virtual int? UserResourceAddressId { get; set; }

    [Column("RecruitedById", TypeName="int")]
    public virtual int? RecruitedById { get; set; }

    [Column("GPEmployeeId", TypeName="nvarchar"), Required]
    public virtual string GPEmployeeId { get; set; }

    [Column("RecruitedByDate", TypeName="datetimeoffset"), Required]
    public virtual DateTimeOffset RecruitedByDate { get; set; }

    [Column("FullName", TypeName="nvarchar")]
    public virtual string? FullName { get; set; }

    [Column("PublicFullName", TypeName="nvarchar")]
    public virtual string? PublicFullName { get; set; }

    [Column("SSN", TypeName="nvarchar")]
    public virtual string? SSN { get; set; }

    #endregion Columns
}
```

---

## 3. Stored Procedure Wrapper

**File:** `Solasta.DataAccess\SPROCS\RSU\UserResourceFullGetByIdSPROC.generated.cs`  
**Schema:** `RSU` | **SPROC:** `spUserResourceFullGetById`

Key observations:
- Namespace: `{RootNamespace}.Models.{Schema}` (same as models — SPROC wrappers live alongside model-layer utilities).
- The class receives a `DbContext` via constructor injection.
- The entry point is always `CallStoredProcedureAsync(...)` returning `DSMEnvelope<List<T>>`.
- `DSMEnvelope` is a result-wrapper type from the `NVL9.DSM.Core` library — carries either a success payload or a captured exception.
- Parameters are mapped to `SqlParameter` objects using `@ParamName` convention matching the SPROC signature.
- `FromSqlRaw` is used to execute the SPROC and materialize the result set.
- Both `SqlException` and general `Exception` are caught and captured into the envelope — callers check `envelope.IsSuccess`.

```csharp
namespace Solasta.Data.Models.RSU;

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using NVL9.DSM.Core;
using NOAH.Data.UDDTs.RSU;

/// <summary>
/// Strongly-typed Stored Procedure implementation for the UserResourceFullGetByIdSPROC class.
/// </summary>
public partial class UserResourceFullGetByIdSPROC
{
    private readonly DbContext _dbContext;

    public UserResourceFullGetByIdSPROC(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DSMEnvelope<List<>>> CallStoredProcedureAsync(int userresourceid)
    {
        var envelope = DSMEnvelopeManager.Instance.InitEnvelope<List<>>(userresourceid);

        try
        {
            var userresourceidParam = new SqlParameter("@UserResourceId", userresourceid);

            var result = await _dbContext.Set<>()
                .FromSqlRaw("EXEC [RSU].[spUserResourceFullGetById] @UserResourceId;", userresourceidParam)
                .ToListAsync();

            envelope.Success(result);
        }
        catch(SqlException sqlEx)
        {
            envelope.CaptureException(sqlEx);
        }
        catch (Exception ex)
        {
            envelope.CaptureException(ex);
        }

        return envelope;
    }
}
```

> **Note:** The `<>` type argument placeholders in `DSMEnvelope<List<>>` and `_dbContext.Set<>()` indicate the return-type entity is a generator token that gets resolved to the correct model class at generation time. In the emitted file for this SPROC, these are bound to the specific result-set type derived from the SPROC's output columns.

---

## 4. DbContext (partial)

**File:** `Solasta.Data.Repository\SolastaDbContext.generated.cs`

Key observations:
- The generator emits a `partial class` — the non-generated half (hand-authored) handles `OnConfiguring` and `OnModelCreating`.
- One `DbSet<T>` is emitted per table/view across **all** configured schemas.
- `DbSet` properties use the **plural form** of the entity name.
- Sections are grouped by schema using `#region DbSets - {Schema} (Schema)` blocks.
- Imports cover all model namespaces so entity types resolve without qualification.

```csharp
namespace Solasta.Data.Repository;

using Microsoft.EntityFrameworkCore;
using Solasta.Data;
using Solasta.Data.Models.ACC;
using Solasta.Data.Models.ACE;
using Solasta.Data.Models.MAC;
using Solasta.Data.Models.QAL;
using Solasta.Data.Models.RSU;

/// <summary>
/// Strongly-typed DbContext for the Solasta database.
/// </summary>
public partial class SolastaDbContext
{
    #region DbSets - ACC (Schema)

    /// <summary>Gets or sets the Dealers DbSet.</summary>
    public virtual DbSet<Dealer> Dealers { get; set; }

    /// <summary>Gets or sets the DealerTenants DbSet.</summary>
    public virtual DbSet<DealerTenant> DealerTenants { get; set; }

    /// <summary>Gets or sets the DealerTenantTypes DbSet.</summary>
    public virtual DbSet<DealerTenantType> DealerTenantTypes { get; set; }

    /// <summary>Gets or sets the Roles DbSet.</summary>
    public virtual DbSet<Role> Roles { get; set; }

    /// <summary>Gets or sets the Users DbSet.</summary>
    public virtual DbSet<User> Users { get; set; }

    #endregion DbSets - ACC (Schema)

    #region DbSets - ACE (Schema)

    /// <summary>Gets or sets the ACECustomers DbSet.</summary>
    public virtual DbSet<ACECustomer> ACECustomers { get; set; }

    /// <summary>Gets or sets the ACECustomerTypes DbSet.</summary>
    public virtual DbSet<ACECustomerType> ACECustomerTypes { get; set; }

    /// <summary>Gets or sets the CustomerAddresses DbSet.</summary>
    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }

    /// <summary>Gets or sets the MasterFileAccounts DbSet.</summary>
    public virtual DbSet<MasterFileAccount> MasterFileAccounts { get; set; }

    /// <summary>Gets or sets the MasterFiles DbSet.</summary>
    public virtual DbSet<MasterFile> MasterFiles { get; set; }

    #endregion DbSets - ACE (Schema)

    // ... MAC, QAL, RSU DbSets follow the same pattern ...
}
```

---

## 5. Repository Interface

**File:** `Solasta.Data.Repository\ACC\IDealerRepository.generated.cs`

Key observations:
- Namespace: `{RootNamespace}.Repository.{Schema}`
- The interface is `partial` — consuming projects can extend it with custom query methods.
- Inherits both `IWriteableRepository<T>` and `IQueryableRepository<T>` from `Solasta.Data.Entity.Repositories`.
  - `IWriteableRepository<T>` — provides Add/Update/Delete/SaveChanges.
  - `IQueryableRepository<T>` — provides GetById, GetAll, Find, etc.
- Body is intentionally empty — all base CRUD is inherited. Custom methods go in the non-generated partial.

```csharp
namespace Solasta.Data.Repository.ACC;

using Solasta.Data.Entity.Repositories;
using Solasta.Data.Repository;
using Solasta.Data.Models.ACC;

/// <summary>
/// Repository interface for Dealers entity.
/// </summary>
public partial interface IDealerRepository : IWriteableRepository<Dealer>, IQueryableRepository<Dealer>
{
}
```

---

## 6. Repository Implementation

**File:** `Solasta.Data.Repository\ACC\DealerRepository.generated.cs`

Key observations:
- Namespace: `{RootNamespace}.Repository.{Schema}`
- Inherits the base `Repository<T>` class (from `Solasta.Data.Repository`) which implements CRUD via EF Core.
- Also implements the generated interface `IDealerRepository`.
- Constructor takes the project-specific `SolastaDbContext` and passes it to `base(context)`.
- Body is empty by design — all logic lives in the base. Custom overrides go in a hand-authored `partial` file.

```csharp
namespace Solasta.Data.Repository.ACC;

using Solasta.Data.Repository;
using Solasta.Data.Models.ACC;

/// <summary>
/// Repository implementation for Dealers entity.
/// </summary>
public partial class DealerRepository : Repository<Dealer>, IDealerRepository
{
    public DealerRepository(SolastaDbContext context) : base(context)
    {
    }
}
```

---

## 7. Unit of Work Interface (excerpt)

**File:** `Solasta.DataAccess\IUnitOfWork.generated.cs`

Key observations:
- Namespace: `{RootNamespace}Access` (e.g., `Solasta.DataAccess`).
- `partial interface` — can be extended by hand.
- Inherits `IDisposable` to ensure the underlying DbContext is released.
- One read-only property per repository, grouped by schema using `#region {Schema} Repositories`.
- Property names use the **plural form** of the entity name (matching DbSet names in DbContext).
- Also declares `SaveChangesAsync()` and transaction helpers (in the non-shown tail of the file).

```csharp
namespace Solasta.DataAccess;

using Solasta.Data.Entity;
using Solasta.Data.Entity.Repositories;
using Solasta.Data.Repository.ACC;
using Solasta.Data.Repository.ACE;
using Solasta.Data.Repository.MAC;
using Solasta.Data.Repository.QAL;
using Solasta.Data.Repository.RSU;

/// <summary>
/// Unit of Work pattern interface for managing database transactions.
/// </summary>
public partial interface IUnitOfWork : IDisposable
{
    #region ACC Repositories

    /// <summary>Gets the Dealer repository.</summary>
    IDealerRepository Dealers { get; }

    /// <summary>Gets the DealerTenant repository.</summary>
    IDealerTenantRepository DealerTenants { get; }

    /// <summary>Gets the DealerTenantType repository.</summary>
    IDealerTenantTypeRepository DealerTenantTypes { get; }

    /// <summary>Gets the Role repository.</summary>
    IRoleRepository Roles { get; }

    /// <summary>Gets the User repository.</summary>
    IUserRepository Users { get; }

    #endregion

    #region ACE Repositories

    /// <summary>Gets the ACECustomer repository.</summary>
    IACECustomerRepository ACECustomers { get; }

    // ... one property per table, per schema ...

    #endregion
}
```

---

## 8. Unit of Work Implementation (excerpt)

**File:** `Solasta.DataAccess\UnitOfWork.generated.cs`

Key observations:
- `partial class` implementing `IUnitOfWork`.
- Uses **lazy initialization** via null-coalescing assignment (`_repo ??= new Repo(_context)`) — repositories are only instantiated on first access.
- Private nullable backing fields for each repository are declared in `// AUTO GEN-START: {Schema} Repositories ... // AUTO GEN-END:` sentinel blocks — these markers allow re-generation to be surgically precise.
- Public properties implement `IUnitOfWork` using `/// <inheritdoc />`.
- The non-generated partial handles the constructor (which receives `SolastaDbContext`), `SaveChangesAsync`, and transaction management.

```csharp
namespace Solasta.DataAccess;

using Microsoft.EntityFrameworkCore.Storage;
using Solasta.Data.Entity;
using Solasta.Data.Entity.Repositories;
using Solasta.Data.Repository.ACC;
using Solasta.Data.Repository.ACE;
using Solasta.Data.Repository.MAC;
using Solasta.Data.Repository.QAL;
using Solasta.Data.Repository.RSU;

/// <summary>
/// Unit of Work implementation for managing database transactions.
/// </summary>
public partial class UnitOfWork : IUnitOfWork
{
    // AUTO GEN-START: ACC Repositories
    #region ACC Repositories

    private IDealerRepository? _dealers;
    private IDealerTenantRepository? _dealerTenants;
    private IDealerTenantTypeRepository? _dealerTenantTypes;
    private IRoleRepository? _roles;
    private IUserRepository? _users;

    #endregion ACC Repositories
    // AUTO GEN-END: ACC Repositories

    // AUTO GEN-START: MAC Repositories
    #region MAC Repositories

    private IEstimateRepository? _estimates;
    private IMACCustomerRepository? _mACCustomers;
    // ... one field per table ...

    #endregion MAC Repositories
    // AUTO GEN-END: MAC Repositories

    // --- Property implementations ---

    #region ACC Repositories

    /// <inheritdoc />
    public IDealerRepository Dealers => _dealers ??= new DealerRepository(_context);

    /// <inheritdoc />
    public IDealerTenantRepository DealerTenants => _dealerTenants ??= new DealerTenantRepository(_context);

    /// <inheritdoc />
    public IUserRepository Users => _users ??= new UserRepository(_context);

    #endregion

    // ... MAC, ACE, QAL, RSU property regions follow identically ...
}
```

---

## 9. Proxy Lookup

**File:** `Solasta.Data\Utilities\ACC\DealerTenantTypeProxyLookup.generated.cs`

Key observations:
- Namespace: `{RootNamespace}.Utilities.{Schema}`
- Generated for tables that are designated as **"proxy type" / lookup tables** in the profile (`criticalDataTypeTables`).
- Emits three artifacts in one file:
  1. `Dictionary<string, int> IdPrefixMappings` — maps the string proxy key (e.g. `"MSTR"`) to the DB `int` ID.
  2. `DealerTenantTypeProxyEnum` — a C# enum with the same values (allows switch/pattern matching without magic strings).
  3. Two static lookup methods: `GetDealerTenantTypeProxyPrefix(int)` and `GetDealerTenantTypeProxyId(string | enum)`.
- Values (enum members, dictionary entries) come from the actual data rows in the database at generation time.
- Intended use: eliminate magic strings/ints when checking type IDs throughout the application.

```csharp
namespace Solasta.Data.Utilities.ACC;

using Solasta.Data;
using System.Collections.Generic;

/// <summary>
/// Proxy-typed definitions for the DealerTenantTypes table.
/// </summary>
public class DealerTenantTypeProxyTypeLookup
{
    #region Lookup Definitions

    public static Dictionary<string, int> IdPrefixMappings { get; set; } = new Dictionary<string, int>
    {
        { "MSTR", 1 }, // This is the master tenant
        { "QUAD", 2 }, // This is the development tenant
    };

    public enum DealerTenantTypeProxyEnum
    {
        MSTR = 1,
        QUAD = 2,
    }

    #endregion Lookup Definitions

    #region Methods

    public static string GetDealerTenantTypeProxyPrefix(int typeId)
    {
        foreach (var kvp in IdPrefixMappings)
        {
            if (kvp.Value == typeId)
                return kvp.Key;
        }
        throw new KeyNotFoundException($"Type ID '{typeId}' not found in GetDealerTenantTypeProxyPrefix.");
    }

    public static int GetDealerTenantTypeProxyId(string prefix)
    {
        if (IdPrefixMappings.TryGetValue(prefix, out var typeId))
            return typeId;
        throw new KeyNotFoundException($"Prefix '{prefix}' not found in GetDealerTenantTypeProxyId.");
    }

    public static int GetDealerTenantTypeProxyId(DealerTenantTypeProxyEnum proxyType)
    {
        return (int)proxyType;
    }

    #endregion Methods
}
```

---

## Cross-Cutting Conventions

| Convention | Rule |
|---|---|
| File suffix | All generated files end in `.generated.cs` — never edit by hand |
| `partial` everywhere | Every generated class/interface is `partial` — extend in a sibling non-generated file |
| `AUTO GEN-START / AUTO GEN-END` | Sentinel comments allow targeted re-generation without overwriting custom code |
| Namespace pattern | `{RootNS}.{Layer}.{Schema}` — layer is `Models`, `Repository`, `DataAccess`, or `Utilities` |
| Pluralization | DbSet names, UoW properties, and repository class names use the **plural** DB table name |
| Schema segmentation | All types are in per-schema sub-namespaces and `#region` blocks |
| Header block | Every file begins with `#region Generated Header` containing copyright, timestamp, and `<autogenerated />` |
