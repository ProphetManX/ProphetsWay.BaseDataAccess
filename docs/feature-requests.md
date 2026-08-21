# Feature Requests & Deferred Decisions — ProphetsWay.BaseDataAccess

This is the record of things that were **considered and deliberately not built**, together with the
reasoning behind each decision. Nothing here is a limitation, an apology, or a TODO list. Each entry
exists so a future developer — or a future AI agent — can find the decision, judge whether the tradeoff
that produced it still holds, and reopen it as a real feature request when it does not.

**If you are about to propose one of these, read its entry first.** The entry tells you what was already
weighed, so your proposal can start from the open questions rather than from the beginning.

The contract itself is **not** restated here. The binding rules live in the `<remarks>` on
[`IBaseDataAccess`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs) and the reflection convention lives
in the `<remarks>` on
[`DataAccessConventionException`](../ProphetsWay.BaseDataAccess/DataAccessConventionException.cs). Those
are the source of truth. This file links to them and does not duplicate them, because duplicated rules drift.

## Index

| # | Item | Status |
| --- | --- | --- |
| 1 | [A published conformance kit](#1--a-published-conformance-kit) | **Deferred** — revisit after EFTools, possibly after BPA |
| 2 | [Nested transactions and savepoints](#2--nested-transactions-and-savepoints) | Deferred — out of scope by decision |
| 3 | [A general thread-safety contract](#3--a-general-thread-safety-contract) | Deferred — deliberately unspecified |
| 4 | [Async members and `IAsyncDisposable`](#4--async-members-and-iasyncdisposable) | Deferred — breaking when taken up |
| 5 | [A standalone count capability](#5--a-standalone-count-capability) | **Rejected** — not deferred |
| 6 | [Splitting transactions into `IBaseAccessWithTransactions`](#6--splitting-transactions-into-ibaseaccesswithtransactions) | Scheduled for a possible v4 |
| 7 | [Making a swallowed rollback failure observable](#7--making-a-swallowed-rollback-failure-observable) | **Rejected** — not deferred |
| 8 | [Source Link and symbol packages](#8--source-link-and-symbol-packages) | Deferred — declined for now |
| 9 | [A diagnosable `DataAccessConventionException` message for explicit interface implementation](#9--a-diagnosable-dataaccessconventionexception-message-for-explicit-interface-implementation) | **Proposed** — v3.2.0 at the earliest |
| 10 | [Nullable reference type annotations on the public surface](#10--nullable-reference-type-annotations-on-the-public-surface) | **Scheduled** — v3.2.0, high priority; picked up **after** `ProphetsWay.EFTools` 3.0.0 ships |

Numbers are permanent. Entries are never renumbered and never removed — [purpose-and-scope.md](purpose-and-scope.md)
cites entries by number, and a rejected entry is decision history rather than dead weight.

**One caveat about [purpose-and-scope.md](purpose-and-scope.md) as of entry 10:** its *Scope Gate — the
near-term roadmap* table enumerates entries 1–8 and its *Recommended Refinements* table uses its own separate
numbering. Neither has been updated for entries 9 or 10 — only `Purpose Refiner` writes that file, and this
session was scoped to this one. Do not read the absence of a row there as a scope ruling.

## Release Eligibility — v3.1.0

v3.1.0 is a **minor** release: a target-framework retarget plus documentation. No public member, signature or
implementation changed in it, so **nothing binary-breaking can land in it.** That rules out most of this file
by construction.

| # | Status | Eligible for v3.1.0? | Why |
| --- | --- | --- | --- |
| 1 | Deferred | **No** | Not started, and if built it ships as a *sibling package* with its own version line — it could not ride this release even if it were finished |
| 2 | Deferred | **No** | Out of scope by decision; nothing to land |
| 3 | Deferred | Technically yes — **but no** | Specifying threading adds no member, so it would be documentation-only and non-breaking. It is deferred on cost, not on eligibility, and no work exists to ship |
| 4 | Deferred | **No** | Adding members to a published interface breaks every implementation. v4 at the earliest |
| 5 | Rejected | n/a | Decided against |
| 6 | Scheduled for a possible v4 | **No** | Removes members from a published interface and changes `BaseDataAccess`'s abstract surface. Binary-breaking by definition |
| 7 | Rejected | n/a | Decided against |
| 8 | Deferred | Technically yes — **but no** | Source Link and a `.snupkg` are packaging-only and non-breaking, so a minor release is the right home for them. Declined by the owner for now; see the entry |
| 9 | Proposed | **No** | Changing an exception's message is a **behaviour change**, and v3.1.0 changed no behaviour by construction. It also requires a test-assertion change to land first. v3.2.0 at the earliest |
| 10 | Scheduled | **No** | Filed after v3.1.0 shipped, and it raises `<LangVersion>` and rewrites the compiled metadata of most annotated members. Scheduled for v3.2.0 by owner decision |

**The honest answer is none.** The two non-breaking candidates — 3 and 8 — are both deferred by decision rather
than by version constraint, entry 9 arrived after the release was already scoped as documentation-only, and
everything else is either breaking or already closed.

---

## 1 — A published conformance kit

**Status:** **Deferred — explicitly not rejected.** The problem is settled and the value is accepted; the shape of
the answer is not settled, and the owner is not ready to spend time on it while the library is deliberately this
open-ended and flexible.

**Revisit trigger — the useful part of this decision.** Reopen this **after `ProphetsWay.EFTools` has been built
and updated onto 3.x**, or possibly after `ProphetsWay.BPA`. The reasoning is that a second and a third *real*
implementation are what will teach us what "conforming" actually means. Designing a conformance kit against one
hand-written in-repo implementation and one in-memory reference DAL is designing against a sample of two, and the
open questions below are exactly the ones a real database-backed implementation answers for free.

**One constraint is already settled and survives the deferral:** if this is ever built it ships as a **sibling
package** — working name `ProphetsWay.BaseDataAccess.Conformance` — and **never inside the contracts package.**
The contracts package's strongest property is that it carries zero package references, which is what makes it safe
for a consumer's contracts project to depend on. A test base class drags in a test framework, and putting that in
the dependency graph of every consumer is the exact coupling this library exists to prevent. Recorded in full as
the entry-1 note in [purpose-and-scope.md](purpose-and-scope.md).

### The problem

**Every rule on [`IBaseDataAccess`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs) is an obligation on an
implementer that nothing currently checks.** The interface specifies disposal semantics, transaction
semantics, and — for anyone deriving from `BaseDataAccess` — a reflection-dispatch convention. It then trusts
every Data Access Layer to have read the documentation and honoured it.

`ProphetsWay.BaseDataAccess` cannot close that gap itself. `BaseDataAccess` declares `Dispose` and all three
transaction members `abstract` and holds no connection, context, or transaction state of its own. It has
nothing to enforce the rules *with*. It can only require that an implementer decide.

The 35 tests in `ConformingDataAccessDisposalTests` and `ConformingDataAccessTransactionTests` are worth
being precise about, because they are easy to over-read. Their subject is `ConformingDataAccess` — a
hand-written implementation in the test project — not `BaseDataAccess`. They prove that a correct
implementation is **expressible**, and they pin the contract's meaning so it cannot drift silently. They
prove nothing at all about any *other* implementation. A Data Access Layer that gets disposal wrong is not
caught by them, and is not caught by anything else either.

### The proposal

Publish a package — working name **`ProphetsWay.BaseDataAccess.Conformance`**, which does not exist and is a
name for discussing the idea, not a package to go looking for — containing an abstract xUnit test base. An
implementer derives from it, supplies a factory that produces their own Data Access Layer, and runs it.
Passing would be evidence that they honour the contract.

The shape, roughly:

> **Illustrative** — not currently present in the repo, and not a proposed API surface. It exists to make the
> ergonomics concrete enough to argue about.

```csharp
public class MyDalConformanceTests : BaseDataAccessConformanceTests
{
	protected override IBaseDataAccess CreateDataAccess()
	{
		return new MyDataAccess(/* whatever it needs */);
	}
}
```

Everything else — the assertions, the fixtures, the naming — comes from the base class.

### What it would cover

The existing 35 tests are the natural first draft of its contents. They already correspond one-to-one with
the rules on the interface:

| Area | Behaviour proved |
| --- | --- |
| Disposal | `Dispose` is idempotent; a second call is a no-op and never throws |
| Disposal | Every member throws `ObjectDisposedException` once disposed |
| Disposal | A transaction still open at disposal is **rolled back**, never committed |
| Disposal | Already-committed work survives disposal |
| Disposal | No roll back is attempted when the instance is disposed with nothing open |
| Disposal | A rollback that fails during disposal does not propagate, and the instance is still disposed |
| Disposal | Use-after-dispose and transaction misuse are catchable by one `InvalidOperationException` handler |
| Transactions | `TransactionStart` throws when a transaction is already open — transactions do not nest |
| Transactions | `TransactionCommit` and `TransactionRollBack` throw when nothing is open, including on a second call |
| Transactions | A failed commit leaves **no** transaction open, and a new one can be started afterwards |
| Transactions | Writes inside a committed transaction persist; writes inside a rolled-back or failed one do not |
| Transactions | A write with no transaction open auto-commits on its own |
| Transactions | Transaction scope is the **instance** — a write through another instance is not enrolled, and closing one instance's transaction leaves another's open |
| Convention | Required method names, parameter types, declared return types, and public instance visibility |
| Convention | Identifier resolution — `{TypeName}Id` before `Id`, and the setter requirement |

The convention rows apply only to an implementation that derives from `BaseDataAccess`. An implementation
that implements `IBaseDataAccess` directly is exempt from them by design, which is itself a fact the kit has
to model rather than ignore.

### Open questions

These are genuinely open. The proposal is not finished thinking, and anyone picking it up should expect to
answer these before writing code.

1. **It couples consumers to xUnit.** The house convention is xUnit, so for these repos that is free — but a
   *published* package imposes that choice on every downstream implementer, including ones with an NUnit or
   MSTest suite. Is that acceptable? The alternative is a framework-agnostic core — a set of plain assertion
   methods, or a runner that returns a result object — with thin per-framework adapters layered on top. That
   is more work and a less pleasant developer experience, and it is the main fork in the road.

2. **Database-backed implementations need setup, teardown, and isolation that in-memory ones do not.** The
   in-memory case is trivial: construct, run, throw away. A SQL-backed Data Access Layer needs a schema to
   exist, needs each test isolated from the last, and needs a teardown that runs even when a test fails. The
   kit must give an implementer somewhere to supply that — probably overridable hooks — **without becoming a
   test framework in its own right.** Getting this wrong in either direction is the main way this idea fails:
   too little and it is unusable against a real database, too much and it is a second framework to learn.

3. **Which target frameworks must it reach?** As of v3.1.0 the library targets `netstandard2.0;net10.0` and the
   test project targets `net48;net10.0`. `netstandard2.0` is not a runnable test target, so the kit has to name
   runtimes directly — and the retarget makes that question *wider*, not narrower. A `netstandard2.0`-only
   library is installable on .NET Framework 4.6.1+ and on every .NET Core and .NET 5+ runtime, so the set of
   frameworks an implementer might run their suite on is now larger than the set the library itself names. The
   kit's target list has to be chosen against the reach floor, not against the library's asset list.

4. **One kit, or one per capability?** The DAO interfaces compose by capability on purpose:
   `IBaseGetAllDao<T>` and `IBasePagedDao<T>` are siblings, and an entity declares only what it supports. A
   single monolithic kit would contradict that design language by demanding every implementer prove
   behaviour they never claimed. A per-capability set of kits matches the library's own grain — at the cost
   of more moving parts and a more complicated first-run experience.

5. **`ProphetsWay.EFTools` and `ProphetsWay.Example` are the first two consumers and the natural proving
   ground.** They are also the honest test of whether the idea works: EFTools is database-backed and
   Example's `NoDB` implementation is in-memory, so between them they exercise exactly the tension in
   question 2. If the kit cannot serve both cleanly, it is not ready to publish. **Correction to what this
   entry used to say:** `NoDB` no longer throws `NotImplementedException` from its transaction members. At the
   tip of `ProphetsWay.Example` it implements all three for real, against a `TransactionLog` undo log that
   replays writes in reverse on rollback. The `NotImplementedException` version survives only in the older
   commit that `ProphetsWay.EFTools` pins its submodule to. That **raises** the bar rather than lowering it:
   both reference implementations are now fully transactional, so the kit can assume transactional behaviour
   from the two implementations it can actually test against — which is precisely why the revisit trigger above
   waits for implementations that are not these two.

### Why it fits this library in particular

This is the natural companion to the argument [`ProphetsWay.Example`](https://github.com/ProphetManX/ProphetsWay.Example)
exists to make: that the same tests pass against completely different Data Access Layer implementations.
Example demonstrates that for one hand-built domain. A conformance kit would generalise it — turning a
demonstration into something every implementer can run against their own code, and turning "we documented
the contract" into "the contract is checkable."

---

## 2 — Nested transactions and savepoints

**Status:** Deferred. Out of scope by decision.

Transactions on this interface are a scalpel for a bounded batch of writes — open one, do the batch, close
it. They are not an ambient transaction system and not a unit-of-work framework. Nesting and savepoints
belong to the richer model, and the decision was to keep the narrow one.

Richer semantics would be a **future addition to the interface**, arrived at deliberately. The rule that
follows from that is the important part of this entry: **an implementation must not improvise them
underneath the current contract.** A Data Access Layer that quietly makes `TransactionStart` re-entrant,
or maps it onto savepoints, has changed the meaning of a contract its callers rely on.

The per-member rules are on `TransactionStart` in
[`IBaseDataAccess`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs).

---

## 3 — A general thread-safety contract

**Status:** Deferred. Deliberately unspecified.

Exactly one threading consequence is documented, and it follows from the design rather than from a
guarantee: transaction state belongs to the instance, so **an instance must not be used from more than one
thread while a transaction is open.**

Beyond that, no thread-safety guarantee is made in either direction. Defining a broader threading contract —
what is safe concurrently, what an implementation must synchronise, what a caller may assume — is a feature
in its own right, with real cost to every implementer, and it has not been taken on.

---

## 4 — Async members and `IAsyncDisposable`

**Status:** Deferred wholesale, and deliberately.

The interface is entirely synchronous, and `IAsyncDisposable` is **not** implemented. The two were
considered together on purpose: async disposal without async CRUD is half a feature — it adds a second
disposal path to every implementation while leaving every read and write blocking, which is the part that
actually costs a caller.

**If async members are ever added, that is itself a breaking change.** Adding members to a published
interface breaks every existing implementation. That cost is known and accepted as the price of taking async
seriously later rather than half-heartedly now — it is not an oversight, and it should not be reported as
one.

The `netstandard2.0` target makes `IAsyncDisposable` awkward, which is a practical reason to wait but not the
reason for the decision. The reason is the half-feature problem. **The awkwardness does not expire**, and it is
worth being clear about that: `netstandard2.0` is an API contract rather than a runtime, so it is permanent by
design, and after the v3.1.0 retarget it is the only thing carrying this package to .NET Framework at all. Async
will be taken up *with* that floor in place, not after it lifts.

Related: refinement 4 in [purpose-and-scope.md](purpose-and-scope.md) records the same question from the
scope side — that a modern async-first Data Access Layer must currently choose between `.Result` and a
parallel hand-rolled surface.

---

## 5 — A standalone count capability

**Status:** **Rejected.** Not deferred — this one was decided against, and reopening it needs a new argument
rather than new timing.

`GetCount<T>()` exists as a **required component of paging**, not as a feature in its own right. A paged view
cannot show how many pages there are, or where the last one ends, without the total it is paging over. That
is the whole reason the member is on the interface.

Counting as an independent capability was considered and rejected on that basis. A consumer who wants a bare
count wants it *for a reason* — active users, unpaid invoices, records touched since a date — and that reason
almost always carries filters. A generic `GetCount<T>()` cannot express any of them, so a standalone
capability would satisfy the trivial case and be useless for the real one.

The right home for such a count is the consumer's own `I{Entity}Dao`, as a custom method that carries the
filters that made it worth asking for. The reasoning is stated on `GetCount<T>` in
[`IBaseDataAccess`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs).

---

## 6 — Splitting transactions into `IBaseAccessWithTransactions`

**Status:** Scheduled for a possible v4. **Binary-breaking**, and it has to be coordinated with
`ProphetsWay.EFTools` and `ProphetsWay.Example`.

This entry has been rewritten. The *shape* of the change is accepted; the argument that originally motivated it
is not, and the evidence that argument rested on has expired. Both are recorded below, because the difference
between them decides how the split gets designed.

### The motivation that was NOT accepted

The split was first argued as *non-transactional stores are forced to lie* — a store with no native notion of a
transaction has to either throw from three mandated members or synthesize a capability nobody asked it for.
**The owner does not find that compelling:** he cannot think of a real database implementation that would not
want transactions in some capacity. Do not re-run that argument; it has been heard and set aside.

The evidence behind it has expired independently, and this is a correction to what this entry and
[purpose-and-scope.md](purpose-and-scope.md) used to claim. `ProphetsWay.Example.DataAccess.NoDB` **no longer
throws `NotImplementedException`** from its transaction members. At the tip of `ProphetsWay.Example` it
implements all three for real, against a `TransactionLog` undo log. The `NotImplementedException` version
survives only in the older commit `ProphetsWay.EFTools` pins its submodule to. The vivid proof is gone.

### The motivation that IS accepted — the shape

The real win is not about dishonest implementations. It is about what `BaseDataAccess` forces on every derived
DAL today: **three `override`s, written whether or not the implementer has anything to say.** `BaseDataAccess`
declares `TransactionStart()`, `TransactionCommit()` and `TransactionRollBack()` `abstract`, so there is no
inherited implementation to fall back on and no way to decline.

The proposed shape — the working name is the owner's:

- A separate interface, **`IBaseAccessWithTransactions`**, carrying the three transaction members.
- **`BaseDataAccess` does not implement that interface**, and therefore **does not declare the three members
  abstract.** This is the load-bearing part. A DAL that inherits the dispatcher stops inheriting an obligation.
- A consumer's DAL that wants transactions implements them as **interface members**, not as overrides of
  inherited abstract members.

> **Illustrative** — not a proposed API surface, and nothing below exists in the repo.

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

The secondary benefit is the one the original framing buried: business logic that needs a transaction depends on
the narrower interface and gets a **compile-time** answer, instead of discovering at runtime whether the DAL it
was handed can honour one.

### The open question the split must answer

**Disposal now sits on the base interface while the transaction members would move out.** `IBaseDataAccess`
extends `IDisposable` as of v3.0.0 and carries documented disposal semantics — including *"a transaction still
open at disposal is rolled back"*, a rule that mentions transactions from the base interface. Whoever executes
the split has to decide where disposal belongs and what the base interface's disposal rules say once
transactions are no longer guaranteed to be present.

That is a decision, not an obstacle, but it is one more thing the split has to settle — and it did not exist
when the split was first proposed.

### Coordination cost

This removes members from a published interface and changes `BaseDataAccess`'s abstract surface, so it needs a
major version bump and a CHANGELOG entry. **A correction to how that cost used to be stated here:** this entry
referred to *"both copies of Example"*, and there are not two copies. `ProphetsWay.EFTools` consumes
`ProphetsWay.Example` as a **git submodule** — `.gitmodules` declares `path = ProphetsWay.Example`,
`branch = main`. The two cannot drift; the submodule is merely *pinned*, currently to a pre-3.0.0 commit. So
there is **one** source of Example to change and a pinned pointer to advance. The real consequence is a
coordination requirement, not a duplication problem — and EFTools has not picked up 3.x at all yet, which is the
larger part of the work either way.

Related: refinement 1 in [purpose-and-scope.md](purpose-and-scope.md) records the same split from the scope side
and rates it the strongest finding in that document.

---

## 7 — Making a swallowed rollback failure observable

**Status:** **Rejected.** Not deferred — this one was decided against, and reopening it needs a new argument
rather than new timing.

The owner's reasoning, in his words:

> we can't make implementor devs add more things to their project just in case there are errors, that's for them
> to build around.

That is the decision. A diagnostics sink on `IBaseDataAccess` would be an obligation placed on every implementer
to serve a failure mode most of them will never hit, and reporting on their own failures is the implementer's
job and their existing infrastructure's job, not this library's.

The analysis that produced the proposal is preserved below, because the verifiability hole it identified is real
and a future proposal should start from it rather than rediscover it.

### The hole, which is still real

The contract says `Dispose` never throws, and that a rollback failing during disposal is swallowed — an
implementation *may* log it, but must not propagate it. The reasoning is sound: throwing from `Dispose` masks
any in-flight exception inside a `using` block, turning a diagnosable failure into a confusing one.

The consequence is that **"attempted the rollback and swallowed the failure" and "never attempted the
rollback at all" are indistinguishable to a caller.** Both look like a silent `Dispose`. The rule is
therefore stated but **not verifiable from outside the implementation** — which also means a conformance kit
(entry 1) cannot check it, since a kit only sees what a caller sees. **That remains true and is now simply
accepted.** If entry 1 is ever revisited, this rule is one it cannot cover, and it should say so rather than
attempt it.

### What was proposed and rejected

Requiring implementations to report a swallowed failure to a sink supplied by the consumer — a callback, a
diagnostic listener, something along those lines. It would have made the behaviour observable without
reintroducing a throwing `Dispose`, at the cost of a **new API surface** on an interface whose whole appeal is
being small, plus a decision about whether the sink is required or optional and what an implementation does when
none is supplied.

**This also settles a scope question.** [purpose-and-scope.md](purpose-and-scope.md) flagged this entry as *in
scope, but widens it* — a sink describes how an implementation reports on itself, which is an observability
concern rather than a data-reaching one, and taking it up would have required rewriting the library's
one-sentence purpose to admit it. That widening is now **moot**. The purpose sentence stands unchanged.

---

## 8 — Source Link and symbol packages

**Status:** Deferred. Declined for now, with a technical question to check before it is next argued.

`Repo Analyst` recommended enabling Source Link and publishing a `.snupkg` alongside the package. The argument
is a good one and specific to this library: the XML `<remarks>` on `IBaseDataAccess` and
`DataAccessConventionException` **are the product**, so a consumer staring at a `DataAccessConventionException`
ought to be able to step into `BaseDataAccessHelper` and watch the convention resolution that produced it,
rather than reading about it and guessing.

**The owner declined for now**, on two grounds: he does not want to add a project dependency at this time, and
the library is small enough — eleven files, no conditional compilation — to be easy to reason about without
stepping through it.

### Open technical question — **UNVERIFIED, check before relying on it**

The "adds a dependency" objection may be smaller than it appears, and this should be confirmed rather than
assumed by whoever picks this up:

- A `PackageReference` to Source Link carried with `PrivateAssets="all"` is a build-time asset and **does not**
  flow to consumers as a package dependency.
- The .NET SDK from version 8 onward is understood to include Source Link **built in**, which would mean no
  `PackageReference` at all — plausibly just `<PublishRepositoryUrl>` and `<EmbedUntrackedSources>` in the
  csproj, plus `-p:ContinuousIntegrationBuild=true` and a `.snupkg` push step in the pipeline.

**None of that is asserted as fact here.** It is the thing to verify first, because if it holds then the
package's zero-package-reference property survives intact and the decision is being made against a cost that
is not actually there. If it does not hold, the decline stands on its own terms.

Note that any change here touches `prophets-pipelines` as well as the csproj — the `.snupkg` has to be produced
and pushed by the shared templates, which makes this a multi-repo change rather than a one-file one. Packaging
changes of this kind are non-breaking and belong in a minor release.

---

## 9 — A diagnosable `DataAccessConventionException` message for explicit interface implementation

**Status:** **Proposed.** The behaviour is correct; the *message* is the finding. **v3.2.0 at the earliest** —
see the sequencing constraint below, which is mandatory and not obvious.

### How it was found

A characterization test written this session pinned previously untested behaviour: what happens when an entity
implements [`IBaseIdEntity<T>`](../ProphetsWay.BaseDataAccess/IBaseIdEntity.cs) **explicitly** rather than
implicitly.

```csharp
public class Wraith : IBaseIdEntity<int>
{
	int IBaseIdEntity<int>.Id { get; set; }
}
```

`BaseDataAccessHelper` resolves the identifier with
`entityType.GetProperty($"{Name}Id") ?? entityType.GetProperty("Id")`. `Type.GetProperty(string)` binds
`Public | Instance | Static`, and an explicit implementation is non-public *and* reflected under its
interface-qualified name — so **both lookups miss.** The observed behaviour is a clean
`DataAccessConventionException` thrown before dispatch. The suite is green at 232 tests.

### The problem

The exception **type** is right and throwing before dispatch is right. The **message** is wrong for this case,
and wrong in the way most likely to cost a developer an hour:

> The entity type [Wraith] exposes neither a 'WraithId' nor an 'Id' property, so no identifier can be assigned
> to it.

The developer is looking at a source file that visibly declares `Id`, on a type the compiler accepted as
`IBaseIdEntity<int>`. The message tells them the property does not exist. Nothing in it mentions visibility,
explicit interface implementation, or the interface. The natural next thought — *"but it's right there"* — leads
nowhere.

The proposal is to have the message name the **reason** rather than only the absence: detect that a non-public
or interface-qualified candidate exists and say so, pointing the developer at visibility rather than at
existence.

### Scope — the documentation half is already done

**The remaining gap is runtime diagnostics only.** v3.1.0 already states, on
[`IBaseIdEntity`](../ProphetsWay.BaseDataAccess/IBaseIdEntity.cs),
[`DataAccessConventionException`](../ProphetsWay.BaseDataAccess/DataAccessConventionException.cs),
`BaseDataAccess.Get<T>` and `IBaseDataAccess.Get<TEntityType>`, that the identifier property must be **public**
and that an explicit implementation is not sufficient. Do not re-do that work; this entry is about what the
program says at the moment of failure, not about what the documentation says beforehand.

### Constraints on doing it

- **This is a behaviour change, not a documentation change**, so it cannot land in v3.1.0. v3.2.0 at the
  earliest.
- **The sequencing is mandatory.** A test now asserts the current message text with three `ShouldContain` calls
  in `BaseDataAccessIdResolutionTests`. `Test Designer` updates those assertions **first**; `Implementer`
  changes the message **second**. An implementer who changes the message first faces a failing test it is
  forbidden to edit.
- Message text is **not** part of the binary contract, but anyone matching on it in their own tests would be
  affected. A minor compatibility consideration, not a blocker.

### Open question — do not decide it here

How the improved message is produced. Both options are recorded because neither is obviously right:

1. **A second reflection pass.** Re-query with `BindingFlags.NonPublic` to detect a hidden candidate and tailor
   the message to it. Gives a precise diagnosis, and costs a reflection call only on the path that is already
   failing.
2. **Enumerate the failure modes in the existing static text.** Trivial, and cannot regress anything — at the
   cost of handing the developer a list to work through rather than an answer.

---

## 10 — Nullable reference type annotations on the public surface

**Status:** **Scheduled.** Not proposed and not deferred — the owner decided this directly, rated it **high
priority**, and set the target at **v3.2.0**, to be picked up **after `ProphetsWay.EFTools` 3.0.0 ships.** In
his words:

> i want to get these libraries both updated asap, but i want to get EFTools working and out the door first.

The sequencing is the decision, not a guess at capacity: EFTools 3.0.0 is what surfaced this, and it is
shipping with a local workaround rather than waiting on a contracts release.

### How it was found

While building `ProphetsWay.EFTools` 3.0.0's new `BaseDao<TEntity, TKey>` — lap 1 of that cycle — the
implementation declared the truthful signature:

```csharp
public virtual TEntity? Get(TEntity item)
```

The build emitted **three `CS8766` warnings**, one on `BaseDao<TEntity, TKey>` and one on each of
`BaseGetAllDao<TEntity, TKey>` and `BasePagedDao<TEntity, TKey>`, all three of which carry `#nullable enable`:

> Nullability of reference types in return type of `'TEntity? BaseDao<TEntity, TKey>.Get(TEntity item)'`
> doesn't match implicitly implemented member `'TEntity IBaseDao<TEntity>.Get(TEntity item)'`.

### Why this is a defect here rather than in EFTools

Both halves were verified by opening the files, not inherited:

- [`IBaseDao.cs`](../ProphetsWay.BaseDataAccess/IBaseDao.cs) declares `T Get(T item);` and its own `<returns>`
  says *"The loaded entity. For a reference-type entity, `null` when no record carries that identifier."*
  **The interface documents a nullable return in prose and declares a non-nullable one in code.** EFTools'
  signature is the truthful one; the interface as compiled cannot express it.
- [`ProphetsWay.BaseDataAccess.csproj`](../ProphetsWay.BaseDataAccess/ProphetsWay.BaseDataAccess.csproj)
  declares `<TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>` and carries **no `<Nullable>` and no
  `<LangVersion>`.** The package therefore ships null-oblivious, and every consumer with nullable reference
  types enabled is annotating against a surface that tells them nothing.

The mechanical blocker is the constraint. `IBaseDao<T> where T : IBaseEntity` is an **interface** constraint,
which does not imply a reference type, so `T` may be a `struct`. Annotating the return as `T?` on such a type
parameter requires **C# 9 or later**, and `netstandard2.0` defaults to C# 7.3.

**A correction to how that default is described elsewhere.** [repo-profile.md](repo-profile.md) reports
`LangVersion` as unset and reads the resulting C# 7.3 as *"a consequence of the reach floor … it is why
nullable reference types are absent."* The floor sets the **default**, not a ceiling — `<LangVersion>` is
independent of the target framework and can be raised on a `netstandard2.0` target. Only features needing
runtime types that framework lacks fail, and they fail individually. That file is `Repo Analyst`'s to correct;
the correction is recorded here so the next reader does not conclude the fix is impossible.

### The change

Raise `<LangVersion>`, add `<Nullable>annotations</Nullable>`, and annotate the public surface — starting with
`IBaseDao<T>.Get` as `T? Get(T item)`.

**`annotations` rather than `enable` is the cheap half deliberately.** It publishes annotations to consumers
without switching warnings on inside the library, so `BaseDataAccessHelper` — reflection-heavy, `object`-typed
throughout — is not dragged into the same change. The cost is that the annotations are then **asserted by the
author rather than checked by the compiler.** Moving to `enable` later is a second, larger pass and should be
scoped as one.

### What must survive from the decision

1. **It is not binary-breaking.** Nullable annotations are metadata attributes. For an unconstrained `T`, `T?`
   remains `T` at runtime and does **not** become `Nullable<T>`, so no signature changes and no consumer needs
   to recompile. It is potentially *source*-affecting for a consumer with nullable reference types enabled —
   which is the point, since what it affects them with is the truth.
2. **Do not annotate one member and stop.** A partially annotated public surface is worse than an unannotated
   one: a consumer cannot tell an intentional non-null from an unexamined one, and the compiler will not tell
   them either. This is scoped as **one pass over the whole public surface**, not as a one-line fix.
3. **Value-type entities need a deliberate reading.** `T?` on an unconstrained `T` means *"may be default"*,
   not *"may be null"* — which is the correct reading for a `struct` entity, and it agrees with the paragraph
   already on [`IBaseDataAccess.Get`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs) stating that a
   value-type entity cannot report "not found" as `null` and must signal a miss some other way. State that
   agreement in the pass rather than letting someone discover it.
4. **A metadata-only change with no runtime behaviour change fits a MINOR bump.** That is the version
   *implication*, recorded so the release is scoped correctly. **It is not an instruction to apply one** — no
   agent edits `app-variables.yml`.
5. **The trigger was `ProphetsWay.EFTools` 3.0.0 lap 1, and EFTools carries a local suppression of `CS8766`
   that must be removed once this lands.** Named here so it does not become permanent by forgetting.
   **Verification note, 2026-08-20:** a search of the whole `ProphetsWay.EFTools` tree for `8766`, `#pragma
   warning`, `NoWarn` and `SuppressMessage` found **nothing** — the three files carry `#nullable enable` and
   `BaseDao.cs` line 137 declares `TEntity? Get(TEntity item)` with no suppression beside it. Either the
   suppression had not been written when this entry was filed or it took a form the search did not match.
   Whoever closes this entry should grep EFTools for a `CS8766` suppression and remove whatever is actually
   there, rather than trusting this description of it.

### What the pass actually has to decide — read this before estimating it

The public surface is small: **roughly 25 members across nine files**, and `IBaseEntity` is a marker with no
members while `IBaseSoftEntity`'s three properties are `DateTime`/`DateTime?` and need no decision at all. The
volume is not the risk. **Four of the decisions are contract judgements rather than annotation mechanics**, and
only the first was named when the request was filed:

| # | Decision | Why it is not mechanical |
| --- | --- | --- |
| a | `IBaseDao<T>.Get` returns `T?` | The one already decided. Safe for implementers — see below |
| b | The `item` **parameter** on `GetAll`, `GetPaged` and `GetCount` | Their own `<remarks>` say the dispatcher invokes them *"with a literal `null`"* and that an implementation **must never read** the parameter. The truthful annotation is therefore `T? item` — and parameter nullability runs the **opposite way** from return nullability |
| c | The `IList<T>` returns on `GetAll` and `GetPaged` | The current docs say an empty collection is expected but that a `null` result *"is not intercepted and is forwarded to the caller untouched."* `IList<T>` tightens the contract into a real promise; `IList<T>?` forces every caller to null-check something the docs already call unexpected. That is a contract change either way, not an annotation |
| d | `IBaseIdEntity<T>.Id` and `IBaseDataAccess.Get<TEntityType>(object id)` | A reference-type identifier is legitimately `null` before insert, and `BaseDataAccessNullArgumentTests` proves `null` is a **legal argument** to `Get<T>(object id)` for reference-type and nullable-value-type identifiers. `object? id` follows from a passing test, not from taste |

**Decision (b) is the one that undermines the "harmless" framing, and it is worth being blunt about.**
Annotating a *return* as nullable is safe for existing implementers — an implementation that returns non-null
is stricter than the interface promises, and stricter is allowed silently. Annotating a *parameter* as
nullable is not: an implementation declaring `T item` where the interface now says `T? item` is stricter than
the interface **allows**, and stricter in that direction produces **`CS8767`** on every nullable-enabled
implementation, `ProphetsWay.EFTools`' own new Data Access Objects among them. Doing (b) truthfully therefore
*relocates* the warning noise this entry exists to remove rather than eliminating it. The alternatives are to
annotate the parameter non-nullable and accept that the interface is lying in the other direction, or to
accept the `CS8767` wave and fix it in EFTools and Example in the same coordinated pass. **Do not discover
this at implementation time.**

### One technical thing to confirm first

Plain `?` annotations need no help on `netstandard2.0`: `NullableAttribute` and `NullableContextAttribute` are
synthesized by the compiler when the target framework does not supply them. The flow-analysis attributes are a
different matter — `MaybeNull`, `NotNullWhen`, `AllowNull` and `DisallowNull` live in
`System.Diagnostics.CodeAnalysis` and are **not in `netstandard2.0`'s reference assemblies**, so any use of
them means hand-declaring `internal` copies in this library. Confirm the exact set before relying on it, and
prefer a design that needs none of them — that is also why raising `<LangVersion>` to 9 and writing `T?` beats
the C# 8-era `[return: MaybeNull]` idiom, which would need exactly such a polyfill.

### Relationship to the other entries

- **Entry 9** also targets v3.2.0 and also concerns `Get`, but it is about an exception **message** at runtime.
  The two are independent and can land in the same release without interacting.
- **Entry 4** (async) and **entry 6** (the transaction split) are orthogonal. Annotating now does not make
  either harder, and neither is blocked on it.
- **Entry 1** (a conformance kit) gains slightly from this: an annotated contract is a clearer statement of
  what a kit would be checking.
