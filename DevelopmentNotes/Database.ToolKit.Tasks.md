Review First Consolidate Attempt
Decide What Stays in ReadMe
Version 2	Resolve	DataDefinition / DataStoreStructure
Version 2	Resolve	DataStorage / DataObjectsBase / DatabaseConnection
Merge ForeignKey and Relationship
Refactor into TableSets

Work Off of Task List
Publish

Standardize Notes and Tasks Location Between Application and Library
Create Bash Build Script
ForeignKey	Prepare v2 terminology cleanup	Compatible explicit roles and factory are implemented. Plan and composite-key design: SourceCode/Database.ToolKit/Documentation/RelationshipTerminology.md. Remove legacy aliases in v2 without reversing their current meanings.
ForeignKey	Strengthen ForeignKeys	Centralize null / blank member validation and prevent invalid mutation.   Define the policy for unnamed constraints and null collection entries.
ForeignKey	Remove the obsolete shorter constructor in v2	The current version retains its original parameter order, names, and assignments and warns callers to use ForeignKey.Create.
ForeignKey	Add model tests for missing / blank identifiers and mutation, legacy constructor migration, and descriptive validation errors before writing SQL or ordering dependencies.
ForeignKey	Extend writer / schema-reader integration coverage for self references, composite keys, quoted identifiers, and Access-to-SQLite relationships. Keep type-mapping tests independent of unrelated relationship fixtures.
Architecture	Review duplicate or overlapping functionality after the terminology work	Evaluate ForeignKey and Relationship responsibilities before finalizing the v2 interface; keep consolidation separate from the compatible terminology migration.
DatabaseLibraryNet Resolve Different GetColumnType Methods
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
FK_Contacts
SqLite	PRAGMA strict_mode = ON;
Order Properties	ForeignKey?
Deprec 		public ForeignKey(	string name, string columnName, string parentTable, string parentTableColumn, ConstraintAction onDeleteAction, ConstraintAction onUpdateAction)
ExportSchema	Using CompareText from Read File Is Problematic for the Following Reasons: 1. The variations of use spaces doesn't make the schema invalid - The compare text has different use of spaces then the exported text.  2. The ordering of constraints may and, thus far, will be different.  May need to use Regex's
Simplify Process of Relationship to ForeignKey
GetConstraintsByProvider	Leave as-is, until Enhanced Providers Database Testing
# Nullable
Handling large query return sets - need to set up looping over return set
