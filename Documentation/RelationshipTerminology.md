# Relationship terminology and v2 preparation

## Current major version: compatible migration

The referenced table is the parent target. The referencing table is the
child containing the foreign key. For `Orders.productId -> Products.id`:

| Explicit property | Value | Historical property |
| --- | --- | --- |
| `ReferencedTable` | `Products` | `ChildTable` |
| `ReferencedColumn` | `id` | `ChildColumn` / `Relationship.ChildTableCol` |
| `ReferencingTable` | `Orders` | `ParentTable` |
| `ReferencingColumn` | `productId` | `ParentColumn` / `Relationship.ParentTableCol` |

The new properties are aliases over existing storage. Writes through either
name are visible through both APIs. `ForeignKey.ColumnName` remains an alias
for `ChildColumn`; `ParentTableColumn` remains an alias for `ParentColumn`.
New public properties may appear in reflection-based serialization; callers
with fixed serialized contracts should select property names explicitly.

`ForeignKey.Create` takes the constraint name, referenced parent table and
column, referencing child table and column, then delete and update actions.
Use named arguments as shown in the README. It rejects null, empty, or
whitespace identifiers with an argument exception identifying the parameter.
Names containing spaces are allowed; values are not trimmed or normalized.
The factory currently requires a constraint name and one column pair.

Existing constructor parameter order and names are unchanged, including for
named-argument consumers. The complete constructor still takes the legacy
`parentTable` / `parentColumn` pair first, which represents the referencing
child in the reader/writer contract. Existing setters remain permissive.
Schema readers retain permissive construction so provider metadata such as
an unnamed constraint does not gain a new validation failure.

The shorter constructor is an exception to the historical reader/writer
mapping: it puts the local column in `ChildColumn`, the referenced target
in `ParentTable` / `ParentColumn`, and leaves `ChildTable` unset. Its behavior
is preserved, not inferred or repaired by aliases. It is obsolete with a
warning and scheduled for removal in v2. Migrate by supplying all roles to
the factory; do not mechanically copy its values using the table above.

## Migrating for 1.4.0

`Relationship` is obsolete. Use `ForeignKey` for new definitions and schema
metadata. Published legacy APIs remain callable with obsolete warnings;
projects treating those warnings as errors must migrate or deliberately
suppress them around compatibility code.

| Legacy API | Migration |
| --- | --- |
| `Relationship` | `ForeignKey`; use `ForeignKey.Create` for new definitions. |
| `DataDefinition.GetForeignKeyRelationships` / `DataStoreStructure.GetForeignKeyRelationships` | Create `ForeignKey` objects directly. |
| `DataStoreStructure.GetRelationships` | Use `GetForeignKeysOwnedByTable` when querying owned keys; see direction caveat below. |
| `DataStoreStructure.SetForeignKeys` / `SetTablesRelationships` | Populate the owning table's `ForeignKeys` collection directly. Preserve clear/replace or append behavior as needed. |
| `DataStoreStructure.GetForeignKeyRelationship` | Use `ForeignKey.Create` for validated application definitions. Provider readers should decode directly. |
| `GetRelationship(DataRow)` overrides | Decode rows in `GetForeignKey(DataRow)`. |
| `GetTableWithRelationships` overrides | Apply custom attachment behavior explicitly; `GetSchema()` adds decoded keys directly. |

### Query ownership and direction

For `Orders.productId -> Products.id`, `Orders` owns the constraint and
`Products` is its referenced target. `GetForeignKeysOwnedByTable("Orders")`
returns this key; querying `Products` does not return it. A self-referencing
key belongs to its own table. Results cover only the requested owner, with
no accumulator of keys from earlier table queries.

Each call returns a new `Collection<ForeignKey>`, empty when no keys are
owned by that table. Null names throw `ArgumentNullException`; empty or
whitespace names throw `ArgumentException`. The query does not mutate a
`Table` or an existing collection.

The legacy `GetForeignKeys(string)` raw-metadata method is still supported
and is not obsolete. Its historical direction remains provider-specific:
SQLite filters the owning table; Access filters the referenced target and
returns incoming keys. `GetRelationships` inherits that direction and still
appends to its supplied collection. Replacing an Access incoming-key query
with the new ownership query changes its meaning. To inspect incoming keys,
read the full schema and select keys whose `ReferencedTable` matches the
target; there is no new dedicated incoming-key query in 1.4.0.

### Subclass migration

