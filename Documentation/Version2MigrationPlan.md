# Version 2 Migration Plan

## Goal and scope

Deliver a 2.0 release with a consistent schema model, correct composite constraints, explicit SQL formatting and parameter-binding contracts, and a documented migration path from 1.x. Removing deprecated APIs is one part of this work, not the release's sole purpose.

This is a proposed implementation sequence and release checklist. Existing behavior and detailed role mappings are documented in [RelationshipTerminology.md](RelationshipTerminology.md). Keep that document authoritative for historical meanings rather than copying its mappings here. Delete completed tasks; record significant shipped changes in the README change log.

## Starting point

The 1.x line already provides explicit referenced/referencing roles, owned-table foreign-key queries, typed text/date literal formatting, `SqlWriterMySql`, `ColumnPair`, virtual INSERT/UPDATE overloads, and MySQL identifier quoting. Build on these APIs rather than introducing competing helpers. Deprecated methods retain their historical behavior until the breaking release.

Prioritize compatible storage refactors, execution overloads, and test separation on the 1.x line. Use the [1.x task queue](../DevelopmentNotes/Database.ToolKit.Tasks.md#next-in-1x) for commit order and carry applicable changes into `development`. Use the resulting tests as the baseline for v2 ownership and interface changes.

## Work sequence

Each numbered item is a workstream, not a single large commit. Separate characterization tests, model changes, provider changes, consumer migration, and removals into reviewable commits. Stop after each agreed step for review and commit; keep submodule commits separate from parent pointer updates.

1. **Define the contract and baseline.** Inventory public and protected APIs, obsolete members, internal callers, and representative consumers. Record the provider capability matrix, supported target frameworks, intentional breaking changes, and existing warnings. Capture behavior before modifying it. Decide the supported framework set explicitly rather than carrying every existing target forward automatically.
2. **Finalize the schema model.** Use only explicit referenced/referencing roles. Represent each foreign key as one constraint with an ordered, nonempty collection of column mappings; define ordered primary and unique keys alongside it. Decide immutability, unnamed-constraint handling, duplicate mappings, null entries, and validation boundaries. Preserve raw names without trimming. Separate provider limitations from structurally invalid definitions. Define dependency behavior for cycles, missing tables, and duplicate names.
3. **Implement readers and writers.** Group provider metadata by constraint identity and column ordinal. Emit one foreign-key clause per constraint. Start with SQLite schema creation and inspection, then update Access/OLE DB on its Windows validation track. Verify self references, identifier quoting, type mapping, referential actions, and unsupported-feature reporting. Compare schema meaning rather than SQL whitespace or incidental constraint order.
4. **Consolidate SQL contracts.** Keep raw-value formatting separate from trimming, normalization, parsing, and missing-value policy. Reuse complete literals from `GetTextLiteral` and `GetDateLiteral` without adding quotes again. Keep identifier quoting separate from data values and keep `ColumnPair` values explicitly preformatted. Define numeric and control-character behavior; do not add another ambiguous sanitizing API. Document the MySQL SQL-mode requirement and test supported representations.
5. **Complete execution APIs.** Carry forward the 1.x parameterized operations and address remaining gaps. Use the 1.x lifetime tests to define who opens and disposes a connection, who ends a transaction, and how long a returned reader stays usable. Use explicit parameter order for OLE DB's positional placeholders; do not silently sort dictionaries. Keep trusted SQL fragments and identifiers distinct from bound data. Review interface changes, transaction/resource ownership, and error behavior before freezing the public API.
6. **Migrate and remove.** Migrate representative callers before deleting every obsolete API, including `Relationship`, its conversion/query/attachment paths, legacy SQL helpers, file-inspection forwarders, obsolete constructors, and `Table.PrimaryKey`. Review non-obsolete legacy role aliases and ambiguous signatures for explicit v2 replacement as well. Update subclass hooks and tests; remove obsolete-only compatibility tests after replacement behavior is covered. Document source, behavioral, and serialization changes with before/after examples.
7. **Validate and package.** Separate portable tests from Windows/ACE integration tests and run each in the appropriate environment. Ship the Windows-dependent Access implementation and template in `DigitalZenWorks.Database.ToolKit.OleDb`, which depends on the portable package. Keep Access SQL text generation and file-signature detection in the portable library. Fix reproducible Release restore/package issues, verify the selected target frameworks, build a release-candidate package, and compile migrated consumer examples against it.

## Acceptance criteria

- A synthetic SQLite schema containing scalar and composite keys, an eligible composite primary/unique target, self references, and quoted identifiers can be created, inspected, and recreated without losing modeled semantics.
- With SQLite foreign-key enforcement enabled, valid tuples succeed and mismatched tuples fail. Tests cover column order, null semantics, and supported cascade actions; independent scalar keys must not substitute for a composite constraint.
- Parameterized execution preserves CLR values and distinguishes missing values from empty text and the text `NULL`. Literal tests cover quotes, backslashes, whitespace, Unicode, dates, and explicitly supported control characters. SQL-looking input remains data.
- Verify the portable package dependency graph excludes `System.Data.OleDb`. Build and exercise MsAccessJetAceTool against the `.OleDb` package on Windows and document the assembly and package migration.
- Portable tests pass on Linux and Windows. Access integration passes separately on Windows with a compatible ACE provider. MySQL formatting tests do not imply live-server compatibility; live provider guarantees require corresponding integration tests.
- The capability matrix states what each provider reads, writes, executes, or does not support. Unsupported features are reported explicitly rather than silently weakened.
- Migrated consumer examples compile and pass regression tests. Removed APIs, extension-point changes, package changes, and framework changes have migration guidance.
- Release builds and package validation pass for the chosen frameworks, with no unexplained new warnings. Review intentional v1-to-v2 API breaks explicitly; do not disable validation globally to hide removals. Establish the 2.0 baseline after publication while maintaining the separate 1.x baseline.

## Branch and release strategy

| Branch | Purpose |
| --- | --- |
| `main` | Release-ready code. Remains on 1.x until the first approved 2.0 release, then follows 2.x. |
| `development` | Integration and consolidation for the next 2.x release. Breaking work lands here through focused branches. |
| `maintenance/1.x` | Release-ready 1.x fixes after the lines diverge. Retains 1.x public compatibility and package baseline. |
| `feature/<topic>` | Short-lived work based on `development`, merged back after review. |
| `fix/1.x-<topic>` | Short-lived maintenance fix based on `maintenance/1.x`, merged back after review. |

Before merging breaking v2 work into `main`, create `maintenance/1.x` from the latest released 1.x tag. If unpublished compatible changes are wanted there, review and bring them across deliberately. Check the existing `development` branch's ancestry and pending commits before making it the v2 integration line; do not reset or replace it blindly.

Keep 1.x maintenance limited to compatible bug fixes, appropriate dependency updates, and migration assistance. Port applicable fixes forward to `development` through a separate cherry-pick or adapted commit. Do not merge v2 changes back into maintenance or routinely merge whole branches with different API contracts. A forward-port review should also remove obsolete compatibility code that is unnecessary in v2.

When `development` is release-ready, merge it to `main`, validate that exact merge commit, create an annotated version tag using the existing numeric convention (for example, `2.0.0`), and publish that commit. Subsequent 1.x releases are tagged and published directly from `maintenance/1.x`; they do not merge back into a 2.x `main`. Tags identify the release line unambiguously.

CI currently builds pushes/PRs for `main` and permits publishing only from `main`. Before using this strategy, add validation for `development` and `maintenance/1.x`, permit deliberate maintenance publication, and check branch/package-version agreement. Pull-request validation should cover the intended merge target. Keep publication explicit; merging development work alone must not publish a package.

Set and announce a 1.x support policy before 2.0 ships. Maintain it through an agreed migration window; do not promise an indefinite period or an end date that has not been chosen. Add `development/1.x` only if maintenance work actually needs its own integration branch.

## Deferred beyond the initial 2.0 scope

A full ORM, general-purpose SQL parser, automatic arbitrary-engine migration, streaming/batching redesign, new providers, and universal provider feature parity are separate initiatives. Keep future package restructuring and additional live-server integration bounded by actual consumer needs.
