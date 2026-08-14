using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// An interface to define some basic calls your Base DAL should have accessible.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This file is organised into five sections: <b>TRANSACTIONS</b>, <b>DISPOSAL</b>, <b>THREADING</b>,
	/// <b>CONVENTION-BASED DISPATCH</b>, and <b>DELIBERATE OMISSIONS</b>. Each section states its binding rules
	/// first and its reasoning after, so a reader who only needs the rules can stop at the bullet lists.
	/// </para>
	///
	/// <para><b>═══ TRANSACTIONS ═══</b></para>
	/// <para>
	/// <see cref="TransactionStart"/>, <see cref="TransactionCommit"/> and <see cref="TransactionRollBack"/> exist
	/// for one narrow job: making a bounded batch of writes atomic, so a complex set of records either all persist
	/// or none of them do. They are a scalpel for business logic to reach for deliberately - not an ambient
	/// transaction system, not a unit-of-work framework, and not something to wrap around every call. Open one, do
	/// the batch, close it.
	/// </para>
	/// <list type="bullet">
	/// <item><description>
	/// <b>Outside a transaction, every call auto-commits on its own.</b> A call made while no transaction is open
	/// stands alone and is committed as it completes.
	/// </description></item>
	/// <item><description>
	/// <b>Ambient transactions are left alone.</b> These three members neither create a <c>TransactionScope</c> nor
	/// suppress one. Whatever the underlying provider does when an ambient scope is present - a SQL Server or Azure
	/// SQL provider enrolling in it, for instance - happens exactly as it would without this library. These rules
	/// govern these three members; they do not forbid the provider underneath from behaving as it always has.
	/// </description></item>
	/// <item><description>
	/// <b>Transactions do not nest.</b> See <b>DELIBERATE OMISSIONS</b> below; the per-member rules are on
	/// <see cref="TransactionStart"/>.
	/// </description></item>
	/// </list>
	///
	/// <para><b>═══ DISPOSAL ═══</b></para>
	/// <para>
	/// A Data Access Layer owns things that must be released - a connection, a context, possibly an open
	/// transaction - which is why this interface extends <see cref="IDisposable"/>. The binding rules:
	/// </para>
	/// <list type="bullet">
	/// <item><description>
	/// <b>Disposal rolls back, never commits.</b> A transaction still open when the instance is disposed is rolled
	/// back, as stated on <see cref="TransactionStart"/>.
	/// </description></item>
	/// <item><description>
	/// <b><see cref="IDisposable.Dispose"/> is idempotent. A second and every subsequent call is a no-op and must
	/// never throw.</b> Read that against the transaction members one screen away, which deliberately set the
	/// opposite precedent: calling <see cref="TransactionCommit"/> or <see cref="TransactionRollBack"/> twice
	/// <b>throws</b>. The difference is intentional. Disposal is a cleanup operation that has to be safe to call
	/// from a <c>finally</c> block or a <c>using</c> statement regardless of what came before, so "already done"
	/// is a normal state for it. A repeated transaction call is not cleanup - it says the caller has lost track of
	/// its own control flow, which is a genuine logic error worth surfacing. Do not carry the transaction instinct
	/// across to <c>Dispose</c>.
	/// </description></item>
	/// <item><description>
	/// <b>Calling any member other than <see cref="IDisposable.Dispose"/> on a disposed instance throws
	/// <see cref="ObjectDisposedException"/>.</b> Note that
	/// <see cref="ObjectDisposedException"/> derives from <see cref="InvalidOperationException"/>, which is the
	/// type the transaction members throw. That is deliberate and not a coincidence: the whole family of "you
	/// called this at the wrong time" failures is catchable uniformly, so a consumer's
	/// <c>catch (InvalidOperationException)</c> around transaction handling catches use-after-dispose too.
	/// </description></item>
	/// <item><description>
	/// <b><see cref="IDisposable.Dispose"/> never throws</b> - including when the rollback it performs fails. A
	/// failed rollback during disposal is swallowed; an implementation may log it, but must not propagate it.
	/// Throwing from <c>Dispose</c> masks any in-flight exception inside a <c>using</c> block, turning a
	/// diagnosable failure into a confusing one. And the case being protected against is already handled
	/// underneath: an abandoned transaction is rolled back by the database anyway once the connection drops.
	/// </description></item>
	/// <item><description>
	/// <b>The Data Access Layer disposes what it created; anything handed to it belongs to the caller.</b> If a
	/// consumer passes a connection, a context or a transaction <i>into</i> the Data Access Layer, disposing the
	/// Data Access Layer must not dispose that resource - the caller who supplied it still owns it and will
	/// dispose it on its own schedule. This is where double-dispose bugs live, which is why it is stated plainly
	/// rather than left to instinct.
	/// </description></item>
	/// </list>
	/// <para>
	/// <b>Dependency-injection lifetime is the consumer's decision.</b> Now that the Data Access Layer is
	/// <see cref="IDisposable"/>, a container will dispose it at the end of the scope it was resolved in, where
	/// previously it would not have. That change of behaviour is noted here so nobody is surprised by it. A
	/// consumer is free to register the implementation as a singleton if that suits their deployment; choosing a
	/// lifetime that matches the resources the implementation holds is their responsibility, not this contract's.
	/// </para>
	///
	/// <para><b>═══ THREADING ═══</b></para>
	/// <para>
	/// Transaction state belongs to the Data Access Layer instance, so an instance with a transaction open carries
	/// mutable state and <b>must not be used from more than one thread while that transaction is open</b>. Beyond
	/// that single consequence, this library makes no thread-safety guarantee - see <b>DELIBERATE OMISSIONS</b>.
	/// </para>
	///
	/// <para><b>═══ CONVENTION-BASED DISPATCH ═══</b></para>
	/// <para>
	/// An implementation deriving from <see cref="BaseDataAccess"/> - the optional reflection dispatcher this
	/// library ships - does not implement <see cref="GetAll{T}"/>, <see cref="GetPaged{T}"/>,
	/// <see cref="GetCount{T}"/>, <see cref="Get{TEntityType}"/>, <see cref="Insert{TEntityType}"/>,
	/// <see cref="Update{TEntityType}"/> or <see cref="Delete{TEntityType}"/> directly. Each is resolved <b>by
	/// convention</b> onto a method of the derived Data Access Layer, chosen from the member's name and the entity
	/// type the call was made for, and a derived class that does not satisfy that convention is reported as
	/// <see cref="DataAccessConventionException"/> - the method was never written, carries the wrong signature, was
	/// declared with insufficient visibility, or declares a return type the member cannot use. The convention itself
	/// - the required names and signatures, the requirement that the method be a public instance method, the
	/// declared return type each member demands, and how the identifier property is resolved for
	/// <see cref="Get{TEntityType}"/> - is specified in full on <see cref="DataAccessConventionException"/> and is
	/// not restated here.
	/// </para>
	/// <para>
	/// <b>That is behaviour of the dispatcher, not a term of this contract.</b> This interface knows nothing of
	/// reflection or method resolution; an implementation written directly against it has no derived method to
	/// resolve and can never raise <see cref="DataAccessConventionException"/>. Nothing in this section is an
	/// obligation on such an implementation - it is recorded so a consumer who meets the exception knows what it
	/// indicates and where it came from.
	/// </para>
	///
	/// <para><b>═══ DELIBERATE OMISSIONS ═══</b></para>
	/// <para>
	/// Each of these was considered and left out on purpose. They are recorded with their reasoning so a future
	/// reader can judge whether the tradeoff still holds and raise a feature request if it does not. None of them
	/// is a gap an implementation may quietly fill in underneath this contract.
	/// </para>
	/// <list type="bullet">
	/// <item><description>
	/// <b>Any base implementation of transaction handling. <see cref="BaseDataAccess"/> provides no template
	/// method and tracks no transaction state; <see cref="TransactionStart"/>, <see cref="TransactionCommit"/>,
	/// <see cref="TransactionRollBack"/> and <see cref="IDisposable.Dispose"/> are all abstract, and the whole of
	/// transaction handling belongs to the implementation.</b> The obvious alternative was considered and
	/// rejected: a base <c>Dispose</c> that notices an open transaction and calls a <c>protected virtual</c>
	/// rollback hook the developer overrides. That would require <see cref="BaseDataAccess"/> to own transaction
	/// state it deliberately does not have - it is a reflection dispatcher with no fields, and every member it
	/// declares abstract is abstract for the same reason, because the state lives in the derived Data Access
	/// Layer. The consequence is stated plainly so nobody reads it as a gap: the rules in <b>TRANSACTIONS</b>
	/// above are obligations on an implementer, not behaviour this library performs. An implementation that
	/// ignores them will compile and run.
	/// </description></item>
	/// <item><description>
	/// <b>Nested transactions and savepoints.</b> Out of scope by decision. Transactions here are a scalpel for a
	/// bounded batch of writes, not an ambient system, and richer semantics would be a future addition to this
	/// interface rather than an implementation detail. An implementation must not improvise them.
	/// </description></item>
	/// <item><description>
	/// <b>A general thread-safety contract.</b> Deliberately unspecified beyond the per-instance transaction-state
	/// consequence above. Defining broader threading guarantees is a larger feature in its own right and has not
	/// been taken on.
	/// </description></item>
	/// <item><description>
	/// <b>Async members, and <c>IAsyncDisposable</c> with them.</b> This interface is entirely synchronous, and
	/// <c>IAsyncDisposable</c> is deliberately <b>not</b> implemented: async disposal without async CRUD would be
	/// half a feature. The accepted consequence is stated plainly - <b>if async members are ever added, that is
	/// itself a breaking change</b>. That cost is known and accepted, not overlooked.
	/// </description></item>
	/// <item><description>
	/// <b>A standalone count capability.</b> Considered and rejected; see <see cref="GetCount{T}"/> for the
	/// reasoning.
	/// </description></item>
	/// </list>
	/// </remarks>
	public interface IBaseDataAccess : IDisposable
	{
		/// <summary>
		/// A global version of 'GetAll' that can be implemented in your base DAL class,
		/// Allows for a simple get all call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to retrieve.</typeparam>
		/// <returns>
		/// Every stored entity of type <typeparamref name="T"/>. An implementation is expected to return an empty
		/// collection when there are none; a <c>null</c> result is not intercepted and reaches the caller
		/// untouched. An array satisfies <see cref="IList{T}"/> while remaining fixed size, so treat the result
		/// as read-only unless the Data Access Layer says otherwise.
		/// </returns>
		/// <remarks>
		/// An implementation deriving from <see cref="BaseDataAccess"/> must declare its <c>GetAll</c> with a return
		/// type assignable to <see cref="IList{T}"/>.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		IList<T> GetAll<T>() where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetPaged' that can be implemented in your base DAL class,
		/// Allows for a simple get paged call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to retrieve.</typeparam>
		/// <param name="skip">The number of entities to bypass before the page begins.</param>
		/// <param name="take">The maximum number of entities the page may contain.</param>
		/// <returns>
		/// The requested page of entities. An implementation is expected to return an empty collection when the
		/// page lies beyond the available records; a <c>null</c> result is not intercepted and reaches the caller
		/// untouched. An array satisfies <see cref="IList{T}"/> while remaining fixed size, so treat the result
		/// as read-only unless the Data Access Layer says otherwise.
		/// </returns>
		/// <remarks>
		/// An implementation deriving from <see cref="BaseDataAccess"/> must declare its <c>GetPaged</c> with a return
		/// type assignable to <see cref="IList{T}"/>.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetCount' that can be implemented in your base DAL class,
		/// Allows for a simple get count call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to count.</typeparam>
		/// <returns>The number of entities of that type, or zero when there are none.</returns>
		/// <remarks>
		/// <para>
		/// This exists as the companion to <see cref="GetPaged{T}"/> rather than as a feature in its own right:
		/// a paged view cannot show how many pages exist, or where the last one ends, without the total it is
		/// paging over. Exposing counting as a standalone capability was considered and rejected on that basis -
		/// a count wanted for some other purpose belongs on the consumer's own Dao interface as a custom method,
		/// where it can carry the filters that made it worth asking for.
		/// </para>
		/// <para>
		/// An implementation deriving from <see cref="BaseDataAccess"/> must declare its <c>GetCount</c> with a return
		/// type of <see cref="int"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		int GetCount<T>() where T : IBaseEntity;

		/// <summary>
		/// Gives access outside of the DAL to start a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>One transaction per Data Access Layer instance.</b> Calling this while a transaction is already open on
		/// this instance <b>throws</b>. Transactions do not nest.
		/// </para>
		/// <para>
		/// <b>Scope is the instance, not the connection.</b> Two Data Access Layer instances that share one underlying
		/// connection do not share a transaction; work done through one is not enrolled in the other's.
		/// </para>
		/// <para>
		/// <b>A transaction that is opened must be closed.</b> Disposing an instance with a transaction still open
		/// <b>rolls it back</b> - it is never committed. An unclosed transaction is an abandoned one, and abandoned
		/// work must not silently persist.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when a transaction is already open on this instance.
		/// </exception>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		void TransactionStart();

		/// <summary>
		/// Gives access outside of the DAL to commit a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
		/// <remarks>
		/// <para>
		/// Commits everything written through this instance since <see cref="TransactionStart"/> and leaves the
		/// instance with no transaction open. Calling this when no transaction is open <b>throws</b> - that includes
		/// calling it a second time after a commit or a rollback. It is a programming error, not a silent no-op.
		/// </para>
		/// <para>
		/// <b>A failed commit leaves no transaction open either, and the writes made inside it are discarded.</b> Once
		/// this member returns or throws, the instance has no transaction open, so a failed commit must not be followed
		/// by <see cref="TransactionRollBack"/> - there is nothing left to roll back and that call throws, because the
		/// store has already rolled the batch back.
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no transaction is open on this instance.
		/// </exception>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		void TransactionCommit();

		/// <summary>
		/// Gives access outside of the DAL to rollback a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
		/// <remarks>
		/// Discards everything written through this instance since <see cref="TransactionStart"/> and leaves the
		/// instance with no transaction open. Calling this when no transaction is open <b>throws</b> - that includes
		/// calling it a second time after a rollback or a commit, and following a commit that failed. It is a
		/// programming error, not a silent no-op.
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when no transaction is open on this instance.
		/// </exception>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		void TransactionRollBack();

		/// <summary>
		/// A global version of 'Get' that can be implemented in your base DAL class,
		/// Allows for a simple get call without instantiating an object and 
		/// manually setting the ID field everywhere used in your business logic layers.
		/// </summary>
		/// <typeparam name="TEntityType">The entity type to retrieve.</typeparam>
		/// <param name="id">
		/// The value of the entity's identifier. Typed as <see cref="object"/> so that entities keyed by
		/// <see cref="Guid"/>, <see cref="int"/>, <see cref="long"/> or anything else are all served by this one
		/// member; nothing here constrains every entity in a Data Access Layer to share one identifier type.
		/// </param>
		/// <returns>
		/// The matching entity. When <typeparamref name="TEntityType"/> is a <b>reference type</b>, <c>null</c> is
		/// returned when no entity carries that identifier, and a <c>null</c> result is an ordinary outcome rather
		/// than an error.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Which property <paramref name="id"/> refers to is settled <b>by name</b>: <c>{TypeName}Id</c> first —
		/// for an entity type named <c>Company</c> that is <c>CompanyId</c> — falling back to <c>Id</c>. The
		/// property's type is never considered. <see cref="DataAccessConventionException"/> specifies that
		/// resolution in full, along with how an implementation deriving from <see cref="BaseDataAccess"/> reports
		/// an entity type that exposes neither property, exposes one that is not public — an <b>explicit</b>
		/// implementation of <see cref="IBaseIdEntity{T}"/> among them — or exposes one with no set accessor.
		/// </para>
		/// <para>
		/// An implementation deriving from <see cref="BaseDataAccess"/> further rejects an <paramref name="id"/>
		/// the resolved property cannot hold — <c>null</c> among them where that property is a non-nullable value
		/// type — with an <see cref="ArgumentException"/>, which is <b>caller error</b> rather than the wiring
		/// error <see cref="DataAccessConventionException"/> reports.
		/// </para>
		/// <para>
		/// <b>Value-type entities cannot report "not found" as <c>null</c>.</b> The constraint on
		/// <typeparamref name="TEntityType"/> is satisfied by a <c>struct</c> as readily as by a <c>class</c>, and
		/// for a value-type entity <c>null</c> is simply not representable in the return type — an implementation
		/// deriving from <see cref="BaseDataAccess"/> must declare its <c>Get</c> with a return type assignable to
		/// the entity type, which for a value type admits only that type itself. A Data Access Layer keying on a
		/// value-type entity must therefore signal a miss some other way: return a recognizable default or sentinel
		/// value that the caller checks for, expose the lookup through a member outside this interface that can
		/// express absence, or model the entity as a reference type so that <c>null</c> is available. No member of
		/// this interface exists to distinguish "found the default value" from "found nothing" for a value-type
		/// entity; a design needing that distinction should not put the entity in a <c>struct</c>.
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		TEntityType Get<TEntityType>(object id) where TEntityType : IBaseEntity, new();

		/// <summary>
		/// A global version of 'Insert' that can be used generically.  Allows for custom
		/// generic classes/services that can support Inserting any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Insert' should be called upon.
		/// </summary>
		/// <typeparam name="TEntityType">The entity type to insert.</typeparam>
		/// <param name="item">The entity to insert.</param>
		/// <remarks>
		/// <para>
		/// Implementations are expected to assign the store-generated identifier back onto <paramref name="item"/>
		/// once the insert completes. That is a convention left to the implementation - nothing here performs it
		/// or verifies that it happened.
		/// </para>
		/// <para>
		/// An implementation deriving from <see cref="BaseDataAccess"/> may declare its <c>Insert</c> with any return
		/// type, including <c>void</c>; whatever it returns is discarded.
		/// </para>
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		void Insert<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new();

		/// <summary>
		/// A global version of 'Update' that can be used generically.  Allows for custom
		/// generic classes/services that can support Updating any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Update' should be called upon.
		/// </summary>
		/// <typeparam name="TEntityType">The entity type to update.</typeparam>
		/// <param name="item">The entity to update, identified by the identifier it already carries.</param>
		/// <returns>The count of records the Data Access Layer reports as affected.</returns>
		/// <remarks>
		/// An implementation deriving from <see cref="BaseDataAccess"/> must declare its <c>Update</c> with a return
		/// type of <see cref="int"/>, checked before the method is invoked so a mis-declared one cannot write and only
		/// then report the defect.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		int Update<TEntityType>(TEntityType item) where TEntityType: IBaseEntity, new();


		/// <summary>
		/// A global version of 'Delete' that can be used generically.  Allows for custom
		/// generic classes/services that can support Deleting any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Delete' should be called upon.
		/// </summary>
		/// <typeparam name="TEntityType">The entity type to delete.</typeparam>
		/// <param name="item">The entity to delete, identified by the identifier it already carries.</param>
		/// <returns>The count of records the Data Access Layer reports as affected.</returns>
		/// <remarks>
		/// An implementation deriving from <see cref="BaseDataAccess"/> must declare its <c>Delete</c> with a return
		/// type of <see cref="int"/>, checked before the method is invoked so a mis-declared one cannot write and only
		/// then report the defect.
		/// </remarks>
		/// <exception cref="ObjectDisposedException">Thrown when this instance has already been disposed.</exception>
		int Delete<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new();
	}
}