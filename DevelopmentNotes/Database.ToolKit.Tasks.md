## Immediate — before 1.4.0

1. Version bump deferred at user request. When requested, update
   `DatabaseLibraryNet/DigitalZenWorks.Database.ToolKit.csproj`:
   set `Version` to `1.4.0` and `FileVersion` to `1.4.0.0`. Keep
   `AssemblyVersion` at `1.0.0.0` and the package compatibility baseline
   at published `1.3.62` through this release.
2. Run final SQLite and Access tests, export/schema regressions, and
   Release package/API compatibility checks across all eight frameworks.
3. Optional, if it remains a small change: add PackageClients as a root
   DevelopmentTools submodule at `SourceCode/PackageClients`. Review its
   nested dependencies and commit the intended PackageClients changes
   first. Keep solution integration and a Database.ToolKit package consumer
   separate. This is not a 1.4.0 release requirement; defer until after the
   release if preparation or integration would distract from release work.
4. Commit the reviewed release changes and create an annotated Git tag
   `1.4.0` on the validated release commit, following existing tag naming.
5. Push the release tag and publish the reviewed 1.4.0 NuGet package and
   release artifacts.

## Completed for 1.4.0

- README and API XML comments document trusted SQL text versus bound data,
  named/positional parameters, raw-fragment APIs, and unparameterized legacy
  name helpers. CA2100 remains enabled and unsuppressed.

- Parameter binding uses provider-created parameters, preserving names and
  CLR values and mapping null to `DBNull.Value`. Eight regression cases
  cover SQLite execution, SQL Server/MySQL command binding without servers,
  and Access positional execution. OLE DB iteration order is documented;
  CA2100 remains unsuppressed pending the broader trust-boundary review.

- README usage guidance, the existing terminology/migration guide, and
  the 1.4.0 changelog now cover ownership queries, legacy deprecations,
  schema extension-point migration, and the internal refactors.

- Internal `TableSet` dependency and table ordering extraction is complete,
  with characterization tests and published forwarding APIs preserved.
- Internal `ArgumentGuard`, focused tests, and caller migration are
  complete. Null-check exception types, parameter names, and intentional
  null acceptance are preserved; redundant custom null messages removed.
- Initial review of `overlap-refactor`: current concerns are addressed.
- Compatible `Relationship` retirement: the ForeignKey path, legacy
  deprecations, regression coverage, and expected-warning cleanup are
  complete. Published legacy APIs remain available for migration;
  removal belongs to a major version.

## After 1.4.0 — review findings

- Review raw-SQL APIs and internal callers across `DataStorage`,
  `DataObjectsBase`, and `DatabaseConnection`. Inventory value interpolation,
  caller-supplied WHERE/ORDER BY/LIMIT fragments, and dynamic identifiers.
  In particular, `GetByName` and `GetIdByName` embed the name directly into
  an unescaped SQL literal; add compatible parameterized replacements.
  Distinguish typed numeric values from arbitrary strings when assessing
  actual exposure; avoid generic SQL sanitizers or keyword filters.
- Add missing parameterized query/reader/scalar convenience overloads in
  small compatible changes. Review interface evolution separately so new
  members do not break existing `IDataStorage` implementations.
- Design provider-aware identifier quoting and allowlisted selections for
  higher-level helpers. Parameters bind data values, not identifiers or SQL
  fragments. Consider explicit trusted-SQL naming/contracts in a future
  API revision while retaining compatible raw-SQL entry points.
- Review an ordered parameter API for Access/OLE DB: `?` placeholders bind
  by position, while arbitrary `IDictionary` implementations do not promise
  a portable iteration order. Plan migration rather than silently sorting.
- Revisit CA2100 after the binding fixes, internal-call review, and trust
  contract are verified. Consider a local suppression only at the reviewed
  raw-command assignment, keeping security analysis enabled elsewhere.
  Review SQL logging for sensitive literals as part of the broader audit.

- Review `GetColumnType` responsibilities before consolidating further.
  `DataDefinition` parses declaration text using substring matching and
  returns `Other` for unknown types. `DataStoreStructure` interprets
  normalized provider metadata, groups some types, and returns `Unknown`.
  Compare null/unknown handling, integer/text mappings, precision, flags,
  and length; retain deliberate provider differences. Consider a focused
  SQL-definition parser rather than adding type parsing to `TableSet`.
- Review SQL statement parsing separately from file loading.
  `GetQueriesFromSqlFile` owns file I/O and its error policy;
  `GetSqlQueryStatements` splits SQL text. Check quoted semicolons and
  comments before replacing the current simple splitter.
- Review generic table-collection conversion separately from provider
  filtering. `GetTables` currently also removes `sqlite_sequence`;
  provider-specific system-table rules should stay with schema readers.
- Clarify the distinct responsibilities behind similarly named methods:
  `GetSchema` coordinates file-based loading in `DataDefinition` and reads
  provider metadata in `DataStoreStructure`; `GetTableName` parses SQL
  text in one class and reads a metadata row in the other. These are not
  duplicate implementations merely because their names match.
