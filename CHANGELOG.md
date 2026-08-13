# v3.1.0
### Nobody is stranded — read this first
This release retargets the library to ```netstandard2.0;net10.0```, dropping the dedicated ```net48```, ```net8.0```
and ```net9.0``` assets. That sounds like a package you can no longer install, and it is not. ```netstandard2.0``` is
still here, and it is consumable by .NET Framework 4.6.1 and later and by every .NET Core and .NET 5+ runtime. If you
are on .NET Framework 4.8, .NET 8 or .NET 9 you still install this package, you still resolve an asset, and it still
works. There is nothing you have to do.

### What does change is which assembly you bind
If you previously resolved the ```net48```, ```net8.0``` or ```net9.0``` asset, you now resolve ```netstandard2.0```.
That is behaviorally equivalent, and this is not a hopeful claim — it was checked. The library contains no conditional
compilation whatsoever, not a single ```#if``` in any of its eleven source files, and no package references at all.
```netstandard2.0``` and ```net48``` both compile at C# 7.3. The assets you were resolving before and the asset you
resolve now were the same compilation of the same code; only the folder name in the package differs.

### Why now
.NET 8 and .NET 9 both reach end of life on 10 November 2026, and .NET 10 is the current Long Term Support release.
From here the library tracks Long Term Support releases only. The odd-numbered Standard Term Support releases carry an
eighteen-month window, which is not long enough to be worth a permanent asset in a package whose whole surface is
interfaces and one reflection dispatcher.

### The test project still targets ```net48```, deliberately
A release that drops ```net48``` from the library and keeps it in the tests looks like something that was missed. It
was not. ```Activator.CreateInstance<T>()``` wraps an exception thrown by a constructor on .NET Framework and does not
on .NET Core, which is the reason ```BaseDataAccessHelper``` handles ```TargetInvocationException``` at all — see the
v3.0.0 notes on unwrapped exceptions for why that path matters to you. The ```net48``` test leg is the only thing that
exercises it. Now that the library no longer ships a ```net48``` asset, that leg binds ```netstandard2.0```, which
means it validates the exact assembly a .NET Framework consumer actually receives rather than a sibling build of it.

### Verification
All 115 tests pass on both ```net48``` and ```net10.0```, 230 executions in total, and the build produces zero
warnings, unchanged from v3.0.0. The packed package contains exactly ```lib/netstandard2.0/``` and ```lib/net10.0/```.
No source file, public member, signature or package reference changed in this release.


# v3.0.0
### Exceptions from your DAL now reach you unwrapped — read this before upgrading
This is the one change in this release that breaks quietly. Every one of the generic calls on ```BaseDataAccess```
reaches your derived method through Reflection, and Reflection used to wrap anything that method threw in a
```TargetInvocationException```. That wrapper is gone. Whatever your ```Get```, ```GetAll```, ```GetPaged```,
```GetCount```, ```Insert```, ```Update``` or ```Delete``` throws now arrives at the caller as its own type with its
original stack trace intact, and the same is now true of an exception thrown by an entity's parameterless constructor
or by its identifier property setter.

If you wrote a handler that reaches through the wrapper, it will no longer match and your handling will simply stop
running — there is no compiler error and no warning to tell you.

```c#
	//no longer catches anything - the handler is now dead code
	try { dal.Update(company); }
	catch (TargetInvocationException ex) { Log(ex.InnerException); }

	//catch what your DAL actually throws
	try { dal.Update(company); }
	catch (SqlException ex) { Log(ex); }
```

Search your solution for ```TargetInvocationException``` before you upgrade. If a call site wrapped one of these
methods, that is the code that has to change.

### Your Data Access Layer must now implement ```Dispose```
```IBaseDataAccess``` extends ```IDisposable```, so every implementation has to supply ```Dispose```. If you inherit
```BaseDataAccess``` it is declared ```public abstract void Dispose();```, which means your class will not compile
until you override it. That includes a Data Access Layer holding nothing disposable at all — you still write the
override, empty. Making it abstract rather than virtual is the point: an empty ```Dispose``` should be a decision you
made, not a default you inherited without reading.