`DataStoreStructure.GetSchema()` reads each table, queries its owned keys,
and appends them directly to that table's `ForeignKeys`. It no longer calls
`GetRelationships`, `GetRelationship`, `SetTablesRelationships`, or
`GetTableWithRelationships`. Existing overrides of the legacy virtual
methods still run when explicitly called through those legacy paths, but
no longer customize normal schema loading.

Use these protected virtual hooks for provider customization:

- `GetForeignKeyMetadataOwnedByTable(string referencingTableName)` selects
  raw metadata for the owning table. The base implementation delegates to
  `GetForeignKeys`; the Access override filters `FK_TABLE_NAME`, leaving
  the legacy `PK_TABLE_NAME` query unchanged.
- `GetForeignKey(DataRow row)` decodes one metadata row into a `ForeignKey`.
  Access supplies its own column mapping. Keep provider decoding permissive
  where metadata allows unnamed constraints; the application factory's
  validation is not a replacement for provider decoding.

There is no new attachment hook mirroring `GetTableWithRelationships`.
Subclasses needing custom attachment can override `GetSchema()` or adjust
its returned tables explicitly. Avoid populating the same keys twice.

### Implementation responsibilities

`DataDefinition` retains its file/schema utility APIs. `DataStoreStructure`
coordinates schema reading, and `OleDbSchema` handles Access metadata.
Internal `TableSet` shares dependency and table ordering behind the existing
public forwarding methods; it adds no new public API or ordering policy.
Internal `ArgumentGuard` centralizes framework-specific null checks.
Exception types and parameter names are preserved; redundant custom null
messages now use framework defaults. Methods intentionally accepting null
retain that behavior.

Type parsing, SQL statement parsing, and stricter dependency validation
remain separate post-release review tasks. This release does not change
cycle handling or add composite-key support.

## Composite foreign keys

A composite foreign key matches a tuple, not independent scalar values:

```sql
CREATE TABLE Customers (
    tenantId INTEGER NOT NULL,
    id INTEGER NOT NULL,
    PRIMARY KEY (tenantId, id)
);
CREATE TABLE Orders (
    tenantId INTEGER,
    customerId INTEGER,
    FOREIGN KEY (tenantId, customerId) REFERENCES Customers(tenantId, id)
);
```

Both values must match the same customer row, in the declared column order.
Two independent foreign keys do not express this requirement. With customer
keys `(1, 10)` and `(2, 20)`, `(1, 20)` is not a matching pair even though
each scalar occurs somewhere. Single columns need not be unique separately;
the parent tuple must be a primary or otherwise eligible unique key.
See [SQLite's composite foreign key documentation](https://www.sqlite.org/foreignkeys.html#composite_foreign_key_constraints).

The current scalar models cannot represent one constraint spanning several
column pairs. This release does not add composite-key support or change how
existing metadata is handled. Do not represent a composite constraint by
joining identifiers into a string or creating independent foreign keys.

## Proposed v2 interface decisions

- Remove obsolete `Relationship` and its legacy query/conversion/attachment
  APIs after callers migrate to `ForeignKey`.
- Remove the restored obsolete `Table.PrimaryKey` property and
  `DataObjectOleDb(DatabaseType, string)` constructor after callers migrate.
  Their current independent storage and base-constructor behavior remain
  unchanged until that breaking release.
- Retain only explicit role names; remove historical aliases and the
  obsolete constructor. Review whether the other constructor should be
  replaced by the factory. Never silently reverse legacy property meanings.
- Make identifiers immutable and validate complete definitions. Decide
  whether a constraint name is required or optional before finalizing v2.
- Represent a constraint with parent/child table roles, actions, and a
  nonempty ordered collection of column mappings. Each immutable mapping
  contains `ReferencedColumn` first and `ReferencingColumn` second. A scalar
  key uses one mapping; a composite key uses several. Exact type names and
  ownership depend on the next overlap review.
- Group provider metadata by constraint identity and retain provider column
  ordinals. Writers emit a single FOREIGN KEY clause for the entire mapping;
  table ordering depends on the referenced table once per constraint.
- Test swapped mapping order, repeated column mappings, tuple uniqueness,
  provider grouping, mixed pairs, null semantics, and cascades across all
  pairs. Define duplicate and null policies explicitly, with provider rules
  kept separate from structural validation.

These are v2 design notes, not additional APIs promised in the current
package. The 1.4.0 schema path uses `ForeignKey` directly; `Relationship` and
its legacy APIs remain available with obsolete warnings for compatibility.
Removing those APIs belongs to a major release.
