# AGENTS.md — ProphetsWay.BaseDataAccess

<!-- ═══════════════════════════════════════════════════════════════════════
     BEGIN SHARED BLOCK
     Generated from prophets-pipelines/conventions/AGENTS.shared.md
     DO NOT EDIT BY HAND — use pw-suite-maintenance to synchronize.
     ═══════════════════════════════════════════════════════════════════════ -->

## About This Codebase

`ProphetsWay.*` is a family of small, focused .NET libraries by G. Gordon Nasseri, published to
NuGet under the `ProphetsWay.` prefix and hosted at `github.com/ProphetManX`. Each library lives
in its own repository with its own version line, changelog, and pipeline.

### The Two Families

| Family | Repos | Purpose |
|---|---|---|
| **Utility** | Utilities, Logger, Hasher | Standalone helpers with no dependency on each other |
| **Data Access** | BaseDataAccess, EFTools | A layered DAL-decoupling paradigm; EFTools implements BaseDataAccess |

`ProphetsWay.Example` is a reference implementation, not a published package.
`prophets-pipelines` holds shared Azure DevOps YAML templates and this conventions file.

## Naming

**Display vs. codified.** The organization name is written two ways, and the distinction matters:

- **Display name** — `Prophet's Way`, with the apostrophe. Used in `<Company>`, prose, README text, and anything a human reads.
- **Codified** — `ProphetsWay`, no apostrophe or space. Used in namespaces, package IDs, assembly names, repo names, and the Azure DevOps org.

| Thing | Rule | Example |
|---|---|---|
| Repository | `ProphetsWay.<Library>` | `ProphetsWay.Logger` |
| Package ID | matches repository | `ProphetsWay.Logger` |
| Assembly name | matches repository | `ProphetsWay.Logger` |
| Library project folder | matches repository | `ProphetsWay.Logger/` |
| Test project | `<Library>.Tests` — **plural** | `ProphetsWay.Logger.Tests` |
| Example project | `<Library>.Example` | `ProphetsWay.Logger.Example` |
| `<Company>` | display name | `Prophet's Way` |
| `<Authors>` | `G. Gordon Nasseri` | |
| `<Product>` | library name without prefix | `Logger` |

### Namespaces

The rule is **family-dependent**. Do not "correct" one family to match the other.

- **Utility family** shares one root namespace regardless of assembly name:
  `ProphetsWay.Utilities`, with sub-namespaces for areas (`ProphetsWay.Utilities.LoggerDestinations`).
  A consumer adds one `using ProphetsWay.Utilities;` and reaches every utility library.
  This is why `ProphetsWay.Logger.dll` declares `namespace ProphetsWay.Utilities` — intentional, not a bug.
- **Data Access family** uses per-library namespaces: `ProphetsWay.BaseDataAccess`, `ProphetsWay.EFTools`
  (plus key-type sub-namespaces `.Guid`, `.Int`, `.Long`). These are an architectural paradigm, not
  utilities, and are kept separately addressable.
- **Test projects always use their own namespace**, `<AssemblyName>.Tests` — never the shared root.

## Target Frameworks

```xml
<!-- default for a published library -->
<TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>

<!-- only when a framework-conditional dependency or API requires it -->
<TargetFrameworks>netstandard2.0;net48;net10.0</TargetFrameworks>

<!-- test projects — netstandard2.0 is not a valid test target -->
<TargetFrameworks>net48;net10.0</TargetFrameworks>
```

.NET ships every November: **even-numbered = LTS (3 years), odd-numbered = STS (18 months)**.
.NET 10 is the current LTS (Nov 2025 → ~Nov 2028). **.NET 8 and .NET 9 both go end of life on
10 November 2026** — `net8.0`/`net9.0` are now debt, as are `netcoreapp*`, `net5.0`–`net7.0`,
and anything below `net48`.

1. **LTS only.** Never target an STS release in a published library — an 18-month window means
   re-cutting the list every year and stranding someone each time. Never target a preview.
