# Repo Profile — ProphetsWay.BaseDataAccess

_Generated 2026-08-13 against branch `net-10-update` (PR #39). **Amended 2026-08-23 for v3.2.0**, which
has since shipped and is live on nuget.org. Evidence-based; every claim cites a source file._

> **Amended 2026-08-23.** The previous edition described a tree that no longer exists in three places,
> each corrected below against the file that was opened rather than affirmed:
>
> - **"`LangVersion` is not set in either project"** — false. The library pins
>   `<LangVersion>9.0</LangVersion>` and sets `<Nullable>annotations</Nullable>`.
> - **"`PublishRepositoryUrl` / `EmbedUntrackedSources` / SourceLink — absent"** and **"`IncludeSymbols`
>   / `SymbolPackageFormat` — absent"** — false. All four are present, plus a `TF_BUILD`-conditioned
>   `ContinuousIntegrationBuild`. The *proposed* XML this document carried has landed, **in a different
>   form**: no `PackageReference` was needed or added.
> - **"Gap 2 — No Source Link or symbol package"** — closed. [FR 8](feature-requests.md) is
>   `Done — 2026-08-23`.
>
> **The 116-test figure and the eleven-source-file figure both survived re-derivation** and are stated
> below with the method used. `app-variables.yml` reads **3.2.0**.

This profile is **factual and structural** — API inventory, packaging, target frameworks, test coverage.
Purpose, scope, cohesion, and extraction analysis live in [docs/purpose-and-scope.md](purpose-and-scope.md);
deferred decisions live in [docs/feature-requests.md](feature-requests.md). Neither is re-litigated here.

## One-Line Purpose

A storage-neutral contract vocabulary for a Data Access Layer — entity markers, capability-composed DAO
interfaces, and one aggregate DAL interface — plus an optional reflection dispatcher that lets business logic
call those contracts generically.

## What It Actually Does

The package ships **eleven source files** and **zero package references**. Ten of the eleven are public
contract types plus one exception; the eleventh is the internal reflection engine. There is no conditional
compilation anywhere in the library — no `#if` appears in any file
([BaseDataAccessHelper.cs](../ProphetsWay.BaseDataAccess/BaseDataAccessHelper.cs) is the only file that even
mentions a framework, and it does so in a comment).

Two distinct things are in the box, and a consumer may take either or both:

1. **The vocabulary.** Marker and shape interfaces for entities, DAO interfaces composed by retrieval
   capability, and `IBaseDataAccess` — the aggregate contract business logic depends on. None of these
   reference each other at runtime; they are tied together by the consumer's usage.
   ([IBaseEntity.cs](../ProphetsWay.BaseDataAccess/IBaseEntity.cs),
   [IBaseDao.cs](../ProphetsWay.BaseDataAccess/IBaseDao.cs),
   [IBaseDataAccess.cs](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs))
2. **The optional dispatcher.** `BaseDataAccess` is an abstract class implementing the seven generic members
   of `IBaseDataAccess` by locating an exact public instance method on the derived DAL through reflection.
   It holds **no fields and no state** — `Dispose`, `TransactionStart`, `TransactionCommit` and
   `TransactionRollBack` are all declared `abstract`
   ([BaseDataAccess.cs](../ProphetsWay.BaseDataAccess/BaseDataAccess.cs) lines 97–112).

**The authoritative specifications are the XML `<remarks>`, not this document.** Disposal, transaction and
threading rules are specified on `IBaseDataAccess`
([IBaseDataAccess.cs](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs) lines 6–162, organized as
TRANSACTIONS / DISPOSAL / THREADING / CONVENTION-BASED DISPATCH / DELIBERATE OMISSIONS). The reflection
convention — required method names and signatures, required visibility, required declared return types, and
identifier resolution — is specified on `DataAccessConventionException`
([DataAccessConventionException.cs](../ProphetsWay.BaseDataAccess/DataAccessConventionException.cs) lines
6–135). Both are cited here rather than paraphrased.

## Projects in the Solution

Two projects, plus a Solution Items folder ([ProphetsWay.BaseDataAccess.sln](../ProphetsWay.BaseDataAccess.sln)).

| Project | Type | Role |
| --- | --- | --- |
| `ProphetsWay.BaseDataAccess` | Library, packable | The deliverable — 11 source files |
| `ProphetsWay.BaseDataAccess.Tests` | xUnit test project | 116 test cases; `IsPackable=false`, `IsTestProject=true` |
| Solution Items | folder | `app-variables.yml`, `CHANGELOG.md`, `LICENSE`, `local-pipeline.yml` |

There is no example project here — `ProphetsWay.Example` fills that role in its own repo.

**Minor:** the Solution Items folder omits `README.md`, `AGENTS.md` and `profile.png`, all of which are
repo-root files that are packed or read regularly. Cosmetic; it only affects Visual Studio's tree.

## Public API Surface

Everything below is `public` unless marked. `RootNamespace` and every declared namespace is
`ProphetsWay.BaseDataAccess` ([the csproj](../ProphetsWay.BaseDataAccess/ProphetsWay.BaseDataAccess.csproj)
line 8) — correct for the Data Access family.

### Types

| Type | Kind | Role | Directly tested? |
| --- | --- | --- | --- |
| `IBaseEntity` | interface (marker, no members) | Names "an entity" for every generic constraint in the library | Yes — indirectly; every test entity implements it |
| `IBaseIdEntity<T>` | interface | `T Id { get; set; }`. Shape only — `Get<T>` resolves the identifier **by name** against the public surface, never through this interface | Yes — one characterization test pinning that an **explicit** implementation is not resolvable |
| `IBaseSoftEntity` | interface | `CreatedDate`, `UpdatedDate`, `DeletedDate`. Shape only; no library member reads or assigns them | **No** |
| `IBaseSoftIdEntity<T>` | interface | Composition of the two above; adds no members | **No** |
| `IBaseDao<T>` | interface | `Get(T)`, `Insert(T)`, `Update(T)`, `Delete(T)` | **No** |
| `IBaseGetAllDao<T>` | interface | `IBaseDao<T>` + `GetAll(T)` | **No** |
| `IBasePagedDao<T>` | interface | `IBaseDao<T>` + `GetPaged(T, int, int)` + `GetCount(T)` | **No** |
| `IBaseDataAccess` | interface | The aggregate DAL contract. **Extends `IDisposable`.** 7 generic members + 3 transaction members | Yes — via `BaseDataAccess` and `ConformingDataAccess` |
| `BaseDataAccess` | public abstract class | The optional reflection dispatcher | Yes — heavily |
| `DataAccessConventionException` | public exception (3 ctors) | Reports deterministic wiring errors | Yes |
| `BaseDataAccessHelper` | **`internal` static class** | The whole convention engine — method lookup, return-type validation, identifier resolution, unwrapped invocation | Yes — through `BaseDataAccess` |

`BaseDataAccessHelper` being `internal` is **by design**, not a packaging gap. It is exercised entirely
through the public dispatcher.

### `IBaseDataAccess` members

| Member | Notes |
| --- | --- |
| `IList<T> GetAll<T>()` | Derived method must declare a type assignable to `IList<T>` |
| `IList<T> GetPaged<T>(int skip, int take)` | Same return-type rule |
| `int GetCount<T>()` | Companion to `GetPaged`, not a standalone capability; derived method must declare `int` |
| `TEntityType Get<TEntityType>(object id)` | `object` key so one DAL may mix `int`, `long`, `Guid` |
| `void Insert<TEntityType>(TEntityType item)` | Derived return type unconstrained, including `void`; result discarded |
| `int Update<TEntityType>(TEntityType item)` | Derived method must declare `int` |
| `int Delete<TEntityType>(TEntityType item)` | Derived method must declare `int` |
| `void TransactionStart()` / `TransactionCommit()` / `TransactionRollBack()` | `InvalidOperationException` on misuse |
| `void Dispose()` | Inherited from `IDisposable` |

Every member documents `ObjectDisposedException` once the instance is disposed.

### `BaseDataAccess` members

Seven `public virtual` generic members mirroring the interface, plus four `public abstract` members —
`Dispose()`, `TransactionStart()`, `TransactionCommit()`, `TransactionRollBack()`. The virtual generic
members are overridable, so a derived DAL can opt out of reflection per member; the four abstract members
have no base implementation to inherit at all.

## Dependencies

**The library has zero `PackageReference` entries.** The csproj contains no `ItemGroup` other than the one
packing `CHANGELOG.md`, `README.md` and `profile.png`. `using` directives reach only `System`,
`System.Collections.Generic`, `System.Reflection` and `System.Runtime.ExceptionServices`. That is the whole
dependency graph, and it is what makes the package safe to reference from a contracts project.

Test project ([ProphetsWay.BaseDataAccess.Tests.csproj](../ProphetsWay.BaseDataAccess.Tests/ProphetsWay.BaseDataAccess.Tests.csproj)):

| Package | Version |
| --- | --- |
| `Microsoft.NET.Test.Sdk` | 17.13.0 |
| `xunit` | 2.9.3 |
| `xunit.runner.visualstudio` | 3.0.2 (`PrivateAssets=all`) |
| `Shouldly` | 4.3.0 |
| `coverlet.collector` | 6.0.4 (`PrivateAssets=all`) |

No Moq, no FluentAssertions. House convention satisfied.

## Target Frameworks

| Project | `TargetFrameworks` |
| --- | --- |
| `ProphetsWay.BaseDataAccess` | `netstandard2.0;net10.0` |
| `ProphetsWay.BaseDataAccess.Tests` | `net48;net10.0` |

**The mismatch is deliberate and correct — do not report it as a defect.** Three separate facts support it:

1. `netstandard2.0` is not a runnable test target, so a test project must name runtimes directly.
2. `net48` in the test project is how .NET Framework *behavior* is verified, which is a different thing from
   the library *supporting* .NET Framework. The behavior in question is concrete:
   `Activator.CreateInstance<T>()` wraps a throwing constructor in a `TargetInvocationException` on .NET
   Framework and rethrows it unwrapped on .NET Core, which is the only reason `CreateEntity<T>()` has a catch
   block at all — documented in the `<remarks>` on that method
   ([BaseDataAccessHelper.cs](../ProphetsWay.BaseDataAccess/BaseDataAccessHelper.cs) lines 166–172, which
   state plainly that removing the catch because a modern target never reaches it silently regresses `net48`).
3. Now that the library ships no `net48` asset, the `net48` test leg binds `netstandard2.0` — meaning it
   validates the exact assembly a .NET Framework consumer actually receives rather than a sibling build.

**`LangVersion` is pinned at `9.0` in the library and unset in the test project** — read from
[the csproj](../ProphetsWay.BaseDataAccess/ProphetsWay.BaseDataAccess.csproj) on 2026-08-23. **The
previous edition's "`LangVersion` is not set in either project" is false and must not be restated**, and
neither may the conclusion it drew — that "nullable reference types are absent" because `netstandard2.0`
caps shared code at C# 7.3.

The csproj comments both properties on the line above each:

| Property | Value | Stated reason |
| --- | --- | --- |
| `LangVersion` | `9.0` | "the minimum that allows `T?` on a type parameter constrained only by an interface" |
| `Nullable` | `annotations` | "annotations, not enable: publish nullability to consumers without turning warnings on inside the library" |

**`LangVersion` is independent of the target framework**, and nullable *annotations* need no BCL support
— `NullableAttribute` is emitted into the assembly. So a `netstandard2.0` asset can and does carry them.
What 7.3 would have blocked is a *language feature* requiring newer syntax, which is why the pin exists.

**`annotations` is not a half-finished `enable`.** It publishes nullability to consumers while leaving
the library's own compilation warning-free, which is the correct shape for a contracts package — and it
is what let `ProphetsWay.EFTools` delete ten `CS8766` suppression pairs, because 3.1.0 shipped
null-oblivious and the compiler could not check what those pragmas were silencing.
[FR 10](feature-requests.md) is `Done`; [FR 11](feature-requests.md), which asks for `enable`, is
`Proposed` with **no owner decision taken**.

No end-of-life or redundant TFM is present. **No change recommended.**

## Packaging Audit

**PACKAGING: informational — the package is correctly and completely configured.**

Publication intent is unambiguous: `PostTargetToNuGet: 'yes'` in
[app-variables.yml](../app-variables.yml), a non-empty `<PackageId>`, and a NuGet version badge in the README.

`<Version />`, `<AssemblyVersion />`, `<FileVersion />` and `<InformationalVersion />` are empty **on
purpose** — the `Assembly-Info-NetCore@3` task in `prophets-pipelines/stages/ci-build.yml` supplies all four
from `Major`/`Minor`/`Patch`. Not gaps.

`<PackageReleaseNotes />` is likewise empty **on purpose and must stay present**: the "Extract Release Notes
from CHANGELOG" PowerShell step in `ci-build.yml` does
`$projXml.GetElementsByTagName("PackageReleaseNotes")[0].InnerText = …`, which indexes `[0]` with no null
guard. **Deleting the empty element would fail the build.** Recorded here because it looks exactly like dead
markup to anyone tidying the file.

| Field | State | Value / note |
| --- | --- | --- |
| `PackageId` | **present** | `ProphetsWay.BaseDataAccess` |
| `Product` | **present** | `BaseDataAccess` |
| `Authors` | **present** | `G. Gordon Nasseri` |
| `Company` | **present** | `Prophet's Way` — display form, correct |
| `Description` | **present** | Multi-paragraph; covers both the vocabulary and the optional dispatcher |
| `RepositoryType` | **present** | `git` |
| `RepositoryUrl` | **present** | `https://github.com/ProphetManX/ProphetsWay.BaseDataAccess` |
| `PackageLicenseExpression` | **present** | `MIT`, matching [LICENSE](../LICENSE) |
| `PackageRequireLicenseAcceptance` | **present** | `true` |
| `PackageIcon` | **present** | `profile.png`, paired with `<Content Include="..\profile.png" Pack="true" PackagePath="" />` |
| `PackageReadmeFile` | **present** | `README.md`, paired with `<None Include="..\README.md" Pack="true" PackagePath="" />` |
| `PackageTags` | **present** | `dal data-access data-access-layer dao repository abstraction interfaces decoupling` |
| Packed CHANGELOG | **present** | `<None Include="..\CHANGELOG.md" Pack="true" PackagePath="" />` |
| `PackageReleaseNotes` | empty **by design** | Pipeline-populated; see above |
| `GenerateDocumentationFile` | **present** | `true` — the XML docs, which are the contract, ship in the package |
| `PackageProjectUrl` | **present** | `https://github.com/ProphetManX/ProphetsWay.BaseDataAccess` — previously an empty stub; populated |
| `Copyright` | **present** | `Copyright © Prophet's Way` — previously an empty stub; populated |
| `NeutralLanguage` | **present** | `en-US` — previously empty; populated |
| `PublishRepositoryUrl` / `EmbedUntrackedSources` / SourceLink | **present** | Both properties `true`. **No `PackageReference` — `Microsoft.SourceLink.GitHub` ships inside SDK 10.0.400, with the `buildMultiTargeting\` assets a multi-targeted project needs.** The zero-package-reference property is intact |
| `IncludeSymbols` / `SymbolPackageFormat` | **present** | `true` / `snupkg` |
| `ContinuousIntegrationBuild` | **present, `TF_BUILD`-conditioned** | Its own `PropertyGroup` with `Condition="'$(TF_BUILD)' == 'true'"`, so deterministic paths apply in CI only — unconditionally on, it breaks local debugging |
| `ApplicationIcon` / `Win32Resource` | **empty** | Inert leftovers from the original template |

**Every gap this document previously raised is now closed. Nothing is proposed.** All three landed:
`PackageProjectUrl`, `Copyright`, and — in v3.2.0 — Source Link plus symbols.

### 1 — Source Link + symbols. **CLOSED in v3.2.0**, and not in the shape this document proposed

What is actually in the csproj, read on 2026-08-23:

```xml
<!-- Source Link. Microsoft.SourceLink.GitHub ships inside the SDK, so no PackageReference is needed. -->
<PublishRepositoryUrl>true</PublishRepositoryUrl>
<EmbedUntrackedSources>true</EmbedUntrackedSources>
<IncludeSymbols>true</IncludeSymbols>
<SymbolPackageFormat>snupkg</SymbolPackageFormat>
```

```xml
<!-- Deterministic build paths in CI only; unconditionally on, it breaks local debugging. -->
<PropertyGroup Condition="'$(TF_BUILD)' == 'true'">
	<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>
</PropertyGroup>
```

**The `Microsoft.SourceLink.GitHub` `PackageReference` this document proposed was not added and was not
needed.** It ships inside SDK 10.0.400 at `sdk\10.0.400\Sdks\Microsoft.SourceLink.GitHub\`, carrying both
`build\` and `buildMultiTargeting\`. The tradeoff this document flagged honestly — that the csproj would
stop being dependency-free — **turned out not to exist**, and that is what reversed the owner's decline.
The library still has **zero `PackageReference` entries**. [FR 8](feature-requests.md) is
`Done — 2026-08-23`.

**Symbols publish — and the mechanism is not a push step. Word this carefully.** The shared template's
`NuGetCommand@2` still pushes `$(build.artifactstagingdirectory)/{PackageType}/*.nupkg`, and
**`nuget.exe` 6.4.0 pushes a co-located `.snupkg` of its own accord** without being named in the glob.
Two things in `prophets-pipelines/stages/deploy-release.yml` hold that still: a `NuGetToolInstaller@1`
**pinning the CLI to 6.4.0**, and an artifact download deliberately *not* narrowed to `*.nupkg`. The
evidence is the owner's `Push NuGet Alpha Package` log of **2026-08-23T04:45**, showing an unprompted
`PUT` to `/api/v2/symbolpackage/` returning **`Created`** for `3.2.0-504.Alpha`. **A push step was
written (`63dd56b`) and substantially reverted the same day (`6110d69`).** Do not go looking for it and
do not restore it.

### 2 — `PackageProjectUrl`. **CLOSED.**

### 3 — `Copyright`. **CLOSED.**

## Test Suite

**116 test cases** across 15 test classes — **re-derived 2026-08-23 by grepping every xUnit attribute in
the project, not carried**: 115 attributes, of which **114 are `[Fact]`** and one is a **`[Theory]` with
two `[InlineData]` rows**
([BaseDataAccessIdentifierRejectionTests.cs](../ProphetsWay.BaseDataAccess.Tests/BaseDataAccessIdentifierRejectionTests.cs)
line 54). There is **no `[MemberData]`, no `[ClassData]` and no `[Trait]` anywhere in the project**, so
there is no scope partition here and no filtered conformance gate — unlike `ProphetsWay.Example`. Two TFMs
× 116 = **232 executions**. [CHANGELOG.md](../CHANGELOG.md) records 116 under **both** `v3.1.0` and
`v3.2.0`, which matches.

xUnit + Shouldly, tabs, Allman, `//setup` / `//act` / `//assert` comment structure throughout. Test class
names describe the behavior area rather than mirroring a single type — a deliberate departure from
"`HasherTests` mirrors `Hasher`" that suits a library with one dispatcher and many failure modes.

Twenty-two supporting files supply purpose-built DALs and entities (`WellFormedDataAccess`,
`ConformingDataAccess`, `ShadowingDataAccess`, `StructEntityDataAccess`, `TestEntities`, and others).

| Area | Class | Cases |
| --- | --- | --- |
| Happy-path dispatch, all seven members | `BaseDataAccessDispatchTests` | 8 |
| Method lookup — arity, parameter types, visibility, `static`, base-typed parameters | `BaseDataAccessMethodLookupTests` | 17 |
| Declared return-type validation, checked before invocation | `BaseDataAccessReturnTypeTests` | 10 |
| Disposal contract | `ConformingDataAccessDisposalTests` | 20 |
| Transaction contract | `ConformingDataAccessTransactionTests` | 15 |
| Unwrapped exception propagation (incl. the `net48` constructor path) | `BaseDataAccessExceptionPropagationTests` | 9 |
| Exception message type rendering (`IList<Company>`, not backtick-arity) | `BaseDataAccessTypeRenderingTests` | 8 |
| `null` argument handling | `BaseDataAccessNullArgumentTests` | 6 |
| Identifier resolution — `{TypeName}Id` before `Id`, and public-only visibility | `BaseDataAccessIdResolutionTests` | 5 |
| Identifier assignment, incl. non-public setters | `BaseDataAccessIdentifierAssignmentTests` | 4 |
| Identifier rejection — `ArgumentException` vs. convention error | `BaseDataAccessIdentifierRejectionTests` | 4 |
| Entity with no identifier property | `BaseDataAccessEntityWithoutIdPropertyTests` | 3 |
| `null` results forwarded untouched | `BaseDataAccessNullResultTests` | 3 |
| `struct` entities | `BaseDataAccessStructEntityTests` | 3 |
| `new`-shadowed methods resolve to the most derived | `BaseDataAccessShadowedMethodTests` | 1 |

### What is covered

`BaseDataAccess` and `BaseDataAccessHelper` are covered exhaustively — every branch of the convention has at
least one test, and the ordering guarantees (return type validated before invocation, identifier property
validated before the probe entity is constructed) are pinned by tests asserting that the derived method was
**never called**.

The 35 disposal and transaction tests are worth being precise about, and
[feature-requests.md](feature-requests.md#1--a-published-conformance-kit) already is: their subject is
`ConformingDataAccess`, a hand-written implementation in the test project, **not** `BaseDataAccess`. They
prove a correct implementation is expressible and pin the contract's meaning. They prove nothing about any
other implementation.

### What is not covered

**Verified by search: no file under `ProphetsWay.BaseDataAccess.Tests/` references `IBaseDao`,
`IBaseGetAllDao`, `IBasePagedDao`, `IBaseSoftEntity` or `IBaseSoftIdEntity`.** Six of the eleven public types
therefore have **zero** direct test coverage:

| Untested type | Consequence |
| --- | --- |
| `IBaseDao<T>`, `IBaseGetAllDao<T>`, `IBasePagedDao<T>` | Pure contracts with no library-side behavior — a test could only assert the inheritance graph. Low real risk, but the graph is load-bearing for consumers, and an accidental change to it compiles clean here and breaks EFTools |
| `IBaseSoftEntity`, `IBaseSoftIdEntity<T>` | Same — shape-only, no runtime role in this library by design |

Every test entity in [TestEntities.cs](../ProphetsWay.BaseDataAccess.Tests/TestEntities.cs) implements
`IBaseEntity` and nothing else, with one deliberate exception — `Wraith`, which implements
`IBaseIdEntity<int>` **explicitly** for the characterization test below. That the identifier-resolution
entities carry no such interface is itself the strongest available evidence for the documented claim that
`Get<T>` does not key on `IBaseIdEntity<T>`.

One specific case recorded as refinement 5 in [purpose-and-scope.md](purpose-and-scope.md) — an entity
implementing `IBaseIdEntity<T>` **explicitly**, where the property becomes private and interface-prefixed so
neither `{TypeName}Id` nor `Id` matches — is **now verified**, by `Wraith` in `TestEntities.cs` and
`ShouldThrowConventionExceptionWhenTheEntityImplementsItsIdentifierExplicitly` in
[BaseDataAccessIdResolutionTests.cs](../ProphetsWay.BaseDataAccess.Tests/BaseDataAccessIdResolutionTests.cs).
The rule is also now stated in the `<remarks>` on `IBaseIdEntity<T>`, `DataAccessConventionException`,
`IBaseDataAccess.Get` and `BaseDataAccess.Get`.

## Real Usage Examples Found

There is no example project in this repo, so runnable usage lives elsewhere. Two genuinely real snippets are
already quoted in [README.md](../README.md) from `ProphetsWay.Example` — `IExampleDataAccess` composing
`IBaseDataAccess` with five DAO interfaces, and `ICompanyDao` extending `IBasePagedDao<Company>` with a custom
method. Everything else in the README is explicitly and correctly labeled **"Illustrative — not currently
present in the repo."**

The test project's `ConformingDataAccess` and `WellFormedDataAccess` are the most complete in-repo
implementations of the contract and are the best reference for anyone writing a DAL, though they are test
fixtures rather than samples.

## README Accuracy Check

| Existing claim | Verdict | Evidence |
| --- | --- | --- |
| "Targets: .NET Standard 2.0 and .NET 10.0." | **Accurate — previously stale, now corrected** | [README.md](../README.md) line 38 matches `netstandard2.0;net10.0` in the csproj. The line formerly read ".NET Standard 2.0, .NET Framework 4.8, .NET 8.0, and .NET 9.0"; re-verified 2026-08-16 by opening the file |
| Entity, DAO and aggregate API reference tables | Accurate | Verified member-by-member against all 11 source files |
| Convention table — required method, required declared return type | Accurate | Matches `DataAccessConventionException` `<remarks>` and `BaseDataAccessHelper.GetMethodByNameForType` |
| Inheritance diagram (`All --> Dao`, `Paged --> Dao`, `SoftId --> Id`, `SoftId --> Soft`) | Accurate | Matches the declarations |
| Transaction rules table (7 rows) | Accurate | Matches the TRANSACTIONS section of `IBaseDataAccess` `<remarks>` |
| Disposal rules (4 bullets) | Accurate | Matches the DISPOSAL section |
| Setter "need not be public"; only total absence of a setter fails | Accurate | `!prop.CanWrite` check in `GetMethodFindAndSetIdPropertyAndInvoke`, and pinned by `Sprocket` in `TestEntities.cs` |
| `Get<T>(null)` throws for a non-nullable value-type identifier | Accurate | `SetIdentifier` guard; pinned by `BaseDataAccessIdentifierRejectionTests` |
| Value-type entity cannot express "not found" as `null` | Accurate | Matches both `IBaseDataAccess.Get` and `BaseDataAccess.Get` `<remarks>` |
| Build badge `definitionId=23` | Not verifiable from source; pre-existing | Copied from the repo as required — not invented here |

**The one stale line has since been fixed.** No README defect remains as of the 2026-08-16 re-check.

## CHANGELOG Accuracy Check — one measurably false claim, 2026-08-23

`CHANGELOG.md` is owned by `Changelog Author`; **nothing in this pass edited it.** Its newest heading is
**`v3.2.0`**, matching `app-variables.yml`.

| Existing claim | Verdict | Evidence |
| --- | --- | --- |
| "Source Link and symbol packages are on: `PublishRepositoryUrl`, `EmbedUntrackedSources`, `IncludeSymbols`, and `SymbolPackageFormat` set to `snupkg`" | **Accurate** | All four read from the csproj |
| "**no package reference was added** — Source Link ships inside the SDK — so this package still has no package dependencies at all" | **Accurate** | The csproj has no `PackageReference` at all |
| "All 116 tests pass on both `net48` and `net10.0`" | **Accurate — count re-derived** | 114 `[Fact]` + 2 `[InlineData]` = 116 |
| **"One honest caveat: the `.snupkg` is produced but is not yet pushed to the feed. The publish step in the shared pipeline still pushes `*.nupkg` only, so do not expect symbols to be available for download…"** (lines 109–111) | **FALSE as of 2026-08-23** | The glob observation is still true and **the conclusion drawn from it is wrong**. `nuget.exe` 6.4.0 pushes the co-located `.snupkg` unprompted; the owner's `Push NuGet Alpha Package` log of 2026-08-23T04:45 shows a `PUT` to `/api/v2/symbolpackage/` returning **`Created`** for `3.2.0-504.Alpha`. `prophets-pipelines` pins the CLI to 6.4.0 precisely to keep that true. **Symbols *are* on the feed for 3.2.0** |

**That last row is the highest-value item for `Changelog Author` in this repo**, because it is a
published statement that tells a consumer not to look for something that is there.

## Gaps & Observations

Ordered by consequence. None of these is a defect in behavior.

1. ~~**Three documents carry the pre-retarget TFM list.**~~ **CLOSED — re-verified 2026-08-16 by opening all
   three.** [README.md](../README.md) now reads "Targets: .NET Standard 2.0 and .NET 10.0.";
   [docs/purpose-and-scope.md](purpose-and-scope.md) and [docs/feature-requests.md](feature-requests.md) both
   now state `netstandard2.0;net10.0` for the library and `net48;net10.0` for the tests. The AGENTS.md
   per-repo section was corrected earlier. No document in the repo still carries
   `netstandard2.0;net48;net8.0;net9.0`.

2. ~~**No Source Link or symbol package.**~~ **CLOSED in v3.2.0 — verified 2026-08-23 by opening the
   csproj.** All four properties are present, `ContinuousIntegrationBuild` is `TF_BUILD`-conditioned, and
   **no `PackageReference` was added** — Source Link ships inside SDK 10.0.400. Symbols publish by way of
   the `nuget.exe` 6.4.0 pin in `prophets-pipelines`, **not** a `.snupkg` push step; see the Packaging
   Audit. [FR 8](feature-requests.md) is `Done`. Do not re-report this.

3. **Indentation is mixed across the library.** Four files use tabs
   ([IBaseDataAccess.cs](../ProphetsWay.BaseDataAccess/IBaseDataAccess.cs),
   [IBaseDao.cs](../ProphetsWay.BaseDataAccess/IBaseDao.cs),
   [IBaseEntity.cs](../ProphetsWay.BaseDataAccess/IBaseEntity.cs),
   [IBaseIdEntity.cs](../ProphetsWay.BaseDataAccess/IBaseIdEntity.cs)); the other seven use four spaces
   ([BaseDataAccess.cs](../ProphetsWay.BaseDataAccess/BaseDataAccess.cs),
   [BaseDataAccessHelper.cs](../ProphetsWay.BaseDataAccess/BaseDataAccessHelper.cs),
   [DataAccessConventionException.cs](../ProphetsWay.BaseDataAccess/DataAccessConventionException.cs),
   [IBaseGetAllDao.cs](../ProphetsWay.BaseDataAccess/IBaseGetAllDao.cs),
   [IBasePagedDao.cs](../ProphetsWay.BaseDataAccess/IBasePagedDao.cs),
   [IBaseSoftEntity.cs](../ProphetsWay.BaseDataAccess/IBaseSoftEntity.cs),
   [IBaseSoftIdEntity.cs](../ProphetsWay.BaseDataAccess/IBaseSoftIdEntity.cs)). House convention is tabs; both
   csproj files and the entire test project comply. Purely cosmetic, invisible in the package, and the repo
   has no `.editorconfig` to enforce either way — recorded so it is not rediscovered, not proposed as work.

4. **Six of eleven public types have no direct test.** Detailed above. Defensible — they are pure contracts
   — but the DAO inheritance graph is what EFTools and every consumer bind against, and nothing in this repo
   fails if it changes.

5. **The empty `<PackageReleaseNotes />` is a build-critical element that looks like dead markup.** The
   pipeline indexes `GetElementsByTagName("PackageReleaseNotes")[0]` with no guard. **In *this* csproj the
   element is no longer empty** — it points at `CHANGELOG.md` — but the hazard is unchanged for any sibling
   repo where it is still a stub, and the element must stay present here regardless.

6. ~~**`ProphetsWay.EFTools` has not picked up 3.x.**~~ **CLOSED 2026-08-23.** Both
   `ProphetsWay.EFTools.csproj` and `ProphetsWay.Example.DataAccess.EF.csproj` reference
   `ProphetsWay.BaseDataAccess` **3.2.0**, and EFTools' suite runs **270 / 270 / 0** against these
   contracts. Its `0da679f` deleted **ten `CS8766` suppression pairs** made unnecessary by 3.2.0's nullable
   annotations — which is the most concrete evidence available that this release did what it was for. What
   remains is EFTools' own tag and publish, not anything owed by this repo.

7. **A shared-block generalization is now demonstrably wrong, and it is not this repo's to fix.** The
   Target Frameworks section of the generated block states that `netstandard2.0` defaults to C# 7.3 and
   that this "is why nullable reference types do not work in these libraries regardless of what a csproj
   claims." This library ships nullable **annotations** on a `netstandard2.0` asset today. The master copy
   is `prophets-pipelines/conventions/AGENTS.shared.md`, and a change there propagates to seven repos — it
   needs an owner decision and a `/sync-agents-md` run, not a hand edit.

## Open Questions for the Owner

1. ~~**Is adding `Microsoft.SourceLink.GitHub` acceptable?**~~ **ANSWERED and moot.** The owner reversed
   the decline, and the premise turned out to be false: Source Link ships inside SDK 10.0.400, so **no
   `PackageReference` was added** and the zero-dependency property was never at risk. Landed in v3.2.0.

2. **Does the `PackageProjectUrl` / `Copyright` fix belong in the sibling repos?** **Answered for this repo**
   — both are now populated, as is `NeutralLanguage`. AGENTS.md calls this repo's csproj the reference other
   repos should copy, and that is now more true than before: it is the only one in the workspace carrying
   Source Link. `ProphetsWay.EFTools` is the known laggard — its deviation 5 / FR 16 records the same gaps,
   explicitly **not** release-blocking.

3. **Is the DAO inheritance graph worth pinning with a test?** `UNKNOWN — needs owner input`. A handful of
   compile-time assertions (`typeof(IBasePagedDao<Company>).ShouldBeAssignableTo<IBaseDao<Company>>()`) would
   catch an accidental change here rather than in EFTools. It may equally be judged as testing the compiler.

4. **Should the `<PackageReleaseNotes />` dependency be hardened in the pipeline?** It is populated here now,
   so this repo is safe either way — but the pipeline fix (an `if` guard before the index) protects all seven
   repos, and some of them still carry the empty stub.

5. **Does `<Nullable>enable</Nullable>` follow?** [FR 11](feature-requests.md) is `Proposed` with **no owner
   decision taken**. `annotations` is a complete and correct end state for a contracts package; `enable`
   would be a further step, not a completion of this one. Do not treat the current state as half-finished.

6. **Who corrects the shared block's nullable generalization?** See gap 7. It needs an owner decision and a
   `/sync-agents-md` run — no agent may hand-edit a generated block.
