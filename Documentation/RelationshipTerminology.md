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

## Implementation and verification sequence

1. Add explicit role aliases and the validated factory. Deprecate only the
   shorter constructor; retain existing signatures and permissive behavior.
2. Migrate schema readers, conversions, all writers, dependency ordering,
   and fixtures to explicit roles. Keep valid generated SQL unchanged.
3. Verify factory validation, two-way alias synchronization, both legacy
   constructors, unnamed metadata, reader mappings, writer equivalence,
   self references, dependency order, and actual constraint enforcement.
4. Build every package target framework and run the full application and
   library suite before choosing the release version and publishing.
5. After this terminology change, review duplicate / overlapping
   functionality, including the responsibilities of `Relationship` and
   `ForeignKey`. Keep that consolidation separate from this migration.
6. Cut the v2 branch from the accepted compatible-change commit. Use the
   decisions below as preparation; finalize shared model ownership after
   the overlap review. No package version or branch is changed by this doc.

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
package. Consolidating overlapping models is the next work item after the
compatible terminology change, before implementing the clean v2 interface.
