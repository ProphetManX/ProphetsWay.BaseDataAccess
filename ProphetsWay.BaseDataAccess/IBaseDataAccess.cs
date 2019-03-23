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
        IList<T> GetAll<T>() where T : IBaseDDLEntity, new();

        /// <summary>
        /// A global version of 'GetPaged' that can be implemented in your base DAL class,
        /// Allows for a simple get paged call without instantiating an empty object first.
        /// </summary>
        IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity, new();

        /// <summary>
        /// A global version of 'GetCount' that can be implemented in your base DAL class,
        /// Allows for a simple get count call without instantiating an empty object first.
        /// </summary>
        int GetCount<T>() where T : IBaseEntity, new();

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
	}

    /// <summary>
    /// An interface to define some basic calls your Base DAL should have accessible.
    /// Adds a nice shortcut method if you're using an "int" based index key, inherits IBaseDataAccess
    /// </summary>
    public interface IBaseDataAccessInt : IBaseDataAccess
    {
        /// <summary>
        /// A global version of 'Get' that can be implemented in your base DAL class,
        /// Allows for a simple get call without instantiating an object and 
        /// manually setting the ID field everywhere used in your business logic layers.
        /// Implies your ID properties are of type Int.
        /// </summary>
        T Get<T>(int id) where T : IBaseEntity, new();
    }

    /// <summary>
    /// An interface to define some basic calls your Base DAL should have accessible.
    /// Adds a nice shortcut method if you're using an "long" based index key
    /// </summary>
    public interface IBaseDataAccessLong : IBaseDataAccess
    {
        /// <summary>
        /// A global version of 'Get' that can be implemented in your base DAL class,
        /// Allows for a simple get call without instantiating an object and 
        /// manually setting the ID field everywhere used in your business logic layers.
        /// Implies your ID properties are of type Long.
        /// </summary>
        T Get<T>(long id) where T : IBaseEntity, new();
    }


}