```c#
	//a DAL that owns no connection, context or transaction still writes this
	public override void Dispose() { }
```

The quieter consequence is at your composition root. If you register ```IBaseDataAccess``` in a dependency-injection
container, the container now disposes your Data Access Layer at the end of the scope it was resolved in — it has no
reason to have done so before. Nothing at your call sites changes, but the lifetime of the object underneath them
does, and a Data Access Layer that was quietly outliving its scope will stop doing so. Registering it as a singleton
is one way to control that; which lifetime is right is your call, but it is now a call you have to make.

### The disposal contract is now specified
```Dispose``` is idempotent — a second call, and every call after it, is a no-op that must never throw. Read that
against the transaction members, which set the opposite precedent deliberately: calling ```TransactionCommit``` or
```TransactionRollBack``` twice throws, because a repeated transaction call means the caller has lost track of its own
control flow, while disposing an already-disposed object is a normal thing for cleanup code to do.

Calling any member *other than* ```Dispose``` on a disposed instance throws ```ObjectDisposedException```. That type
derives from ```InvalidOperationException```, which is what the transaction members throw, and the overlap is
intentional — one ```catch (InvalidOperationException)``` covers both "you used this wrong" cases rather than making
you write two handlers for the same class of mistake.

```Dispose``` itself never throws, including when a rollback it performs fails. Throwing from ```Dispose``` masks
whatever exception was already in flight inside a ```using``` block, turning a diagnosable failure into a misleading
one, and an abandoned transaction is rolled back by the database anyway once the connection drops. A transaction still
open at disposal is **rolled back, never committed** — an unclosed transaction is an abandoned one, and abandoned work
is not work you meant to keep.

Finally: your Data Access Layer disposes what it created, and nothing else. If a caller hands it a connection, a
context or a transaction, disposing the Data Access Layer must not dispose that resource — the caller still owns it.
That is where double-dispose bugs live, so it is now written down rather than assumed.

### The transaction contract is now specified
One transaction per Data Access Layer instance. A second ```TransactionStart``` while one is already open throws
```InvalidOperationException```, and so does a commit or rollback with none open. Transactions do not nest, and that
is a decision rather than an omission — these three members are a scalpel for making a bounded batch of writes atomic,
not an ambient transaction system and not a unit-of-work framework.

Scope is the **instance**, not the connection. Two instances pointed at the same database do not share a transaction,
and work done through one is not enrolled in the other's.

A failed commit leaves no transaction open **and discards the writes made inside it**. Once ```TransactionCommit```
returns or throws, the instance has nothing open, so a failed commit must not be followed by ```TransactionRollBack```
— there is nothing left to roll back and that call throws.

```c#
	//there is no transaction left to roll back - this catch block throws its own exception
	try { dal.TransactionCommit(); }
	catch (Exception) { dal.TransactionRollBack(); }
```

Outside a transaction every call auto-commits on its own, so you do not need one to make a single write durable. And
ambient transactions are left alone — these members neither create a ```TransactionScope``` nor suppress one, so
whatever your provider already does inside an enclosing scope keeps happening unchanged.

### Documentation corrections — the first one could have cost you a debugging session
```GetAll```, ```GetPaged``` and ```GetCount``` documented their ```item``` parameter as needing to be "an instance of
itself". That was simply false. The generic dispatcher invokes your derived method with a literal ```null``` — 
```default(T)``` when the entity is a value type — so the parameter is a **type selector** and must never be read. An
implementer who trusted the old documentation and dereferenced it compiled clean and threw ```NullReferenceException```
the first time the call arrived through the dispatcher, with nothing in the build to warn them.

```c#
	//throws when the call comes through the generic dispatcher - item is null
	public IList<Company> GetAll(Company item) => _ctx.Companies.Where(c => c.Region == item.Region).ToList();

	//the parameter selects the type and nothing else
	public IList<Company> GetAll(Company item) => _ctx.Companies.ToList();
```

