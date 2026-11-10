# DigitalZenWorks.Database.ToolKit
A .NET library with a set of database utility classes.

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

## Change Log
### Unreleased

- Add explicit relationship roles and a validated `ForeignKey.Create` factory.
- Deprecate the shorter `ForeignKey` constructor with a warning; removal is
  planned for v2. Consumers treating obsolete warnings as errors must migrate.
- Preserve legacy constructor and property behavior and generated SQL.

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
