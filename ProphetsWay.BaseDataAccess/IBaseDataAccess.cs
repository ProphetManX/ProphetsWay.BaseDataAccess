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
		IList<T> GetAll<T>() where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetPaged' that can be implemented in your base DAL class,
		/// Allows for a simple get paged call without instantiating an empty object first.
		/// </summary>
		IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity;

		/// <summary>
		/// A global version of 'GetCount' that can be implemented in your base DAL class,
		/// Allows for a simple get count call without instantiating an empty object first.
		/// </summary>
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
		/// Implies your ID properties are of type TIdType.
		/// </summary>
		TEntityType Get<TEntityType>(object id) where TEntityType : IBaseEntity, new();

		/// <summary>
		/// A global version of 'Insert' that can be used generically.  Allows for custom
		/// generic classes/services that can support Inserting any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Insert' should be called upon.
		/// </summary>
		void Insert<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new();

		/// <summary>
		/// A global version of 'Update' that can be used generically.  Allows for custom
		/// generic classes/services that can support Updating any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Update' should be called upon.
		/// </summary>
		int Update<TEntityType>(TEntityType item) where TEntityType: IBaseEntity, new();


		/// <summary>
		/// A global version of 'Delete' that can be used generically.  Allows for custom
		/// generic classes/services that can support Deleteing any of your generic 
		/// base class entities, and the DAL will automatically identify which Dao the
		/// 'Delete' should be called upon.
		/// </summary>
		int Delete<TEntityType>(TEntityType item) where TEntityType : IBaseEntity, new();
	}

	[Obsolete("You no longer need to use this Generic type of IBaseDataAccess, you can use the normal IBaseDataAccess. (just remove the generic assignment)", false)]
	public interface IBaseDataAccess<TIdType> : IBaseDataAccess
	{

	}
}