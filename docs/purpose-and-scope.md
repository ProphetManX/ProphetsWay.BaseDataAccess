# Purpose & Scope — ProphetsWay.BaseDataAccess

_Refreshed 2026-08-13 against the working tree of branch `net-10-update` (PR #39), which retargets the library for
v3.1.0. The factual base — API inventory, packaging audit, target frameworks, test coverage — is
[docs/repo-profile.md](repo-profile.md) and is not re-derived here. Deferred decisions live in
[docs/feature-requests.md](feature-requests.md)._

## Proposed One-Sentence Purpose

A storage-neutral contract vocabulary for a Data Access Layer — entity markers, capability-composed DAO
interfaces, and one aggregate DAL interface — plus an optional reflection dispatcher that lets business logic
call those contracts generically.

## Current Purpose (as implied by README/csproj)

The csproj `<Description>` now opens:

> A storage-neutral contract vocabulary for a Data Access Layer (DAL): entity marker interfaces, DAO interfaces
> composed by capability (CRUD, retrieve-all, paging), and one aggregate DAL interface for business logic to depend
> on, so an entire DAL implementation can be replaced with minimal impact on its consumers. … Also included, and
> optional, is the abstract BaseDataAccess class.

It used to say *"a group of interfaces to define any/all interactions required for a Data Access Layer (DAL)"*, which
is what the drift section below was written against. **The stated purpose and the real purpose now agree** — the
current description is the proposed one-sentence purpose above, expanded. The README leads with decoupling and DAL
swappability, which also matches the code.

## The Drift

Three specific gaps between what the library claims and what it does. **One of the three is now closed.**

1. ~~**"any/all interactions" over-claims.**~~ **Closed — the description was rewritten**; see above. The fact it
   rested on is still true and still worth stating plainly: the library defines exactly four CRUD operations plus
   `GetAll`, `GetPaged`, and `GetCount`. There is no contract for querying by arbitrary predicate, bulk operations,
   projections, async, or cancellation. Consumers add those to their own entity DAOs. The description now describes
   that *small, deliberately minimal* vocabulary rather than papering over it — the minimalism reads as the feature
   it is.

2. **`IBaseIdEntity<T>` plays no runtime role.** It is declared, and it is never consumed anywhere inside the
   library. `Get<T>(object id)` resolves the identifier **by name** — `{TypeName}Id`, falling back to `Id` — and
   never consults the interface. An entity implementing `IBaseIdEntity<int>` and an entity implementing only
   `IBaseEntity` are treated identically. The test entities prove this: every one of them implements only
   `IBaseEntity`, including those exercising identifier resolution.

3. **`IBaseSoftEntity` declares a vocabulary the library never honors.** `Delete<T>` dispatches to `Delete(T)`
   regardless of whether `T` is soft-deletable. All soft-delete behavior lives in EFTools. This is defensible
   layering — contracts describe data shape, implementations own policy — but the name implies enforcement that
   does not exist here.

None of these is drift in *scope*. The library is doing the right job. They are drift in *stated* scope, and the
one that was worth fixing has been fixed. The two that remain are documented rather than corrected, which is the
right treatment — both describe deliberate layering, not a mistake.

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
| Stable surface | Passes | Hardened and pinned by 115 tests in v3.0.0, 116 today; v3.1.0 touched only XML `<remarks>` in four files — no member, signature, or behavior changed. |
| Not already solved | Inconclusive | `UNVERIFIED — check nuget.org` for convention-dispatch libraries. |

Failing criterion 1 is decisive. Splitting would produce two packages that are **always installed together**,
double the version lines, changelogs, and pipelines, and hand consumers a version-matrix problem — in exchange
for letting a hypothetical user skip roughly 250 lines of a library that is 11 files total.

**No extraction candidate clears the bar, so `docs/nuget-extraction-proposal.md` was not written.**

**Re-verified at v3.1.0 — the conclusion is unchanged.** The retarget touched only `<TargetFrameworks>`: no source
file was added or removed, no public type or member changed, and the package still carries zero `PackageReference`
entries across eleven files. Every cluster in the map above therefore still fails criterion 1 for exactly the reason
it failed before. No proposal file has been created, because writing one would be recording a conclusion of *no*.

### The opposite problem: is this repo too thin to stand alone?

Eleven source files is small enough to ask whether it should fold into EFTools. **It should not.** The entire
value proposition is that a consumer's contracts project references *only* the vocabulary, with no EF, no
provider, and no `DbContext` anywhere in its dependency graph. Merging it into EFTools would drag Entity
Framework into every contracts project and destroy the decoupling the library exists to provide. The thinness is
the point.

## In Scope

- Entity marker and shape contracts (`IBaseEntity`, `IBaseIdEntity<T>`, `IBaseSoftEntity`, `IBaseSoftIdEntity<T>`).
- Capability-composed DAO contracts (`IBaseDao<T>`, `IBaseGetAllDao<T>`, `IBasePagedDao<T>`).
- The aggregate DAL contract, `IBaseDataAccess` — which as of v3.0.0 extends `IDisposable` and carries a
  specified disposal contract and a specified transaction contract.
- The optional reflection dispatcher and its convention, including `DataAccessConventionException`.
- `BaseDataAccessHelper` staying `internal` — correctly not part of the public surface.

## Out of Scope (and where it should live instead)

| Thing | Where it belongs | Status |
| --- | --- | --- |
| Soft-delete *behavior* (filtering, delete-as-update) | The DAL implementation — EFTools `BaseSoftDao`, `RootBaseSoftDao` | Correct today |
| Provider types (`DbContext`, `SqlConnection`) | Implementation packages | Correctly absent |
| Entity-specific queries | The consumer's own `I{Entity}Dao` | Correct by design |
| Async / cancellation surface | Not present anywhere | Genuine gap — see refinement 4 |

## Scope Gate — the near-term roadmap

Every entry in [feature-requests.md](feature-requests.md) measured against the one-sentence purpose at the top of
this document. **Seven of the eight sit correctly.** One is only in scope for the family rather than for this
package, and the one that would have widened the purpose has since been rejected outright, which settles it.

| # | Planned work | Verdict | Because |
| --- | --- | --- | --- |
| 1 | A published conformance kit | **Out of scope for this package — in scope for the family** | See below. Now **deferred** pending EFTools and possibly BPA — a second and third real implementation are what define "conforming" |
| 2 | Nested transactions and savepoints | **Out of scope** | A richer transaction model is a unit-of-work framework, which is a different product. Correctly deferred |
| 3 | A general thread-safety contract | **In scope** | Specifying behavior on the contracts *is* the job; the vocabulary already carries disposal and transaction rules. Deferred on cost, not on fit |
| 4 | Async members and `IAsyncDisposable` | **In scope** | An async CRUD surface is still contract vocabulary. The purpose sentence needs no change; only the timing and the breaking-change budget are open |
| 5 | A standalone count capability | **Out of scope** | A useful count carries filters a generic member cannot express. Correctly rejected — the consumer's own `I{Entity}Dao` is the right home |
| 6 | Splitting transactions into `IBaseAccessWithTransactions` | **In scope, and it sharpens the purpose** | It makes a capability composable that is currently mandated, which is the design language the rest of the library already speaks |
| 7 | Making a swallowed rollback failure observable | **Moot — rejected** | It would have widened the purpose; it is not being built. See below |
| 8 | Source Link and symbol packages | **In scope** | Packaging and lifecycle, not surface. No member changes, so the purpose sentence is untouched. Deferred by decision |
| — | The v3.1.0 retarget itself | **In scope** | Packaging and lifecycle. No public surface moved; nothing in this document's analysis depends on the target list |

### Entry 1 — the right answer is a sibling package, not a bigger one

The conformance kit is worth building and must **not** ship inside `ProphetsWay.BaseDataAccess`. **It is now
deferred** — not rejected — until `ProphetsWay.EFTools` is built and updated onto 3.x, and possibly until
`ProphetsWay.BPA` exists, on the reasoning that a second and third real implementation are what will define what
"conforming" means. The dependency argument below is unaffected by that timing and is the part that must survive
into whatever is eventually built. The package's
strongest property is that it has zero package references, which is what makes it safe to reference from a contracts
project. A test base class drags in xUnit, or whichever framework wins that argument, and would put a test dependency
in the dependency graph of every consumer's contracts project. That is the exact coupling this library exists to
prevent.

Note the tension with the extraction analysis above: a conformance kit fails criterion 1 — *independently useful* —
for the same reason the dispatcher does, since nobody installs it without the contracts. **For a test-support package
that criterion does not govern.** The decisive argument is dependency direction, and it points one way and clearly:
whatever is required to *verify* a contract must never be in the package that *declares* it. Build it as a sibling.
That is the only new package this repo should grow, and it is an addition rather than an extraction, so it does not
change the "no extraction candidate clears the bar" conclusion.

### Entry 7 — settled, and settled the right way

**This has been rejected, so the widening is moot and the one-sentence purpose stands unchanged.** The analysis
below is kept because it is the reason the rejection is correct rather than merely convenient.

Making a swallowed rollback failure observable means putting a diagnostics sink — a callback, a listener, something
— on `IBaseDataAccess`. That is a **different kind of member** from everything else on the interface. Every current
member describes how data is reached. A sink describes how the implementation reports on itself, which is an
observability concern, and observability concerns have a habit of growing: once one sink exists, the case for a
second one is easier to make.

Had it proceeded, the one-sentence purpose would have had to gain something like *"…and a minimal hook for
reporting failures a caller cannot otherwise see"*, with the csproj description and README following. The owner
declined on a simpler ground — that an implementer should not be made to carry machinery for a failure mode they
may never hit — which lands on the same side as the scope argument. The cheaper alternative it leaves in place is
the one already in effect: the rule stays documented and unverifiable, and implementations report through their own
existing infrastructure without the library naming a sink at all. Recorded as entry 7 in
[feature-requests.md](feature-requests.md).

## Recommended Refinements

Ordered by value. None requires a package split.

| # | Change | Rationale | Effort | Breaking? |
| --- | --- | --- | --- | --- |
| 1 | Split transaction members out of `IBaseDataAccess` into `IBaseAccessWithTransactions` | See below — strongest finding | Medium | **Yes** — v4 |
| 2 | ~~Document that `IBaseIdEntity<T>` has no runtime role in identifier resolution~~ | **Done in v3.0.0.** Stated in the `<remarks>` on `IBaseIdEntity<T>` and in the README | — | No |
| 3 | ~~Rewrite the csproj `<Description>` to drop "any/all interactions"~~ | **Done.** The description now describes the minimal vocabulary and the optional dispatcher; drift 1 is closed | — | No |
| 4 | Decide explicitly whether async belongs in the vocabulary | A modern EF/Cosmos DAL is async-first; a sync-only contract forces `.Result` or a parallel hand-rolled surface. **The decision is now recorded** — deferred wholesale, and breaking when taken up; see entry 4 in [feature-requests.md](feature-requests.md). What remains is the timing, and v3.1.0 does not change it — see below | Low (decision) | No (decision only) |
| 5 | ~~Add a test for an entity that implements `IBaseIdEntity<T>` *explicitly*~~ | **Done.** A characterization test in `BaseDataAccessIdResolutionTests` now pins it: explicit implementation makes the property non-public and interface-qualified, both `GetProperty` lookups miss, and a `DataAccessConventionException` is raised before dispatch. The behaviour is correct; the *message* is not diagnosable for this case, which is now entry 9 in [feature-requests.md](feature-requests.md) | — | No |
| 6 | ~~Refresh the `AGENTS.md` "Known Deviations" table~~ | **Done.** All three deviations shipped as fixed in v3.0.0 and the table now records an empty state | — | No |

### What the LTS-only target standard changes here — nothing in scope, one thing in timing

The workspace standard is now `netstandard2.0` plus exactly one modern LTS target, never a Standard Term Support
release, and v3.1.0 brings this library onto it. **It changes no scope judgement in this document.** Target
frameworks decide who can *install* the package; they say nothing about what the package should *contain*, and the
cohesion map, the extraction verdict and refinements 1 through 6 are all arguments about content.

It does settle one piece of timing, and settles it in the unwelcome direction. Entry 4 in
[feature-requests.md](feature-requests.md) notes in passing that `netstandard2.0` makes `IAsyncDisposable` awkward —
true, and easy to read as a constraint that expires once the old targets are dropped. **It does not expire.**
`netstandard2.0` is an API contract rather than a runtime, so it is permanent by design and is the whole reason this
package reaches .NET Framework at all now that the dedicated `net48` asset is gone. Whenever async is taken up, it
will be taken up *with* that floor in place, not after it lifts. That strengthens the existing decision to defer
async wholesale rather than half of it: the friction is structural, not temporary.

### Refinement 1 in detail — the strongest finding

`IBaseDataAccess` mandates `TransactionStart()`, `TransactionCommit()`, and `TransactionRollBack()` for **every**
DAL, and `BaseDataAccess` declares all three `abstract`, so every derived class is forced to supply them.

v3.0.0 **specified** what those three members mean — one transaction per instance, no nesting, rolled back on
disposal — but it did not change *who* has to implement them. The finding below is therefore untouched by that
work: specifying an obligation more precisely does not make it optional.

**The original motivation has been set aside by the owner; the change survives on a better one.** The finding was
first argued as *non-transactional stores are forced to lie*, pointing at `ProphetsWay.Example.DataAccess.NoDB`,
which implemented all three members as `throw new NotImplementedException();`. Two things happened to that
argument. The evidence expired — at the tip of `ProphetsWay.Example`, `NoDB` now implements all three for real
against a `TransactionLog` undo log that replays writes in reverse on rollback, and the `NotImplementedException`
version survives only in the older commit `ProphetsWay.EFTools` pins its submodule to. And the owner does not
accept the premise anyway: he cannot think of a real database implementation that would not want transactions in
some capacity, so "forced to lie" is not a case he needs answered.

What he does accept is the **shape**, and the shape is where the real win was all along:

- **`BaseDataAccess` forces three `override`s on every derived DAL, whether or not it has anything to say.** The
  three members are declared `abstract`, so there is no inherited implementation and no way to decline. That is a
  tax on every implementer that inherits the dispatcher, paid in boilerplate, and it is unrelated to whether the
  store is transactional.
- **A caller still gets no compile-time answer.** A read-only API-backed DAL, a document store with no
  multi-document transaction, or a façade over a third-party service cannot be distinguished from a fully
  transactional DAL by anyone holding `IBaseDataAccess` until it throws at runtime.
- **The capability is mandated rather than composed**, which contradicts the library's own design language.
  Retrieval capability is composed optionally — `IBaseGetAllDao<T>` and `IBasePagedDao<T>` are siblings precisely
  so an entity declares only what it supports. Transactions are the one capability that is mandated instead.

The fix follows the pattern already established, with one difference that carries the whole benefit:
**`BaseDataAccess` does not implement the new interface, and therefore stops declaring the three members
abstract.** A DAL that wants transactions implements them as *interface members*, not as overrides of inherited
abstract members.

```csharp
public interface IBaseAccessWithTransactions
{
    void TransactionStart();
    void TransactionCommit();
    void TransactionRollBack();
}

public class MyDataAccess : BaseDataAccess, IBaseAccessWithTransactions
{
    //no 'override' keyword anywhere in here
    public void TransactionStart() { /* ... */ }
}
```

Business logic that needs a transaction depends on the narrower interface and gets a **compile-time** answer
instead of discovering the truth at runtime.

**One thing the split now has to answer that it did not when this was first written.** v3.0.0 put `IDisposable`
on `IBaseDataAccess`, and the disposal rules it carries reference transactions directly — *"a transaction still
open at disposal is rolled back."* If the transaction members move to `IBaseAccessWithTransactions`, whoever
executes the split has to decide where disposal belongs and what the base interface's disposal rules say once
transactions are no longer guaranteed to be present. That is a decision, not an obstacle, but it is one more
thing the split must settle. Recorded as entry 6 in [feature-requests.md](feature-requests.md).

This is binary-breaking — it removes members from a published interface and changes `BaseDataAccess`'s abstract
surface — so it requires a major version bump and a CHANGELOG entry, and it must be coordinated with EFTools and
Example. (Example is a **git submodule** of EFTools, not a second copy, so there is one source to change and a
pinned pointer to advance — cheaper than this document previously assumed, though EFTools has not picked up 3.x at
all yet.) Recommend scheduling it for v4 rather than doing it now.

**v3.1.0 changes none of this calculus.** The retarget altered no member, no signature and no implementation; the
finding is exactly as valid, and exactly as breaking, as it was at v3.0.0.

## Note on Apparent Deviations That Are Correct

Do not "fix" these:

- **The test project targets `net48;net10.0` while the library targets `netstandard2.0;net10.0`.** As of v3.1.0 the
  library ships no `net48` asset and the test project still runs a `net48` leg, which looks like something that was
  missed. It was not, and there are two reasons rather than one. First, `netstandard2.0` is not a runnable test
  target, so a test project must name runtimes directly — that part was always true. Second, a `net48` test leg is
  how .NET Framework *behavior* is verified, which is a different thing from the library merely *supporting*
  Framework through `netstandard2.0`: `Activator.CreateInstance<T>()` wraps a throwing constructor on .NET Framework
  and does not on .NET Core, and that leg is the only thing exercising the catch block in `CreateEntity<T>()`.
  Dropping it would leave a documented behavior untested. It now binds the `netstandard2.0` asset, i.e. the exact
  assembly a .NET Framework consumer receives, which makes it a better test than it was before.
- `Get<T>(object id)` takes `object` rather than a typed key. This is deliberate and documented — it lets one DAL
  mix `int`, `long`, and `Guid` keys across entities, which Example actually does.
- `{TypeName}Id` winning over `Id` when an entity exposes both is pinned by a test. Any change to identifier
  resolution order would break it.
