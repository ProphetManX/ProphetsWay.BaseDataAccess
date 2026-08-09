# Purpose & Scope — ProphetsWay.BaseDataAccess

## Proposed One-Sentence Purpose

A storage-neutral contract vocabulary for a Data Access Layer — entity markers, capability-composed DAO
interfaces, and one aggregate DAL interface — plus an optional reflection dispatcher that lets business logic
call those contracts generically.

## Current Purpose (as implied by README/csproj)

The csproj `<Description>` says:

> A group of interfaces to define any/all interactions required for a Data Access Layer (DAL).

The README (rewritten this session) leads with decoupling and DAL swappability, which matches the code.

## The Drift

Three specific gaps between what the library claims and what it does:

1. **"any/all interactions" over-claims.** The library defines exactly four CRUD operations plus `GetAll`,
   `GetPaged`, and `GetCount`. There is no contract for querying by arbitrary predicate, bulk operations,
   projections, async, or cancellation. Consumers add those to their own entity DAOs. The description should
   describe a *small, deliberately minimal* vocabulary — that minimalism is a feature, not something to paper over.

2. **`IBaseIdEntity<T>` plays no runtime role.** It is declared, and it is never consumed anywhere inside the
   library. `Get<T>(object id)` resolves the identifier **by name** — `{TypeName}Id`, falling back to `Id` — and
   never consults the interface. An entity implementing `IBaseIdEntity<int>` and an entity implementing only
   `IBaseEntity` are treated identically. The test entities prove this: every one of them implements only
   `IBaseEntity`, including those exercising identifier resolution.

3. **`IBaseSoftEntity` declares a vocabulary the library never honors.** `Delete<T>` dispatches to `Delete(T)`
   regardless of whether `T` is soft-deletable. All soft-delete behavior lives in EFTools. This is defensible
   layering — contracts describe data shape, implementations own policy — but the name implies enforcement that
   does not exist here.

None of these is drift in *scope*. The library is doing the right job. They are drift in *stated* scope.

## Cohesion Map

Edges are counted **inside this library only**. Consumers (EFTools, Example) are noted separately because
inbound edges from another repo do not make a cluster extractable — they make it load-bearing.

| Cluster | Types | Depends on | Depended on by | Extraction candidate? |
| --- | --- | --- | --- | --- |
| Entity root | `IBaseEntity` | — | Every other cluster | No — it is the shared root |
| Typed identifier | `IBaseIdEntity<T>` | `IBaseEntity` | **Nothing inside the library**; EFTools constrains on it heavily | No — one 4-line interface |
| Soft-delete vocabulary | `IBaseSoftEntity`, `IBaseSoftIdEntity<T>` | `IBaseEntity`, `IBaseIdEntity<T>` | **Nothing inside the library**; EFTools `BaseSoftDao` family | No — no standalone utility |
| DAO capability contracts | `IBaseDao<T>`, `IBaseGetAllDao<T>`, `IBasePagedDao<T>` | `IBaseEntity` | **Nothing inside the library**; EFTools + consumer DAOs | No — meaningless outside the paradigm |
| Aggregate DAL contract | `IBaseDataAccess` | `IBaseEntity` | `BaseDataAccess` | No |
| Reflection dispatcher | `BaseDataAccess`, `BaseDataAccessHelper` (internal), `DataAccessConventionException` | `IBaseEntity`, `IBaseDataAccess` | Nothing | Theoretically — fails the bar, see below |

**The striking result:** almost every cluster has zero inbound edges *inside* the library. Normally that is the
signature of a package begging to be split. Here it is not, and the reason matters — this library is a
**vocabulary**, not a machine. A vocabulary's clusters are supposed to be independent of one another; they are
tied together by the consumer's usage, not by internal calls. Low internal coupling is the design working, not a
seam to cut along.

### The one candidate that could be argued: the reflection dispatcher

`BaseDataAccess` + `BaseDataAccessHelper` + `DataAccessConventionException` is the only cluster with real
substance — the helper carries the entire convention engine. It depends on the contracts one-way, so it *could*
compile as `ProphetsWay.BaseDataAccess.Conventions` referencing a contracts-only parent.

| Criterion | Verdict | Evidence |
| --- | --- | --- |
| Independently useful | **Fails** | It dispatches to `Get(T)`/`Insert(T)`/`Update(T)` — names defined only by this paradigm. Nobody installs it without the contracts. |
| Low coupling | Passes | One-way dependency on `IBaseEntity` and `IBaseDataAccess`. |
| Stable surface | Passes | Just hardened and pinned by 68 tests in v3.0.0. |
| Not already solved | Inconclusive | `UNVERIFIED — check nuget.org` for convention-dispatch libraries. |

Failing criterion 1 is decisive. Splitting would produce two packages that are **always installed together**,
double the version lines, changelogs, and pipelines, and hand consumers a version-matrix problem — in exchange
for letting a hypothetical user skip roughly 250 lines of a library that is 11 files total.

**No extraction candidate clears the bar, so `docs/nuget-extraction-proposal.md` was not written.**

### The opposite problem: is this repo too thin to stand alone?

