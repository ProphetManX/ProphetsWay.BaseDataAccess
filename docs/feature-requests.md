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
| 1 | [A published conformance kit](#1--a-published-conformance-kit) | **Proposed** — wants design work |
| 2 | [Nested transactions and savepoints](#2--nested-transactions-and-savepoints) | Deferred — out of scope by decision |
| 3 | [A general thread-safety contract](#3--a-general-thread-safety-contract) | Deferred — deliberately unspecified |
| 4 | [Async members and `IAsyncDisposable`](#4--async-members-and-iasyncdisposable) | Deferred — breaking when taken up |
| 5 | [A standalone count capability](#5--a-standalone-count-capability) | **Rejected** — not deferred |
| 6 | [Splitting transactions into `IBaseTransactionalDataAccess`](#6--splitting-transactions-into-ibasetransactionaldataaccess) | Scheduled for a possible v4 |
| 7 | [Making a swallowed rollback failure observable](#7--making-a-swallowed-rollback-failure-observable) | Open question |

---

## 1 — A published conformance kit

**Status:** Proposed. The problem is settled; the shape of the answer is not. This is the entry that wants
input.

### The problem

**Every rule on [`IBaseDataAccess`](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs) is an obligation on an
implementer that nothing currently checks.** The interface specifies disposal semantics, transaction
semantics, and — for anyone deriving from `BaseDataAccess` — a reflection-dispatch convention. It then trusts
every Data Access Layer to have read the documentation and honoured it.

`ProphetsWay.BaseDataAccess` cannot close that gap itself. `BaseDataAccess` declares `Dispose` and all three
transaction members `abstract` and holds no connection, context, or transaction state of its own. It has
nothing to enforce the rules *with*. It can only require that an implementer decide.

The 32 tests in `ConformingDataAccessDisposalTests` and `ConformingDataAccessTransactionTests` are worth
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

The existing 32 tests are the natural first draft of its contents. They already correspond one-to-one with
the rules on the interface:

| Area | Behaviour proved |
| --- | --- |
| Disposal | `Dispose` is idempotent; a second call is a no-op and never throws |
| Disposal | Every member throws `ObjectDisposedException` once disposed |
| Disposal | A transaction still open at disposal is **rolled back**, never committed |
| Disposal | Already-committed work survives disposal |
| Disposal | A rollback that fails during disposal does not propagate, and the instance is still disposed |
| Disposal | Use-after-dispose and transaction misuse are catchable by one `InvalidOperationException` handler |
| Transactions | `TransactionStart` throws when a transaction is already open — transactions do not nest |
| Transactions | `TransactionCommit` and `TransactionRollBack` throw when nothing is open, including on a second call |
| Transactions | A failed commit leaves **no** transaction open, and a new one can be started afterwards |
| Transactions | Writes inside a committed transaction persist; writes inside a rolled-back or failed one do not |
| Transactions | A write with no transaction open auto-commits on its own |
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

3. **Which target frameworks must it reach?** The library targets `netstandard2.0;net48;net8.0;net9.0`.
   `netstandard2.0` is not a runnable test target, so the kit plausibly needs a narrower list — but the list
   has to cover every framework an implementer might actually run their suite on.

4. **One kit, or one per capability?** The DAO interfaces compose by capability on purpose:
   `IBaseGetAllDao<T>` and `IBasePagedDao<T>` are siblings, and an entity declares only what it supports. A
   single monolithic kit would contradict that design language by demanding every implementer prove
   behaviour they never claimed. A per-capability set of kits matches the library's own grain — at the cost
   of more moving parts and a more complicated first-run experience.

5. **`ProphetsWay.EFTools` and `ProphetsWay.Example` are the first two consumers and the natural proving
   ground.** They are also the honest test of whether the idea works: EFTools is database-backed and
   Example's `NoDB` implementation is in-memory, so between them they exercise exactly the tension in
   question 2. If the kit cannot serve both cleanly, it is not ready to publish. Note also that Example's
   `NoDB` implementation currently throws `NotImplementedException` from its transaction members — see
   entry 6 — so what "conforming" means for a non-transactional store has to be settled alongside this.

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

The `netstandard2.0` target makes `IAsyncDisposable` awkward today, which is a practical reason to wait but
not the reason for the decision. The reason is the half-feature problem.

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

## 6 — Splitting transactions into `IBaseTransactionalDataAccess`

**Status:** Scheduled for a possible v4. **Already recorded in full** as refinement 1 in
[purpose-and-scope.md](purpose-and-scope.md) — read it there. It is the strongest finding in that document,
with the evidence, the proposed shape, and the coordination cost across EFTools and both copies of Example.

Not restated here. One thing has changed since it was written, and this entry exists to record it:

**Disposal now sits on the base interface while the transaction members might later move out.**
`IBaseDataAccess` extends `IDisposable` and carries documented disposal semantics — including *"a transaction
still open at disposal is rolled back"*, a rule that mentions transactions from the base interface. If the
transaction members move to a narrower `IBaseTransactionalDataAccess`, whoever executes that split has to
decide where disposal belongs and what the base interface's disposal rules say once transactions are no
longer guaranteed to be there.

That is a decision, not an obstacle, but it is one more thing the split has to answer — and it did not exist
when the split was first proposed.

---

## 7 — Making a swallowed rollback failure observable

**Status:** Open question. A real hole in verifiability, with a real cost to closing it.

The contract says `Dispose` never throws, and that a rollback failing during disposal is swallowed — an
implementation *may* log it, but must not propagate it. The reasoning is sound: throwing from `Dispose` masks
any in-flight exception inside a `using` block, turning a diagnosable failure into a confusing one.

The consequence is that **"attempted the rollback and swallowed the failure" and "never attempted the
rollback at all" are indistinguishable to a caller.** Both look like a silent `Dispose`. The rule is
therefore stated but **not verifiable from outside the implementation** — which also means a conformance kit
(entry 1) cannot check it, since a kit only sees what a caller sees.

One way to close it: require implementations to report a swallowed failure to a sink supplied by the
consumer — a callback, a diagnostic listener, something along those lines. That makes the behaviour
observable without reintroducing a throwing `Dispose`, and would make the rule checkable.

The cost is a **new API surface** on an interface whose whole appeal is being small, plus a decision about
whether the sink is required or optional, and what an implementation does when none is supplied. That
tradeoff has not been made. It is recorded here because the verifiability hole is real and should be
weighed alongside entry 1 rather than discovered during it.