2. **`netstandard2.0` is permanent.** It is an API contract, not a runtime, so it cannot expire.
   It is the reach floor: consumable by .NET Framework 4.6.1+ (painless from 4.7.2 up) and by every
   .NET Core/5+ runtime. It is also the last .NET Standard version Framework supports —
   `netstandard2.1` deliberately excluded it.
3. **`net48` is conditional, not default.** `netstandard2.0` already reaches .NET Framework 4.8, so
   an explicit `net48` target earns its place only when the repo has a framework-conditional
   *dependency* or needs an API `netstandard2.0` does not expose. `ProphetsWay.EFTools` qualifies —
   its EF6 branch is keyed on `net4*`. Most repos do not. Justify it per repo.
   (.NET Framework 4.8 is the final Framework version; it ships as a Windows component and inherits
   the OS lifecycle, so it has no standalone EOL date.)
4. **Carry exactly one modern TFM** unless something concrete requires two. Every extra target
   multiplies build time.
5. **Test projects name runtimes directly**, since `netstandard2.0` cannot be a test target. A
   `net48` test target is how .NET Framework behavior is *verified*, which is distinct from a
   library merely *supporting* it: `Activator.CreateInstance<T>()` wraps a throwing constructor on
   .NET Framework and does not on .NET Core, so `ProphetsWay.Example.Tests` must keep `net48` or its
   exception-passthrough regression guard stops guarding anything.
6. **Canonical dotted monikers** — `net10.0`, never `net100`. The undotted form parses, but it is
   non-standard and inconsistent across the repos.
7. **`LangVersion`:** `netstandard2.0` defaults to C# 7.3, and that constraint applies to all shared
   code in a multi-targeted project. This is why nullable reference types do not work in these
   libraries regardless of what a csproj claims.
8. **Dropping `net8.0`/`net9.0` is not a breaking change** while `netstandard2.0` remains — those
   consumers still install and still resolve an asset.
9. **Adding a TFM is a MINOR bump, never a patch.** A new target silently repoints existing
   consumers to a *different assembly* — a .NET 10 consumer that resolved the `netstandard2.0` asset
   starts binding the `net10.0` one, a different compilation with different BCL bindings and no
   netstandard shims. A patch must be safe to take without reading the notes.

## Packaging Metadata

Required in every **published** library's `.csproj`. If a repo is not published to NuGet, these
are optional — but they become mandatory the moment publishing is on the table.

```xml
<PackageId>ProphetsWay.Thing</PackageId>
<Product>Thing</Product>
<Authors>G. Gordon Nasseri</Authors>
<Company>Prophet's Way</Company>
<Description>...</Description>
<RepositoryType>git</RepositoryType>
<RepositoryUrl>https://github.com/ProphetManX/ProphetsWay.Thing</RepositoryUrl>
<PackageLicenseExpression>MIT</PackageLicenseExpression>
<PackageRequireLicenseAcceptance>true</PackageRequireLicenseAcceptance>
<PackageIcon>profile.png</PackageIcon>
<PackageReadmeFile>README.md</PackageReadmeFile>
<PackageTags>...</PackageTags>
```

Paired with the item group that actually packs those files — declaring `PackageIcon` or
`PackageReadmeFile` without the matching `<None Pack="true">` packs nothing and fails the build:

```xml
<ItemGroup>
  <None Include="..\CHANGELOG.md" Link="CHANGELOG.md" Pack="true" PackagePath="" />
  <None Include="..\README.md" Link="README.md" Pack="true" PackagePath="" />
  <Content Include="..\profile.png" Link="profile.png" Pack="true" PackagePath="" />
</ItemGroup>
```

**An empty self-closing element is not a value.** `<PackageId />` silently falls back to
`AssemblyName` and leaves the nuget.org listing without a license, readme, or source link.
Treat empty stubs as missing.

Versioning is owned by the pipeline. Leave `<Version />`, `<AssemblyVersion />`,
`<FileVersion />`, and `<InformationalVersion />` empty in the csproj — `app-variables.yml`
supplies them at build time.

## Testing