Eleven source files is small enough to ask whether it should fold into EFTools. **It should not.** The entire
value proposition is that a consumer's contracts project references *only* the vocabulary, with no EF, no
provider, and no `DbContext` anywhere in its dependency graph. Merging it into EFTools would drag Entity
Framework into every contracts project and destroy the decoupling the library exists to provide. The thinness is
the point.

## In Scope

- Entity marker and shape contracts (`IBaseEntity`, `IBaseIdEntity<T>`, `IBaseSoftEntity`, `IBaseSoftIdEntity<T>`).
- Capability-composed DAO contracts (`IBaseDao<T>`, `IBaseGetAllDao<T>`, `IBasePagedDao<T>`).
- The aggregate DAL contract, `IBaseDataAccess`.
- The optional reflection dispatcher and its convention, including `DataAccessConventionException`.
- `BaseDataAccessHelper` staying `internal` — correctly not part of the public surface.

## Out of Scope (and where it should live instead)

| Thing | Where it belongs | Status |
| --- | --- | --- |
| Soft-delete *behavior* (filtering, delete-as-update) | The DAL implementation — EFTools `BaseSoftDao`, `RootBaseSoftDao` | Correct today |
| Provider types (`DbContext`, `SqlConnection`) | Implementation packages | Correctly absent |
| Entity-specific queries | The consumer's own `I{Entity}Dao` | Correct by design |
| Async / cancellation surface | Not present anywhere | Genuine gap — see refinement 4 |

## Recommended Refinements

Ordered by value. None requires a package split.

| # | Change | Rationale | Effort | Breaking? |
| --- | --- | --- | --- | --- |
| 1 | Split transaction members out of `IBaseDataAccess` into `IBaseTransactionalDataAccess` | See below — strongest finding | Medium | **Yes** — v4 |
| 2 | Document that `IBaseIdEntity<T>` has no runtime role in identifier resolution | Closes drift 2; prevents a false assumption that implementing it changes dispatch | Low | No |
| 3 | Rewrite the csproj `<Description>` to drop "any/all interactions" | Closes drift 1; the minimal vocabulary is the selling point | Low | No |
| 4 | Decide explicitly whether async belongs in the vocabulary | A modern EF/Cosmos DAL is async-first; a sync-only contract forces `.Result` or a parallel hand-rolled surface. Decide and record the decision — adding it is a large change | Low (decision) | No (decision only) |
| 5 | Add a test for an entity that implements `IBaseIdEntity<T>` *explicitly* | Explicit implementation makes the property private and named with its interface prefix, so `GetProperty("Id")` should miss it and raise a convention error. **Unverified by any current test** — worth pinning whichever way it behaves | Low | No |
| 6 | Refresh the `AGENTS.md` "Known Deviations" table | Deviations 1–3 (no test project; `net461`-era TFM list; undotted monikers) are all **already fixed** on this branch, but still listed as outstanding | Low | No |

### Refinement 1 in detail — the strongest finding

`IBaseDataAccess` mandates `TransactionStart()`, `TransactionCommit()`, and `TransactionRollBack()` for **every**
DAL, and `BaseDataAccess` declares all three `abstract`, so every derived class is forced to supply them.

The evidence that this is misplaced is in the reference implementation itself. `ProphetsWay.Example.DataAccess.NoDB`
implements all three as:

```csharp
public override void TransactionCommit()
{
    throw new NotImplementedException();
}
```

Meanwhile EFTools implements them for real against `Context.Database.CurrentTransaction`.

This contradicts the library's own design language. Retrieval capability is **composed optionally** —
`IBaseGetAllDao<T>` and `IBasePagedDao<T>` are siblings precisely so an entity declares only what it supports.
Transactions are the one capability that is **mandated** instead. A non-transactional store — in-memory, a
read-only API-backed DAL, some document stores — must declare three members it cannot honor, and a caller holding
`IBaseDataAccess` cannot tell the difference until it throws at runtime.

Since swappability is the entire purpose of the library, a contract that forces a legitimate implementation to
lie about its capabilities directly undermines that purpose. The fix follows the pattern already established:

```csharp
public interface IBaseTransactionalDataAccess : IBaseDataAccess
{
    void TransactionStart();
    void TransactionCommit();
    void TransactionRollBack();
}
```

Business logic that needs a transaction depends on the narrower interface and gets a **compile-time** answer
instead of a runtime `NotImplementedException`.

This is binary-breaking — it removes members from a published interface and changes `BaseDataAccess`'s abstract
surface — so it requires a major version bump and a CHANGELOG entry, and it must be coordinated with EFTools and
both copies of Example. Recommend scheduling it for v4 rather than doing it now.

## Note on Apparent Deviations That Are Correct

Do not "fix" these:

- The test project targets `net48;net8.0;net9.0` while the library targets `netstandard2.0;net48;net8.0;net9.0`.
  `netstandard2.0` is not a runnable test target; this is correct.
- `Get<T>(object id)` takes `object` rather than a typed key. This is deliberate and documented — it lets one DAL
  mix `int`, `long`, and `Guid` keys across entities, which Example actually does.
- `{TypeName}Id` winning over `Id` when an entity exposes both is pinned by a test. Any change to identifier
  resolution order would break it.
