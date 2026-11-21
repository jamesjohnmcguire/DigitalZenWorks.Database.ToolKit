## Next in 1.x

1. Reuse `DataStorage.GetConnectionString` in the SQLite file constructor of `DataObjectsBase`. Preserve the existing path resolution, connection string, and accepted providers.
2. Share creation of closed provider connections between `DataStorage` and `DatabaseConnection`. Preserve each caller's opening behavior, unsupported-provider behavior, and the virtual `GetConnectionObject` hook used by OLE DB. Review each of these first two changes as its own commit.
3. Add characterization tests for starting a transaction on a fresh instance, rolling back a write, disposing a wrapper around caller-supplied storage, and consuming a returned reader. Record defects demonstrated by the tests as separate fixes.

## High-priority 1.x follow-up

- Bind the name parameter in `GetByName`. Cover an apostrophe and preserve virtual dispatch from `GetIdByName`.
- Add missing parameterized reader, scalar, and query overloads on concrete classes, one operation at a time. Check existing call resolution and result behavior.
- Run portable tests independently of Windows/ACE integration tests. Verify the portable suite on Linux and the Access suite on Windows before the 2.0 package split.

## Review findings

- Investigate Release packaging consuming Debug-only restore assets under SDK 11. Reproduce the cause of missing dependency groups and API comparison errors before changing build/workflow configuration. Workaround: restore with `-p:Configuration=Release --force`, then pack with `--no-restore`. Keep package validation enabled.
- Review raw-SQL APIs and internal callers across `DataStorage`, `DataObjectsBase`, and `DatabaseConnection`. Inventory value interpolation, caller-supplied WHERE/ORDER BY/LIMIT fragments, and dynamic identifiers.  Distinguish typed numeric values from arbitrary strings when assessing actual exposure; avoid generic SQL sanitizers or keyword filters.
- Design provider-aware identifier quoting and allowlisted selections for higher-level helpers. Parameters bind data values, not identifiers or SQL fragments. Consider explicit trusted-SQL naming/contracts in a future API revision while retaining compatible raw-SQL entry points.
- Review an ordered parameter API for Access/OLE DB: `?` placeholders bind by position, while arbitrary `IDictionary` implementations do not promise a portable iteration order. Plan migration rather than silently sorting.
- Revisit CA2100 after the internal-call review and trust contract are verified. Consider a local suppression only at the reviewed raw-command assignment, keeping security analysis enabled elsewhere. Review SQL logging for sensitive literals as part of the broader audit.
- Review `GetColumnType` responsibilities before consolidating further. `DataDefinition` parses declaration text using substring matching and returns `Other` for unknown types. `DataStoreStructure` interprets normalized provider metadata, groups some types, and returns `Unknown`. Compare null/unknown handling, integer/text mappings, precision, flags, and length; retain deliberate provider differences. Consider a focused SQL-definition parser rather than adding type parsing to `TableSet`.
- Review SQL statement parsing separately from file loading. `GetQueriesFromSqlFile` owns file I/O and its error policy; `GetSqlQueryStatements` splits SQL text. Check quoted semicolons and comments before replacing the current simple splitter.
- Review generic table-collection conversion separately from provider filtering. `GetTables` currently also removes `sqlite_sequence`; provider-specific system-table rules should stay with schema readers.
- Clarify the distinct responsibilities behind similarly named methods: `GetSchema` coordinates file-based loading in `DataDefinition` and reads provider metadata in `DataStoreStructure`; `GetTableName` parses SQL text in one class and reads a metadata row in the other. These are not duplicate implementations merely because their names match.
- Review dependency policy separately from extraction: cycles are tolerated, absent dependency names appear in name-ordering results but are omitted from ordered table objects, and duplicate table names fail. Any stricter validation or new sorting policy is a behavior change.

## Backlog

- Improve the README description and list the operations supported by each database engine.
- ForiegnKey	Change ParentTable to ParentTableName, etc
- ForiegnKey	Order Properties
- Revisit PackageClients submodule and consumer integration separately; inclusion is deferred.
- Add a Bash build script.
- For 2.0, use the 1.x lifetime tests to decide connection and transaction ownership, including disposal of caller-supplied storage. Assess changes to `IDataStorage` against existing implementers. Review remaining schema responsibilities in `DataDefinition` and `DataStoreStructure` separately.
- Follow the [Version 2 Migration Plan](../Documentation/Version2MigrationPlan.md) for release scope, acceptance criteria, migration sequencing, and 1.x maintenance branches. Keep historical role mappings and detailed composite-key notes in [RelationshipTerminology.md](../Documentation/RelationshipTerminology.md).
- Define ForeignKey mutation and validation policy, including unnamed constraints and null collection entries. Add tests for invalid identifiers, mutation, constructor migration, and errors before SQL generation.
- Extend writer/schema-reader coverage for self references, quoted identifiers, composite keys, and Access-to-SQLite relationships. Review composite primary-key handling alongside composite foreign keys.
- Review provider constraint testing, including the MsAccess constraint fixture and `GetConstraintsByProvider` behavior.
- Review exported-SQL assertions for meaningful schema equivalence rather than whitespace or constraint-order differences. Check Access export line endings and the Orders fixture in the generated Access SQL.
- Review `ColumnType.NText` mapping and whether `SqlWriter` needs a `DatabaseType` setting.
- For 2.0, move Windows-dependent Access operations and `Template.accdb` to the `.OleDb` project, then migrate MsAccessJetAceTool to the package and verify it on Windows with ACE.
- Review SQLite strict-table support and the naming of `OleDbSchema` separately.
- Review `ImportSchemaMdb` and whether to deprecate `DataDefinition.GetOrderedDependencies` in a future API revision.
- Define multi-row `GetBy` behavior and foreign-key handling when an input value is zero versus SQL NULL.
- Review database error propagation and a create-if-missing database API.
- Review whether ForeignKey property ordering needs an explicit contract.
- Enable nullable analysis consistently across projects and add streaming or batching support for large query results.
- Correct remaining XML-comment typos, including reader execution and dependency descriptions.
- MS Access to Sqlite	Should under go a 2 step process. 1. Normalize from Access 2. Switch to Sqlite
- Review and Evaluate - The Access writer currently emits IDENTITY for any primary-key column, so that test can pass even if detection stops working. That writer behavior is a separate issue.
- Make ForeignKey Members read only