```IBaseDao<T>.Get``` promised to "return the passed Object", which constrained instance identity for no reason.
Whether an implementation populates the instance it was handed or materializes a fresh one is now explicitly
unspecified and both conform — use the return value, and do not assume the argument was mutated.

```Insert``` assigning the store-generated identifier back onto the entity, and ```Update```/```Delete``` returning 1,
are now described as conventions an implementation is expected to honor rather than guarantees the library makes or
verifies. ```0``` from an update or delete is correct for an identifier matching no row.

```GetCount``` is documented as a required component of paging rather than a standalone feature — a pager cannot render
without knowing the total it is paging over. If you want a count and do not need paging, declare your own count method
on your own Dao interface; that is the supported answer, not a gap.

```IBaseEntity```, ```IBaseIdEntity<T>```, ```IBaseSoftEntity``` and ```IBaseSoftIdEntity<T>``` had no documentation at
all and now do. Two points on them are worth reading even if you have been using them for years. ```Get<T>``` does
**not** key on ```IBaseIdEntity<T>``` — the identifier is resolved by name, ```{TypeName}Id``` first and ```Id``` as
the fallback, so implementing the interface satisfies the fallback but an entity also exposing ```{TypeName}Id``` has
that one used instead. And the library reads none of ```CreatedDate```, ```UpdatedDate``` or ```DeletedDate```; soft
delete is entirely your implementation's behavior, and these interfaces only mark which entities take part in it.

### The method convention is now enforced strictly, and enforced before anything is written
The remaining breaking changes all fail loudly on the first call, so they will find you rather than the other way
around.

Parameter matching is now exact. A convention method declared with a base class or interface parameter — 
```Insert(IBaseEntity item)``` standing in for every entity type — used to be matched by the Reflection binder and
will no longer be found. Declare the method once per entity type, with that entity type as the parameter.

Declared return types are validated *before* the method is invoked. ```GetAll``` and ```GetPaged``` must declare a
type assignable to ```IList<T>```, ```Get``` must declare ```T``` or a subclass of it, and ```GetCount```,
```Update``` and ```Delete``` must declare ```int```. ```Insert``` remains unconstrained and may return anything,
including ```void```. Previously a mis-declared ```Update``` or ```Delete``` ran to completion, wrote to the
database, and only then failed casting its result; that no longer happens.

```static``` convention methods are no longer found. The lookup has always been documented as targeting the public
surface of your DAL, but ```static``` methods were incidentally reachable; they are not any more. Make the method a
public instance method.

The obsolete ```IBaseDataAccess<TIdType>``` and ```BaseDataAccess<TIdType>```, deprecated back in v2.1.0, have been
removed. The fix is the one the obsolete warning has been suggesting for four minor versions — drop the generic
argument and use ```IBaseDataAccess``` / ```BaseDataAccess```.

### Target frameworks consolidated
Now targeting ```netstandard2.0;net48;net8.0;net9.0```, replacing ```net461;net471;net48;net50;net60;net70;net80;net90```.
No consumer is stranded: ```net461``` and ```net471``` resolve against ```netstandard2.0```, as do .NET 5, 6 and 7,
all three of which are past end of support.

### Fixed
A struct entity silently received a default identifier from ```Get<T>```. The probe entity was boxed at the moment
the identifier was assigned, so the mutation landed on a copy that was then discarded and your ```Get``` method
received an entity with an unset key — no exception, just the wrong row or no row. Struct entities now receive the
identifier they were asked for.

```Get<T>(null)``` answered "not found" instead of rejecting the call. A wrong-typed identifier has always thrown
```ArgumentException``` — that is a caller mistake rather than a wiring error, which is why it is deliberately not a
```DataAccessConventionException``` — but ```null``` escaped the rule, because the reflection layer converts it to
```default``` for a non-nullable value type instead of refusing it. An entity keyed on ```int``` therefore probed for
identifier ```0``` and handed back whatever that found, in practice ```null```. Your bug came back as a plausible
answer, with no exception and nothing in the build to catch it. ```Get<T>``` now throws ```ArgumentException```
naming the property, its type and the entity type.

