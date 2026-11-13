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
Use `ForeignKey` for new code. The `Relationship` class is obsolete; its
existing members remain available for compatibility.

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

## Reading foreign keys

On an existing `DataStoreStructure` or `OleDbSchema` instance, query the
referencing table that owns the constraints:

```csharp
Collection<ForeignKey> keys = schema.GetForeignKeysOwnedByTable("Orders");
```

For `Orders.productId -> Products.id`, this returns the key owned by
`Orders`. Querying `Products` does not return that incoming key. Self
references are included for their owning table. Each call returns a new
collection, empty when the table owns no keys; null or blank names throw
argument exceptions. SQLite and Access use the same ownership direction.

`GetSchema()` populates each `Table.ForeignKeys` directly. The legacy
`GetRelationships` method and Relationship conversion/attachment helpers
are obsolete and retained for compatibility. Their provider-specific query
behavior is unchanged: Access's legacy query returns incoming keys.

See [migration and v2 preparation](https://github.com/jamesjohnmcguire/DigitalZenWorks.Database.ToolKit/blob/main/Documentation/RelationshipTerminology.md)
for legacy API mappings, subclass migration, alias meanings, and the
composite-key design notes.

## SQL text and parameter values

SQL text passed to `DataStorage`, `IDataStorage`, or `DatabaseConnection`
must come from trusted application code. These APIs execute the supplied
SQL; they do not validate or sanitize its contents. Pass external data as
parameters instead of concatenating or interpolating it into SQL. Supplying
parameters does not protect values already embedded in the command text.

For SQLite, SQL Server, or MySQL, use a parameterized `DataStorage` overload
with names matching the provider's placeholders:

```csharp
Dictionary<string, object> values = new()
{
    { "@name", suppliedName },
};
DataRow row = database.GetDataRow(
    "SELECT id FROM Customers WHERE name = @name", values);
```

Values retain their CLR types for provider inference; null becomes SQL NULL
through `DBNull.Value`. Parameterized overloads are available on
`ExecuteNonQuery`, `GetDataRow`, `GetDataTable`, `GetDataSet`, `Insert`, and
`Update`. Methods such as `ExecuteReader(string)` and `GetDataField(string)`
have no parameter overload; use a suitable parameterized operation when
external values are involved. Dapper-backed methods taking a parameter
object bind values separately; `DatabaseConnection.Query<T>(string)` does
not expose such a parameter argument.

Access/OLE DB uses `?` placeholders and binds values by position. Supply a
dictionary with a guaranteed iteration order matching the placeholders;
parameter names do not determine binding order. Do not sort parameters by
name or rely on the iteration order of an arbitrary `IDictionary`.

Table/column identifiers and WHERE/ORDER BY/LIMIT fragments are SQL syntax,
not parameter values. Keep fragments under application control; map any
external identifier choice to an allowlist and quote it for the provider.
`DataObjectsBase.GetBy` accepts raw fragments. Its legacy `GetByName` and
`GetIdByName` helpers embed names directly into SQL and do not bind them;
use a parameterized query for external names or names containing quotes.

CA2100 remains enabled at the raw command assignment pending further
review. The binding tests verify literal-value handling; they cannot
establish the safety of SQL constructed by every library consumer.

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
### Unreleased (1.4.0)

- Add `DatabaseFileInspector.GetDatabaseFileType` as the public file inspection
  entry point, retaining obsolete `DataDefinition` methods for compatibility.
- Delegate Access file identification to the inspector and add regression
  coverage for detection, validation, compatibility wrappers, and visibility.
- Add explicit relationship roles and a validated `ForeignKey.Create` factory.
- Deprecate the shorter `ForeignKey` constructor with a warning; removal is
  planned for v2. Consumers treating obsolete warnings as errors must migrate.
- Preserve legacy constructor and property behavior and generated SQL.
- Deprecate `Relationship` and its query/conversion/attachment APIs while
  retaining their published signatures and legacy behavior.
- Add `GetForeignKeysOwnedByTable` with consistent SQLite/Access ownership
  semantics and fresh `Collection<ForeignKey>` results.
- Read schema foreign keys directly into `Table.ForeignKeys`. Schema loading
  no longer invokes `GetRelationship` or `GetTableWithRelationships`
  overrides; subclasses should migrate to the ForeignKey reader hooks.
- Share dependency/table ordering through internal `TableSet`, preserving
  public forwarding APIs and existing ordering behavior.
- Centralize null checks in internal `ArgumentGuard`; replace redundant
  custom null messages with framework defaults, retaining exception types
  and parameter names.
- Fix command parameter construction for SQLite, SQL Server, and MySQL,
  preserving names and CLR values and mapping null to `DBNull.Value`.
- Add parameter-binding regressions, including SQLite literal-value queries
  and Access positional execution, and document the raw-SQL trust boundary.
  CA2100 remains unsuppressed.
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