- **xUnit** — the test framework. Do not introduce NUnit or MSTest.
- **Shouldly** — assertion style. Prefer `result.ShouldBe(...)` over `Assert.Equal`.
  FluentAssertions 8.x requires a paid commercial license; do not add it to any project.
- **coverlet.collector** — coverage.
- **Moq** — only where a test genuinely needs a mock; most of these libraries do not.
- Test class names mirror the type under test: `HasherTests`, `FileDestinationTests`.
- Tests requiring a local database set `LocalTestsOnly: 'yes'` in `app-variables.yml` so CI skips them.

## Pipelines

Every repo consumes the shared templates in `prophets-pipelines` via two root files:

| File | Purpose |
|---|---|
| `app-variables.yml` | Per-repo values — `Major`/`Minor`/`Patch`, `TargetProject`, `Product`, `RepoName`, `PostTargetToNuGet`, `LocalTestsOnly` |
| `local-pipeline.yml` | Thin wrapper pulling `prophets-pipelines` stage templates |

`Major`/`Minor`/`Patch` are bumped **by hand** in `app-variables.yml` as work proceeds. The
pipeline appends build metadata to produce alpha/beta/release packages.

## Repo Layout

```
ProphetsWay.Thing/
├─ AGENTS.md                 ← this file
├─ README.md                 ← packed into the nupkg
├─ CHANGELOG.md              ← packed into the nupkg
├─ LICENSE                   ← MIT
├─ profile.png               ← NuGet icon
├─ app-variables.yml
├─ local-pipeline.yml
├─ ProphetsWay.Thing.sln
├─ ProphetsWay.Thing/        ← library
├─ ProphetsWay.Thing.Tests/  ← xUnit
└─ docs/                     ← agent-generated analysis
  ├─ repo-profile.md
  ├─ purpose-and-scope.md
  ├─ nuget-extraction-proposal.md
  └─ feature-requests.md    ← durable request and decision index
```

These artifacts are generated by agents and committed. `feature-requests.md` becomes applicable once
the first request is captured; an empty repo need not carry a placeholder.

**`docs/architecture.md` and per-project `docs/requirements.md` are `n/a` for a utility or reference
library** — its architecture lives in `AGENTS.md`, the README, and XML `<remarks>`. They apply to
multi-project **application** solutions only. Do not report either as missing from a library repo.

## Solution Layout

For multi-project application solutions, the rule is **base name = contracts, suffix = swappable
implementation**. Business logic has one implementation and needs no split; a DAL has many and does.

| Project | Contains |
|---|---|
| `<Solution>.Core` | Domain models, business logic interfaces, and their implementation |
| `<Solution>.DataAccess` | DAL contracts only — interfaces and entities |
| `<Solution>.DataAccess.<Provider>` | One DAL implementation: `.MSSQL`, `.PostgreSQL`, `.MySQL`, `.NoDB`, `.EF` |
| `<Solution>.Database` | The `.sqlproj` database project |
| `<Solution>.Api` | Service endpoints |
| `<Solution>.Web` | Web UI |
| `<Solution>.Win` | Desktop UI |
| `<Project>.Tests` | xUnit tests for that project — `<Solution>.Core.Tests` |

**The suffix list is open.** A new provider or UI technology gets a new suffix following the same
shape (`.DataAccess.Cosmos`, `.Mobile`, `.Cli`). Do not invent a new *pattern* — extend this one.

A contracts project must never reference an implementation project, and must never expose a type
from a specific technology (`DbContext`, `SqlConnection`, `HttpContext`) in its public surface.
That rule is what makes the DAL swappable, and it is the whole point of the paradigm.

### Database Projects

New `.sqlproj` projects use the **`Microsoft.Build.Sql`** SDK — SDK-style, cross-platform, and
buildable with `dotnet build`:

```xml
<Project Sdk="Microsoft.Build.Sql/<version>">
```