The test is on the **identifier property's type**, not on ```null``` itself. A reference type such as ```string```,
or a nullable value type such as ```int?```, can hold ```null```, and for those ```null``` still reaches your
```Get``` unchanged. Only a non-nullable value type rejects it.

```c#
	//identifier property is int - used to return null, now throws ArgumentException
	dal.Get<Company>(null);

	//identifier property is string or int? - unchanged, null reaches your Get
	dal.Get<Account>(null);
```

If you were passing ```null``` and relying on ```null``` coming back, that call now throws — behaviorally breaking,
though it is a narrow usage and almost certainly an accidental one, since the old behavior was concealing the mistake
rather than offering a feature. None of this had any test coverage before now — the suite asserted on
```ArgumentException``` nowhere at all, which is how it survived — and closing it took the suite from 111 tests to 115.

```GetAll<T>``` and ```GetPaged<T>``` returned ```null``` when the derived method declared a return type that was not
an ```IList<T>```, because the result was coerced with ```as```. A wrong return type is now reported as the wiring
error it is.

An identifier property with no set accessor produced a raw ```ArgumentException``` from the Reflection layer. It is
now reported as a convention error naming the entity and the property.

A convention method hidden with ```new``` bound unpredictably, because the order Reflection returns same-named
methods in is unspecified. The hierarchy is now walked one level at a time, most derived first, so the method
selected is the one a compile-time call against the same type would bind to.

### Added
```DataAccessConventionException``` replaces the generic ```Exception``` previously thrown for wiring errors, so
these can be caught and filtered distinctly from data errors. It carries the full specification of the convention in
its own documentation — the method name and signature looked for, the visibility required, the return type each one
must declare, and how the identifier property is resolved for ```Get```. If you are writing a class that inherits
```BaseDataAccess```, read that type first.

Its messages render types the way you wrote them, so a signature reads as ```(Company, int, int)``` rather than
```(Company, Int32, Int32)```, and a return type as ```IList<Company>``` rather than in namespace-qualified
backtick-arity form.

```docs/feature-requests.md``` records the decisions this release deliberately did not make, and why — a published
conformance kit for verifying an implementation against these contracts, nested transactions and savepoints, a general
thread-safety contract, async members and ```IAsyncDisposable```, a standalone count capability that was considered and
**rejected**, and splitting the transaction members onto their own interface. If something here looks like an
oversight, check there first; the reasoning is written down so you can judge whether the tradeoff still holds and raise
a feature request when it stops holding.

```ProphetsWay.BaseDataAccess.Tests``` was added — the first automated coverage this library has had, 115 tests
pinning the convention, the dispatch behavior, the disposal and transaction contracts, and every fix listed above.

### The XML documentation now ships with the package
Everything above is written on the interfaces, and until now none of it reached you if you installed the package
rather than read the repository. ```GenerateDocumentationFile``` had never been set here, so no XML documentation file
was produced and nothing was packed into the nupkg — not even member summaries. The file is now emitted for all four
target frameworks and the SDK packs it beside each assembly, so your IDE has something to read. ```IBaseIdEntity<T>.Id```
picked up the summary it was missing on the way, which leaves the public surface documented in full, and the dispatched
members on ```IBaseDataAccess``` now name ```DataAccessConventionException``` and the return type each requires instead
of leaving you to find that elsewhere.

One caveat, and it belongs to the tooling rather than the package. ```<summary>```, ```<param>```, ```<typeparam>```
and ```<returns>``` surface reliably. ```<remarks>``` does not, and ```<remarks>``` is where the transaction, disposal,
threading and convention contracts actually live — Rider's quick documentation and VS Code's hover show it, while
Visual Studio's tooltip historically has not. If a tooltip stops short of the contract, it was not left out; the source
and this changelog remain the complete account.

### A note on visibility
The convention has always required a public instance method. That has not changed in this release; it is now stated
in the documentation and covered by tests rather than left to be discovered.


#2.5.0
### Added another interface to identify a base "Soft" entity without an ID property
Added an interface that identifies an entity as "Soft" but doesn't have a specific "Id" property, this is meant to be used in
conjunction with many-to-many tables, or a table that has a compound key and neither is a basic "Id" property that should be keyed off of.


