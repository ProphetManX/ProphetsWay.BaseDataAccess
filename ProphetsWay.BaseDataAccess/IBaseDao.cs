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
		/// <param name="item">
		/// An instance with its "ID" field set; no other property need be populated. <b>Not <c>null</c>.</b>
		/// The plain <typeparamref name="T"/> is what asserts that, for the reason set out on
		/// <see cref="Insert(T)"/>, and nothing enforces it.
		/// </param>
		/// <returns>
		/// The loaded entity. For a reference-type entity, <c>null</c> when no record carries that identifier.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The contract is about the returned value only. Whether an implementation populates and hands back
		/// the same instance it was given or materializes a fresh one is unspecified, and both conform. A
		/// caller must therefore use the return value and must never assume <paramref name="item"/> was
		/// mutated.
		/// </para>
		/// <para>
		/// <b>Read the <c>?</c> on the return as "may be default", not as "may be null".</b>
		/// <typeparamref name="T"/> is constrained only by an interface, which a <c>struct</c> satisfies as
		/// readily as a <c>class</c>, so the annotation resolves to <c>null</c> only where the entity is a
		/// reference type. For a value-type entity it resolves to <c>default(T)</c>, which cannot express
		/// absence - the same limitation stated at length on <see cref="IBaseDataAccess.Get{TEntityType}"/>,
		/// and such an entity must signal a miss the same way it does there. <b>For a value-type entity the
		/// annotation is therefore inert:</b> it raises no diagnostic against any implementation, gives a
		/// caller no nudge that <c>default(T)</c> did not already give them, and relieves none of that
		/// obligation to signal a miss some other way. It changes nothing there; it earns its place on the
		/// reference-type entities, where it is the difference between a truthful contract and a false one.
		/// </para>
		/// <para>
		/// The annotation is metadata only. It changes no signature, so no compiled consumer breaks and none
		/// needs to recompile.
		/// </para>
		/// </remarks>
		T? Get(T item);

		/// <summary>
		/// This will insert the passed item into the database.
		/// </summary>
		/// <param name="item">The entity to insert. <b>Not <c>null</c>.</b></param>
		/// <remarks>
		/// <para>
		/// Implementations are expected to assign the store-generated ID back onto <paramref name="item"/>
		/// once the insert completes. That is a convention left to the implementation - this library neither
		/// performs it nor verifies that it happened - so a caller reading the ID afterwards is relying on the
		/// Data Access Layer honoring the convention.
		/// </para>
		/// <para>
		/// <b><c>null</c> is not a legal argument, and the plain <typeparamref name="T"/> is what says so.</b>
		/// This library publishes nullable annotations - <c>&lt;Nullable&gt;annotations&lt;/Nullable&gt;</c> -
		/// which removed "neutral" as a third option: a parameter is now either annotated nullable or asserted
		/// non-null, and the absence of a <c>?</c> here is the second of those, chosen deliberately rather than
		/// left unexamined.
		/// </para>
		/// <para>
		/// <b>Nothing enforces it, and that is deliberate too.</b> Neither this interface nor
		/// <see cref="BaseDataAccess"/> guards the argument; the dispatcher forwards a <c>null</c> to the
		/// derived Data Access Layer untouched. Passing one is therefore a caller defect, and its consequence
		/// is whatever that Data Access Layer does with it - a <see cref="System.NullReferenceException"/>, a
		/// silently written empty row, or anything else it chooses. Tests in this library that push a
		/// <c>null</c> along this path characterize the unguarded dispatcher; <b>they are not a permission</b>,
		/// and an implementation remains free to reject a <c>null</c> however it sees fit.
		/// </para>
		/// </remarks>
		void Insert(T item);

		/// <summary>
		/// This will update the passed item in the database.
		/// </summary>
		/// <param name="item">
		/// The entity to update, identified by the "ID" field it already carries. <b>Not <c>null</c>.</b>
		/// </param>
		/// <returns>
		/// The number of rows the update actually affected - typically 1 for a record that exists, and 0 when
		/// the identifier matches no stored row.
		/// </returns>
		/// <remarks>
		/// The non-null assertion on <paramref name="item"/> is the deliberate, unenforced one set out on
		/// <see cref="Insert(T)"/>: <c>null</c> is not a legal argument, nothing here guards against one, and
		/// the dispatcher on <see cref="BaseDataAccess"/> forwards it to the derived Data Access Layer
		/// untouched.
		/// </remarks>
		int Update(T item);

		/// <summary>
		/// This will delete the passed item from the database, it should only require that the "ID" field be set.
		/// </summary>
		/// <param name="item">
		/// The entity to delete, identified by the "ID" field it already carries. <b>Not <c>null</c>.</b>
		/// </param>
		/// <returns>
		/// The number of rows the delete actually affected - typically 1 for a record that exists, and 0 when
		/// the identifier matches no stored row.
		/// </returns>
		/// <remarks>
		/// The non-null assertion on <paramref name="item"/> is the deliberate, unenforced one set out on
		/// <see cref="Insert(T)"/>: <c>null</c> is not a legal argument, nothing here guards against one, and
		/// the dispatcher on <see cref="BaseDataAccess"/> forwards it to the derived Data Access Layer
		/// untouched.
		/// </remarks>
		int Delete(T item);
	}
}