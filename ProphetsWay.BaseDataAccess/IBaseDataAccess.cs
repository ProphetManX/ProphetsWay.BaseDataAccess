using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// An interface to define some basic calls your Base DAL should have accessible.
	/// </summary>
	public interface IBaseDataAccess
	{
		/// <summary>
		/// A global version of 'GetAll' that can be implemented in your base DAL class,
		/// Allows for a simple get all call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to retrieve.</typeparam>
		IList<T> GetAll<T>() where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetPaged' that can be implemented in your base DAL class,
		/// Allows for a simple get paged call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to retrieve.</typeparam>
		/// <param name="skip">The number of entities to bypass before the page begins.</param>
		/// <param name="take">The maximum number of entities the page may contain.</param>
		IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetCount' that can be implemented in your base DAL class,
		/// Allows for a simple get count call without instantiating an empty object first.
		/// </summary>
		/// <typeparam name="T">The entity type to count.</typeparam>
		/// <returns>The number of entities of that type, or zero when there are none.</returns>
		int GetCount<T>() where T : IBaseEntity;

		/// <summary>
		/// Gives access outside of the DAL to start a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
		void TransactionStart();

		/// <summary>
		/// Gives access outside of the DAL to commit a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
		void TransactionCommit();

		/// <summary>
		/// Gives access outside of the DAL to rollback a transaction,
		/// allows the business layer logic to wrap many calls within a transaction 
		/// </summary>
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
		/// an entity type that exposes neither property, or exposes one with no set accessor.
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
		TEntityType Get<TEntityType>(object id) where TEntityType : IBaseEntity, new();

		/// <summary>
		/// A global version of 'Insert' that can be used generically.  Allows for custom
		/// generic classes/services that can support Inserting any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Insert' should be called upon.
		/// </summary>
		/// <typeparam name="TEntityType">The entity type to insert.</typeparam>
		/// <param name="item">The entity to insert.</param>
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
		int Delete<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new();
	}
}