The legacy SSDT format (`ToolsVersion="4.0"`, the 2003 MSBuild namespace, `TargetFrameworkVersion`,
plus `.dbmdl`/`.jfm` sidecar files) requires Visual Studio on Windows and cannot be built by the
.NET CLI. Existing legacy projects are **debt to migrate** — the `.sql` files carry over unchanged;
the project header and sidecars are what change.

## Code Style

- Tabs for indentation in `.csproj` and `.cs`.
- Braces on their own line (Allman).
- Interfaces prefixed `I`. Abstract bases prefixed `Base` or `Root`.
- Public API surface gets XML doc comments; internals do not need them.
- No `.editorconfig` exists yet — style is convention, not enforced. Match surrounding code.

## Rules for Agents

- **Default to read-only without an owner-approved assignment.** Do not edit `.cs`, `.csproj`,
  `.sln`, `.yml`, or other product artifacts merely because a change was suggested.
- **V3 scoped delivery.** An owner-approved assignment may be delegated by `PW - Lead` without
  another human message for every worker turn. Each delegation must carry the approved behavior,
  exact allowed paths, checks, evidence destination and stopping boundary. Invoking a role is not
  blanket permission to change product behavior or perform unapproved operations.

  | Agent | May write |
  |---|---|
  | `PW - Lead` | Assignment state and handoffs; delegates product/test authoring and independent review |
  | `PW - Design` | Requirements, decisions and design documents — never production code or tests |
  | `PW - Investigate` | Assigned diagnostic findings — never a production fix |
  | `PW - Tests` | Approved tests, fixtures and explicitly scoped test-project configuration — never production code |
  | `PW - Build` | Approved production contracts and implementation, including `.cs`, `.sql`, explicitly scoped `.xml`, assigned project/build configuration, examples and docs — **never** tests |
  | `PW - Review` | Independent review findings — never the reviewed artifacts |

  Only the test author may make a justified, independently reviewed correction to a specification.
  An implementer changing a test to make it pass is a workflow failure, not a green build.
  Retired v1/v2 role definitions remain historical reference, not current write authority.
- **Never bump a version** in `app-variables.yml`. That is a human decision.
- **Never invent an Azure DevOps `definitionId`.** Badge URLs must be copied from a file that
  already exists in the repo. If one is missing, ask.
- **Feature requests are shared-capture, single-owner triage.** The owner or any agent may append a
  `Proposed` entry to `docs/feature-requests.md`, but must read the index first and extend an existing
  entry instead of duplicating it. Only `PW - Design` may change triage status, recording the
  owner's decision or an explicitly delegated decision authority. Never delete or renumber
  entries; rejected requests remain with their reasoning, and new numbers increase monotonically.
- **A namespace change is a binary-breaking change.** Never make one casually; it requires a major
  version bump and a CHANGELOG entry.
- **Affirming an inherited claim is not verifying it.** Before restating any existing claim in a
  README, `AGENTS.md`, or doc as still accurate, open the artifact it describes. A claim that has
  survived several passes has been *copied* several times, not *checked* several times. Say which
  file you opened.
- **When a hygiene fix has a teaching cost, genericize rather than delete** — replace the
  machine-specific value, keep the artifact — and record the declined option in the
  `docs/feature-requests.md` entry so it is not re-proposed later as unfinished work.
- Respect the family split above. `ProphetsWay.EFTools` living outside `ProphetsWay.Utilities`
  is correct, not drift.
- Deviations from these conventions are listed per-repo below. They are known, not overlooked —
  do not re-report them as discoveries.

<!-- ═══════════════════════════════════════════════════════════════════════
     END SHARED BLOCK
     ═══════════════════════════════════════════════════════════════════════ -->

---

## This Repo

**Family:** Data Access · **Published:** yes, as `ProphetsWay.BaseDataAccess`

A **storage-neutral contract vocabulary** for a Data Access Layer — entity markers,
capability-composed DAO interfaces, and one aggregate DAL interface — **plus an optional
reflection dispatcher** that lets business logic call those contracts generically. Together they
let a DAL implementation be swapped out wholesale with minimal blast radius.
`ProphetsWay.EFTools` is the Entity Framework implementation of these contracts;
`ProphetsWay.Example` demonstrates consuming them.