# v2.4.0
### Adding an interface to identify a base "Soft" entity
Added an interface that identifies an entity as "Soft" so that the DAL will not actually delete the record, but set the deleted date and use that date value as the "deleted" flag.

# v2.3.0
### Updated .Net Framework versions
Updated the target framework to include .Net 7.0, 8.0, and 9.0 and removed support for .Net Core 2.0, 2.1, and 2.2 as they are no longer supported by Microsoft.

# v2.2.0
### Added Generic CRUD Calls
Added the ability to call ```Insert```, ```Update```, and ```Delete``` generically, similar to how ```Get<\T>(int id)``` works already.

# v2.1.3, v2.1.4
### Minor pipeline fixes
No functional changes, just updating how the pipeline triggers and the readme file.

# v2.1.2
### Updated to support .net 6.0
Updated the pipeline template to be more robust and reusable across many of my other projects.  Also updated the build
targets to support .net 6.0

# v2.1.0
### Rolled up ```IBaseDataAccess<TIdType>```/```BaseDataAccess<TIdType>``` into their base classes
Updated the root base interface/class to include the generic ```Get``` method, because it's possible the user would build
a database where most records have an ```int``` primary key, but on one large transaction table you'd prefer to use ```long```
and yet in another you'd want to use ```Guid``` so you can share the key across contexts.  The original implementation
only allowed one type of primary key.  Tagged the old methods as Obsolete, but using them will give you a warning, suggestion
includes the note to simply remove the generic assignment.


# v2.0.0
### Updated to support .net 5.0
Updated a few things, unfortunately it removed a little bit of functionality, so it counts as a major update, 
even tho it's really quite a minor update.
- Removed target frameworks that are not longer supported by Microsoft (netcoreapp2.0, netcoreapp2.2, netcoreapp3.0).
- Added target framework for .Net 5.0.
- Added an icon for the package.
- Updated reference to ProphetsWay.Example 


# v1.1.1
Added support for .Net Framework 4.8 explicitly and updated the changelog to include changes from v1.1.0.
Updated ```IBaseDataAccessInt``` and ```IBaseDataAccessLong``` to implement the new ```IBaseDataAccess<T>```
created in version v1.1.0 to cut down on duplicate code.

Updated ```BaseDataAccessInt``` and ```BaseDataAccessLong``` to implement the new ```BaseDataAccess<T>``` 
created in version v1.1.0 to cut down on duplicate code.

Removed the Example projects and added a submodule pointing to [ProphetsWay.Example](https://github.com/ProphetManX/ProphetsWay.Example).
ProphetsWay.Example references a NuGet reference to this project, albeit a slightly older version.



# v1.1.0
### New Interfaces ```IBaseIdEntity<T>``` and ```IBaseDataAccess<T>```
For added functionality/flexibility, I added some code to specify the ID property of your entities, as well as its type.
Then was able to refactor the BaseDataAccess classes to make use of these new features.

Old classes are marked as Obsolete, but are still usable, all changes are backwards compatible.

##### ```IBaseIdEntity<T>```
Created a new optional interface, IBaseIdEntity, which inherits from IBaseEntity for backwards compatibility
but this new interface will specify a property "Id" that must exist on your entities, and the type of the Id is 
specified by the generic passed in.  In general its likely to be either int, long, or a Guid.

##### ```IBaseDataAccess<T>```
Created a new interface to replace the two older options ```IBaseDataAccessInt``` and ```IBaseDataAccessLong```.
Now supports ```Guid``` id types.

##### ```BaseDataAccess<TIdType>```
Created a new base abstract class to replace the two older options ```BaseDataAccessInt``` and ```BaseDataAccessLong```.
Now supports ```Guid``` id types.



# v1.0.0
### Initial proper release.  
Contains all the interfaces and a single abstract base class for implementing a decoupled Data Access Layer (DAL) for your software solution.  See the 
readme for more information and check out the Example project for a working solution to reference.