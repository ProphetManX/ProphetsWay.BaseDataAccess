namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// Base interface that all your Dao Interfaces should inherit from.  
	/// Creates the basic CRUD calls, requires your entity as a parameter, so all Daos have the same basic signature
	/// </summary>
	/// <typeparam name="T">The entity type this Dao reads and writes.</typeparam>
	public interface IBaseDao<T> where T : IBaseEntity
	{
		/// <summary>
		/// Loads the entity matching the identifier carried by <paramref name="item"/>.
		/// </summary>
		/// <param name="item">An instance with its "ID" field set; no other property need be populated.</param>
		/// <returns>
		/// The loaded entity. For a reference-type entity, <c>null</c> when no record carries that identifier.
		/// </returns>
		/// <remarks>
		/// The contract is about the returned value only. Whether an implementation populates and hands back
		/// the same instance it was given or materializes a fresh one is unspecified, and both conform. A
		/// caller must therefore use the return value and must never assume <paramref name="item"/> was
		/// mutated.
		/// </remarks>
		T Get(T item);

		/// <summary>
		/// This will insert the passed item into the database.
		/// </summary>
		/// <param name="item">The entity to insert.</param>
		/// <remarks>
		/// Implementations are expected to assign the store-generated ID back onto <paramref name="item"/>
		/// once the insert completes. That is a convention left to the implementation - this library neither
		/// performs it nor verifies that it happened - so a caller reading the ID afterwards is relying on the
		/// Data Access Layer honoring the convention.
		/// </remarks>
		void Insert(T item);

		/// <summary>
		/// This will update the passed item in the database.
		/// </summary>
		/// <param name="item">The entity to update, identified by the "ID" field it already carries.</param>
		/// <returns>
		/// The number of rows the update actually affected - typically 1 for a record that exists, and 0 when
		/// the identifier matches no stored row.
		/// </returns>
		int Update(T item);

		/// <summary>
		/// This will delete the passed item from the database, it should only require that the "ID" field be set.
		/// </summary>
		/// <param name="item">The entity to delete, identified by the "ID" field it already carries.</param>
		/// <returns>
		/// The number of rows the delete actually affected - typically 1 for a record that exists, and 0 when
		/// the identifier matches no stored row.
		/// </returns>
		int Delete(T item);
	}
}