**The dispatcher is optional and that optionality is the design.** A consumer may implement
`IBaseDataAccess` directly and never inherit `BaseDataAccess` — inheriting it buys convention-based
dispatch in exchange for reflection. Do not describe this package as interfaces-only.

This is the **root of the Data Access family**. A breaking change here cascades into EFTools and
every consumer. Treat its surface as close to frozen.

### 3.2.0 — shipped 2026-08-23. Two things landed, and both are decisions rather than accidents

`app-variables.yml` reads `Major: '3'` / `Minor: '2'` / `Patch: '0'`, opened 2026-08-23. **Never change
it.** `CHANGELOG.md` opens with a `v3.2.0` entry. Everything below was read from
[ProphetsWay.BaseDataAccess/ProphetsWay.BaseDataAccess.csproj](ProphetsWay.BaseDataAccess/ProphetsWay.BaseDataAccess.csproj)
on that date, not carried.

**1 — Nullable *annotations*, deliberately not `enable`.** The csproj carries
`<Nullable>annotations</Nullable>` and `<LangVersion>9.0</LangVersion>`, each with a comment on the line
above giving its reason: annotations **publish** nullability to consumers without turning warnings on
inside the library, and 9.0 is the minimum language version that allows `T?` on a type parameter
constrained only by an interface. **This is the correct shape for a contracts package** — a consumer
compiling with nullable enabled gets a surface that tells them something, and the library takes on no
internal churn. It is what let `ProphetsWay.EFTools` delete **ten `CS8766` suppression pairs** in its
commit `0da679f`: 3.1.0 shipped null-oblivious, so the compiler could not check what those pragmas were
silencing, and 3.2.0 annotated means it now does. [FR 10](docs/feature-requests.md) is `Done`;
[FR 11](docs/feature-requests.md) — going further to `<Nullable>enable</Nullable>` — is `Proposed` with
**no owner decision taken**. Do not treat `annotations` as a half-finished `enable`.

> **This contradicts a generalization in the shared block, and the shared block is the one that is
> wrong.** The Target Frameworks section states that `netstandard2.0` defaults to C# 7.3 and that this
> "is why nullable reference types do not work in these libraries regardless of what a csproj claims."
> `LangVersion` is independent of the target framework, and nullable **annotations** need no BCL support
> — this library ships them on a `netstandard2.0` asset today. **Do not hand-edit the shared block to
> fix this**; it is generated from `prophets-pipelines/conventions/AGENTS.shared.md` and a change there
> propagates to seven repos. Raise it with the owner and let `/sync-agents-md` carry it.

**2 — Source Link and symbol packages, with no `PackageReference`.** `PublishRepositoryUrl`,
`EmbedUntrackedSources`, `IncludeSymbols` and `SymbolPackageFormat` = `snupkg` are all present with
values, and `ContinuousIntegrationBuild` sits in its own `TF_BUILD`-conditioned `PropertyGroup` so
deterministic paths apply in CI only and local debugging still works.
**`Microsoft.SourceLink.GitHub` ships inside the SDK, so nothing was added** — the library still has
**zero `PackageReference` entries**, which is the property that makes it safe for a contracts project to
depend on. [FR 8](docs/feature-requests.md) is `Done — 2026-08-23`.

**Word the symbol half carefully or you will send a reader looking for code that does not exist.**
Symbols publish, and did publish for `3.2.0-504.Alpha` — **but by way of the `nuget.exe` 6.4.0 pin in
`prophets-pipelines/stages/deploy-release.yml`, not a `.snupkg` push step.** 6.4.0 pushes a co-located
`.snupkg` on the ordinary `.nupkg` push; the measurement is a `PUT` to `/api/v2/symbolpackage/` returning
`Created` in the 2026-08-23T04:45 log. A push step **was** written, in `63dd56b`, and substantially
reverted the same day in `6110d69`. Do not restore it, and do not describe the `*.nupkg` push glob as a
defect — the glob is unchanged and the conclusion once drawn from it was wrong.