- Review dependency policy separately from extraction: cycles are
  tolerated, absent dependency names appear in name-ordering results but
  are omitted from ordered table objects, and duplicate table names fail.
  Any stricter validation or new sorting policy is a behavior change.

## Later / backlog

After 1.4.0: consider a further review of `overlap-refactor` if warranted.

Version 2	Resolve	DataDefinition / DataStoreStructure public API redesign; internal TableSet ordering extraction is complete for 1.4.0.
Version 2	Resolve	DataStorage / DataObjectsBase / DatabaseConnection

Create Bash Build Script
ForeignKey	Prepare v2 terminology cleanup	Compatible explicit roles and factory are implemented. Plan and composite-key design: SourceCode/Database.ToolKit/Documentation/RelationshipTerminology.md. Remove legacy aliases in v2 without reversing their current meanings.
ForeignKey	Strengthen ForeignKeys	Centralize null / blank member validation and prevent invalid mutation.   Define the policy for unnamed constraints and null collection entries.
ForeignKey	Remove the obsolete shorter constructor in v2	The current version retains its original parameter order, names, and assignments and warns callers to use ForeignKey.Create.
ForeignKey	Add model tests for missing / blank identifiers and mutation, legacy constructor migration, and descriptive validation errors before writing SQL or ordering dependencies.
ForeignKey	Extend writer / schema-reader integration coverage for self references, composite keys, quoted identifiers, and Access-to-SQLite relationships. Keep type-mapping tests independent of unrelated relationship fixtures.
Architecture	Review duplicate or overlapping functionality after the terminology work	Evaluate ForeignKey and Relationship responsibilities before finalizing the v2 interface; keep consolidation separate from the compatible terminology migration.
MsAccessToolKit	/// Get constaints test.
Database.ToolKit	Add a future task to re-visit adding DatabaseType to SqlWriter.
Database.ToolKit	Note: OleDbTests.ExportSchema (CRLF vs LF) — not caused by anything you or I touched. This is a genuinely pre-existing bug that's simply invisible in your Linux verifier run, because that whole test class is [SupportedOSPlatform("windows")]-skipped there. It only surfaced now because you ran locally on Windows, where it executes for real. Irrelevant to this task's scope and to the grading pipeline — but worth a mental note for later, separate from this bounty.
Database.ToolKit	ColumnType.NText isn't explicitly listed but harmless.
Database.ToolKit	fix tests
Database.ToolKit	Remove Table.PrimaryKey	looks like a vestige of the original single-PK-per-table design you mentioned
Database.ToolKit	Generate Access.sql Orders
DB	/// Execture a data reader.
DB	/// depdenencies.</param>
In the 'GetSchema' method, ensure that the 'oleDbSchema.GetPrimaryKeys' does not return an empty DataTable to avoid 'IndexOutOfRangeException' when accessing 'Rows[0]'. Consider adding a check for the row count.		DatabaseLibraryNet/DataDefinition.cs:330
The list concatenation syntax '[.. relationships, .. newRelationships]' is unconventional in C#. Consider using List.AddRange(newRelationships) to merge the lists for better clarity and compatibility.	DatabaseLibraryNet/DataDefinition.cs:804
Accessing primaryKeys.Rows[0] without checking if any primary key exists may lead to an exception when a table does not have a defined primary key. Consider adding a check to ensure that primaryKeys contains at least one row before accessing it.
Database.ToolKit	Replace Windows Specific (OleDb) Code with Cross Platform Support Code focusing on SqLite functionality
Database.ToolKit	GetSchema Assumes Only One Primary Key, Adjust for Potential Composite Primary Keys
GetBy	need to return multiple
Need to omit or set to NULL when foreign key fields when value is 0
database errors	throw?
CreateDatabaseFileIfNotExists
Have OleDbSchema Use New BuildConnectionString Method
Deprecate DataDefinition.GetOrderedDependencies
Create DatabaseLibraryNetWin Project
Move OleDbSchema to DatabaseLibraryNetWin
Move references to accdb, mdb	Move / Copy template.accdb to MsAccessTool and DatabaseUtilitiesWin
Deal with ImportSchemaMdb
Rename OleDbSchema to DataStoreStructureOleDb?
SqLite	PRAGMA strict_mode = ON;
Order Properties	ForeignKey?
Deprec 		public ForeignKey(	string name, string columnName, string parentTable, string parentTableColumn, ConstraintAction onDeleteAction, ConstraintAction onUpdateAction)
ExportSchema	Using CompareText from Read File Is Problematic for the Following Reasons: 1. The variations of use spaces doesn't make the schema invalid - The compare text has different use of spaces then the exported text.  2. The ordering of constraints may and, thus far, will be different.  May need to use Regex's
Simplify Process of Relationship to ForeignKey
GetConstraintsByProvider	Leave as-is, until Enhanced Providers Database Testing
Implement #Nullable Project Wise
Handling large query return sets - need to set up looping over return set
