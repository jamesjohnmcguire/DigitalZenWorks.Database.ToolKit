# DigitalZenWorks.Database.ToolKit
A .NET library with a set of database utility classes.

## Database file identification

Use the public inspection method to identify an existing database file:

```csharp
DatabaseType type = DatabaseFileInspector.GetDatabaseFileType("data.accdb");
```

Recognized file headers take precedence over file extensions. If the header
is unrecognized, detection falls back to known SQLite, Access, and SQL Server
extensions. Missing or unidentified files return `DatabaseType.Unknown`.
Access files return `DatabaseType.OleDb`. Inspection reads the file header;
it does not open a database connection or validate the complete database.

`GetDatabaseFileType` is the inspector's only public operation. Header reading,
header interpretation, and extension detection are internal helpers. The four
existing file-inspection methods on `DataDefinition` remain public, obsolete
forwarders with their historical behavior. For general file type detection,
migrate to `DatabaseFileInspector.GetDatabaseFileType` with a file path;
this operation does not replace specialized byte-array or extension-only use.

## Foreign key terminology

Use `ReferencedTable` / `ReferencedColumn` for the parent target and
`ReferencingTable` / `ReferencingColumn` for the child that holds the key.
These names are available on both `ForeignKey` and `Relationship`.

For new application definitions, use the validated, parent-first factory:

```csharp
ForeignKey key = ForeignKey.Create(
    name: "FK_Orders_Products",
    referencedTable: "Products",
    referencedColumn: "id",
    referencingTable: "Orders",
    referencingColumn: "productId",
    onDeleteAction: ConstraintAction.SetNull,
    onUpdateAction: ConstraintAction.Cascade);
```

Both existing constructors retain their parameter names, order, and behavior.
The shorter constructor now produces an obsolete warning and is scheduled
for removal in v2. The complete constructor remains supported. Legacy
properties remain writable aliases with their historical meanings; they
have not been silently reversed. Validation is confined to the new factory.

See [migration and v2 preparation](https://github.com/jamesjohnmcguire/DigitalZenWorks.Database.ToolKit/blob/main/Documentation/RelationshipTerminology.md)
for the alias mapping, compatibility details, composite-key design, and the
planned follow-up review of duplicate or overlapping functionality.

## Release API compatibility

Release builds validate the package against published version `1.3.62` on
all shipped target frameworks, including public parameter names. From this
library repository root, run:

```powershell
dotnet build DatabaseLibraryNet/DigitalZenWorks.Database.ToolKit.csproj -c Release
```

Keep this baseline through the 1.4.0 release, then advance it to published
1.4.0 for subsequent releases. The checks allow additive APIs and do not
replace behavioral tests. Validation-only package downloads provide metadata
for transitive references; they do not add published package dependencies.

`Table.PrimaryKey` and `DataObjectOleDb(DatabaseType, string)` remain available
with obsolete warnings and are scheduled for removal in v2. The property
retains independent storage; use `Column.Primary` to define actual keys.
The old constructor preserves its base initialization and database-type
restrictions; new OLE DB callers should use `DataObjectOleDb(string)`.
The exact two-argument `ExportSchema` overload remains supported and forwards
to the default Access export. Consumers treating obsolete warnings as errors
will need to migrate or suppress the intentional warnings.

## Change Log
### Unreleased

- Add `DatabaseFileInspector.GetDatabaseFileType` as the public file inspection
  entry point, retaining obsolete `DataDefinition` methods for compatibility.
- Delegate Access file identification to the inspector and add regression
  coverage for detection, validation, compatibility wrappers, and visibility.
- Add explicit relationship roles and a validated `ForeignKey.Create` factory.
- Deprecate the shorter `ForeignKey` constructor with a warning; removal is
  planned for v2. Consumers treating obsolete warnings as errors must migrate.
- Preserve legacy constructor and property behavior and generated SQL.
- Restore the two-argument `ExportSchema` method for compiled consumers.
- Restore obsolete `Table.PrimaryKey` and the database-type/path
  `DataObjectOleDb` constructor until v2.
- Validate Release packages against published 1.3.62, including public
  parameter names, with regression tests for the restored APIs.

### 1.3.62

Includes updated packages that resolve vulnerabilities in transient packages
Added limited dapper support
Increased SqLite support
Enhanced nullable support
Better cross-platform support
Consolidated and moved OleDb functionality
Improvements based on code analysis warnings
Other various bug fixes
Removed 'All Rights Reserved.'