> **`CHANGELOG.md` is stale on exactly this point, and it is not this file's to fix.** Its v3.2.0 entry
> still reads *"One honest caveat: the `.snupkg` is produced but is not yet pushed to the feed — the
> publish step in the shared pipeline still pushes `*.nupkg` only."* That was true when written and is
> **false now**. `Changelog Author` owns that file; recorded here so the next reader does not repeat it.

### Layout

| Project | Role |
|---|---|
| `ProphetsWay.BaseDataAccess/` | The library — **11 source files, zero `PackageReference` entries**, contracts plus the reflection dispatcher. Targets `netstandard2.0;net10.0` |
| `ProphetsWay.BaseDataAccess.Tests/` | xUnit + Shouldly; **116 tests** over `net48`/`net10.0`, **232 executions**. Re-derived 2026-08-23 by grepping every xUnit attribute in the project — **114 `[Fact]` plus one `[Theory]` carrying 2 `[InlineData]`** across 15 test classes, and **no `[Trait]` anywhere**, so there is no scope partition here and no filtered gate |

There is no example project in this repo — `ProphetsWay.Example` fills that role.

**The library/test TFM split is deliberate, and is unchanged by 3.2.0.** The library has shipped no
`net48` asset since 3.1.0, but the test project keeps a `net48` leg because that is how .NET Framework
*behavior* is verified — `Activator.CreateInstance<T>()` wraps a throwing constructor there and does not
on .NET Core, which is the only reason `CreateEntity<T>()` in `BaseDataAccessHelper` has a catch block.
That leg binds the `netstandard2.0` asset, i.e. the exact assembly a .NET Framework consumer receives.
Do not report the mismatch as drift.

### Key Types

| Type | Kind | Role |
|---|---|---|
| `IBaseEntity`, `IBaseIdEntity` | interfaces | Entity contracts |
| `IBaseSoftEntity`, `IBaseSoftIdEntity` | interfaces | Soft-delete entity contracts |
| `IBaseDao`, `IBaseGetAllDao`, `IBasePagedDao` | interfaces | DAO contracts by retrieval capability |
| `IBaseDataAccess` | interface | The DAL root a consumer injects. **Extends `IDisposable`** as of 3.0.0, and carries the specified disposal and transaction contracts in its `<remarks>` |
| `BaseDataAccess` | public abstract class | The optional reflection dispatcher — largest file in the library. Declares `Dispose` and all three transaction members **abstract** |
| `BaseDataAccessHelper` | internal static class | All reflection logic behind the dispatcher |
| `DataAccessConventionException` | public exception | Thrown when a derived DAL violates the convention |

Namespace is `ProphetsWay.BaseDataAccess` throughout — correct for the Data Access family.

### Behavioral Contracts Worth Knowing

- **The interface-level `<remarks>` on `IBaseDataAccess` is the source of truth for disposal,
  transactions, and threading.** It is organized into **five** sections — TRANSACTIONS / DISPOSAL /
  THREADING / CONVENTION-BASED DISPATCH / DELIBERATE OMISSIONS. Read it before documenting or
  changing any of them; do not restate its rules from memory. The CONVENTION-BASED DISPATCH section
  is explicitly *not* a term of the contract: it records dispatcher behavior so a consumer meeting
  `DataAccessConventionException` knows where it came from, and it binds no implementation written
  directly against the interface.
- **The reflection convention is specified in full in the XML `<remarks>` on
  `DataAccessConventionException`** — method names and signatures, required visibility, required
  declared return types, and identifier resolution. Read it before touching `BaseDataAccess` or
  `BaseDataAccessHelper`; it is the source of truth, not this file.
- **`IBaseDataAccess` extends `IDisposable` as of 3.0.0.** Every implementation supplies `Dispose`;
  `BaseDataAccess` declares it `public abstract void Dispose();` so even a DAL holding nothing
  disposable writes an explicit empty override. Any sample showing an implementation must include it.
