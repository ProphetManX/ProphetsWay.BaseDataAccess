namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// Base interface that all your Dao Interfaces should inherit from.  
    /// Creates the basic CRUD calls, requires your entity as a parameter, so all Daos have the same basic signature
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBaseDao<T> where T : IBaseEntity
	{
		/// <summary>
		/// This should only require that the "ID" field be set, 
        /// it will then load all properties from the DB and return the passed Object.
		/// </summary>
		T Get(T item);

        /// <summary>
        /// This will insert the passed item into the database, 
        /// the ID property will be set on the object once the method is completed.
        /// </summary>
		void Insert(T item);

        /// <summary>
        /// This will update the passed item in the database, 
        /// for a sanity check, it will return the number of rows updated, which should always be 1.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
		int Update(T item);

        /// <summary>
        /// This will delete the passed item from the database, it should only require that the "ID" field be set, 
        /// but it will then delete the record from the database and return the number of rows affected, which should always be 1.
        /// </summary>
        /// <returns>The number of rows affected.</returns>
		int Delete(T item);
	}
}