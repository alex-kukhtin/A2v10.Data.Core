# Multi-database support — design note

Status: **design only, nothing implemented.** Written 2026-08-30 to avoid re-deriving the
same path. Target: Postgres first, but the approach is meant to work on *any* database,
MySQL included.

## The idea

A "procedure" is **plain SQL text, written by hand for the target database**, stored in the
database as text. At execution time the text is read, split on dumb comment markers, and
**each marked block is issued as a separate database call returning exactly one resultset**:

```sql
-- BEGIN RECORDSET
select * from table1;
-- END RECORDSET

-- BEGIN RECORDSET
select * from table2;
-- END RECORDSET
```

"One call, one query, one resultset with parameters" is the lowest common denominator that
every database supports. Building the data model out of N such calls removes the entire
class of vendor-specific machinery the current implementation depends on.

The markers are comments, so the stored text stays valid SQL. There is no dialect, no
parser, no grammar — only a scan for line-anchored markers. This is deliberate: a language
would grow an `IF`, then a loop, then exception handling, and end up a worse plpgsql.

## What this removes

* `NextResult()` over multiple resultsets from one stored procedure — no database does this
  portably (Postgres would need refcursors, MySQL nothing at all).
* Table-valued parameters and `SqlDbType.Structured`.
* `SqlCommandBuilder.DeriveParameters` and `MetadataCache` built around it.
* The `.Metadata` companion procedure (`Update2Metadata`), together with its extra roundtrip
  on every save, and `WriterMetadata` / `DataTablePattern` as products of a database call.

## What stays untouched

`DataModelReader` and the whole column-name convention layer (`Name!TAgent!Object`, `!!Id`,
`!$RowCount`, `$System`, `$Aliases`, `$Grouping`, `$Defaults`, cross-mapping, id-mapping,
grouping). The reader depends on `IDataReader` only — the `NextResult()` loop lives in
`SqlDbContext.ReadDataAsync`, outside it. Feeding it one resultset per call changes nothing
inside.

This matters beyond convenience: the conventions are the product. Two convention dialects
would mean applications stop being portable between backends, and the layer above (XAML
views, client-side `$Metadata`) would have to know which database it is running on.

## Rules

* **Inside markers — only statements that return rows. Outside — only statements that
  return none** (DDL, `SET`, `CREATE TEMP TABLE`, `INSERT`/`UPDATE` without `RETURNING`).
  Blocks outside markers are plumbing. This keeps the block-to-resultset mapping explicit
  rather than positional: inserting a plumbing statement can never silently shift the model.
* **All blocks run in one transaction on one connection.** N separate calls would otherwise
  be N separate snapshots, and a torn read can leave `CrossMapper` / `IdMapper` with a
  reference resolving to nothing. The connection must not return to the pool between blocks,
  or a temp table created in a plumbing block will not survive to the next one.
* **Never reformat, trim, or re-assemble the text.** Send blocks verbatim and keep each
  block's starting line offset, so database error positions map back to the stored source.

## Bulk write

The one thing that stays provider-specific is delivering the flattened row set — it is the
only non-scalar parameter, whatever the text says.

1. A plumbing block creates a temp table by hand, using the target database's own syntax.
2. C# fills it with the provider's bulk API — `SqlBulkCopy`, binary `COPY`, `LOAD DATA` /
   multi-row `INSERT`. One method per provider; everything else is shared.
3. The marked table block carries only *document path → target table name*.
4. Column types are read back from the created table once and cached.

Point 4 is the reason to prefer this over declaring types in the marker: today types live in
two places (the table type and `.Metadata`) and can drift apart. Here they live in one.

The author of the text never writes a table parameter at all — they read from the temp table
with ordinary SQL.

The flattening itself (`DataModelWriter.GetDataForSave`: recursive walk propagating
`ParentId` / `ParentKey` / `RowNumber` / `ParentGuid` / `ParentRowNumber`) is kept, and so
are its two semantic rules that are easy to mistake for plumbing: `Id = 0` → `NULL` for new
records (`CheckId`), and empty string → `NULL` under `AllowEmptyStrings`.

## Alternatives considered and rejected

**JSON / JSONB in and out.** JSON is badly slow in every database. It holds even though the
cost would vanish against a roundtrip for a small save — the write mechanism has to be
chosen for the 5000-row case, not the 10-row one.

**Postgres `refcursor[]`.** Preserves `NextResult()` exactly, but: a transaction is required
for every read, 3–4 roundtrips, cursor names must be generated to avoid collisions within a
transaction, 5–6 lines of ceremony in every procedure, and cursors are planned under
`cursor_tuple_fraction` (0.1 by default) while we always `FETCH ALL` — a systematic risk of
the wrong plan. Postgres-only in any case.

**`unnest` with typed arrays.** Fast and binary, but Postgres-only, and its one real
advantage — everything travelling as parameters in a single batch — disappeared once each
block became a separate call.

**Composite type arrays.** Npgsql writes composites through static CLR type mapping; the
data here is `ExpandoObject`-driven and dynamic.

**Several `SELECT`s in one command text, generated in C#.** One roundtrip, no cursors, but it
moves the SQL out of the database into `model.json` — a platform-level change, not a data
layer one. The stored-text approach gets the same result while the SQL stays in the database.

## Packaging

One assembly, two (or more) contexts, DI selects by configuration. The model layer stays
`internal`: extracting it into a shared package would mean making ~10 types public
(`DataModelReader`, `WriterMetadata`, `IdMapper`, `CrossMapper`, `DynamicDataGrouping`,
`FieldInfo`, …) and owning their API compatibility forever, for no consumer benefit.

The usual objection — do not force Npgsql on SQL Server users — does not apply here:
`A2v10.Data.Core.csproj` already carries `Azure.Identity`, `Microsoft.Identity.Client` and
two `Microsoft.IdentityModel.*` packages as direct references, none of which appear in any
source file.

## Open questions

* `IParameterBuilder` leaks `SqlDbType` and `DataTable` into `A2v10.Data.Interfaces`, and
  `DbParamsExtension` hard-codes `SqlParameter` behind `IDbContext.ParameterBuilder`. A
  breaking change to settle **before** starting, not halfway through.
* Where the pseudo-procedure text lives, and how its cache invalidates across instances.
  Precedent: `MetadataCache` lives for the process and is never invalidated.
  Write access to that table equals the right to execute arbitrary SQL.
* `SaveModelBatchAsync` / `BatchCommandBuilder` — not discussed. It emits `exec proc @p=@v`;
  presumably it becomes just more blocks, but that was never worked through.
* `ITableDescription` and the `onSetData` callback of `SaveModelAsync` have no source for the
  table shape once `.Metadata` is gone.
* `SetTenantId` becomes a plumbing block, which also resolves the stateless-connection
  question — but the batch path (see above) was never covered.
* `System.Data.Common.DbBatch` (.NET 6+) can collapse the N calls back into one roundtrip
  where the provider implements it (Npgsql, MySqlConnector; `Microsoft.Data.SqlClient`
  apparently not yet — verify). Same code shape, optional optimization, not needed for
  correctness.