- **The disposal contract:** `Dispose` is idempotent and never throws; every member *other than*
  `Dispose` throws `ObjectDisposedException` once disposed; a DAL disposes what it created and not
  what was handed to it.
- **The transaction contract:** one transaction per instance, no nesting,
  `InvalidOperationException` on misuse, scope is the instance not the connection, a failed commit
  leaves nothing open and discards its writes, calls outside a transaction auto-commit, ambient
  `TransactionScope` is untouched, and an open transaction is **rolled back** on disposal.
- **The `item` parameter on `GetAll`/`GetPaged`/`GetCount` is a type selector only** and is `null`
  when the call arrives through the dispatcher. An implementation must never read it. Any prose
  implying otherwise is wrong.
- **`GetCount` is a required component of paging, not a standalone capability.** A consumer wanting
  a bare count declares their own method on their own DAO interface.
- **`IBaseDao<T>.Get` does not promise to return the instance passed in.** Use the return value.
- **`Insert` assigning the identifier, and `Update`/`Delete` returning 1, are implementation
  conventions**, not library guarantees.
- **Exceptions from derived DAL methods propagate unwrapped.** No `TargetInvocationException`
  wrapper — a breaking change in 3.0.0.
- **Value-type entities are supported by `Get<T>` but cannot represent "not found" as `null`.**
- **`Get<T>(null)` throws `ArgumentException`** where the identifier property is a non-nullable value
  type. Reference-type (`string`) and nullable-value-type (`int?`) identifiers still accept `null`.
  `ArgumentException` is caller error; `DataAccessConventionException` is wiring error. The split is
  deliberate.
- **The identifier property must be *public*; implementing `IBaseIdEntity<T>` explicitly is not
  sufficient.** An explicit implementation compiles to a non-public, interface-qualified property, so
  neither the `{TypeName}Id` nor the `Id` lookup finds it and `Get<T>` throws
  `DataAccessConventionException` before dispatching. This is now stated in the `<remarks>` on
  `IBaseIdEntity<T>`, `DataAccessConventionException`, `IBaseDataAccess.Get` and `BaseDataAccess.Get`,
  and pinned by a characterization test — it is documented, not an open gap.

### Known Deviations

**None — re-checked 2026-08-23 rather than assumed, by opening both `.csproj` files, `app-variables.yml`
and every file in the library.** The test project and the canonical dotted monikers landed in 3.0.0,
3.1.0 retargeted the library to `netstandard2.0;net10.0`, and 3.2.0 filled the last packaging gap. Every
previously listed deviation is closed and no new one was found. Do not re-add filler entries here — an
empty state is a real state.

**Three things that look like deviations and are not.** Recorded so they are not rediscovered:

- **`<LangVersion>9.0</LangVersion>` and `<Nullable>annotations</Nullable>`** — deliberate, commented in
  the csproj, and the reason EFTools could delete ten suppressions. See the 3.2.0 section above; the
  shared block's blanket statement about nullable reference types is what needs correcting, not this.
- **The empty `<PackageReleaseNotes />` element is build-critical.** The "Extract Release Notes from
  CHANGELOG" step in `prophets-pipelines/stages/ci-build.yml` indexes
  `GetElementsByTagName("PackageReleaseNotes")[0]` with no null guard — **deleting the element as dead
  markup would fail the build.** It is *not* empty in this csproj today (it points at `CHANGELOG.md`),
  but the hazard applies to any sibling repo where it is a stub.
- **Indentation in the library is mixed** — four files use tabs, seven use four spaces. Both csproj files
  and the whole test project use tabs, there is no `.editorconfig`, and none of it is visible in the
  package. See `docs/repo-profile.md`.

Packaging metadata is **complete and correct, and now includes Source Link** — use this repo's `.csproj`
as the reference when fixing others. It is the only one in the workspace with `PublishRepositoryUrl`,
`EmbedUntrackedSources`, `IncludeSymbols`, `SymbolPackageFormat` and a `TF_BUILD`-conditioned
`ContinuousIntegrationBuild`.
