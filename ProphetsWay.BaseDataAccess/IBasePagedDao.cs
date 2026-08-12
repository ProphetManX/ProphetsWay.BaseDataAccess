using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An interface to specify a particular entity should support paging.
    /// </summary>
    /// <remarks>
    /// Paging is one capability, expressed here as two members. <see cref="GetCount"/> is not an independent
    /// feature bolted onto <see cref="GetPaged"/> - it is a required component of paging, because a pager
    /// cannot render a page count, a last-page control, or any bound on how far forward the user may move
    /// without knowing the total number of records it is paging over. Implementing this interface therefore
    /// means implementing paging, and a total count is part of what paging is.
    /// A consumer who wants a count and does not need paging should declare their own count method on their
    /// own Dao interface, exactly as they would any other custom method. Nothing in this library prevents it,
    /// and that is the intended path.
    /// </remarks>
    public interface IBasePagedDao<T> : IBaseDao<T> where T: IBaseEntity
    {
        /// <summary>
        /// Will return a subset of all items from the database of the given type 'T'.
        /// </summary>
        /// <param name="item">
        /// A type selector, not data. The parameter exists only to identify which specific Dao/BaseEntity
        /// this call belongs to by selecting the overload for entity type 'T'; it carries no values and
        /// nothing is read from it. When this member is reached through the generic dispatcher on
        /// <see cref="BaseDataAccess"/> it is invoked with a literal <c>null</c> for a reference-type entity,
        /// and when 'T' is a value type the reflection layer materializes that <c>null</c> as
        /// <c>default(T)</c>, so a <c>struct</c> entity arrives zero-initialized rather than null.
        /// <b>An implementation must never read this parameter.</b> Dereferencing it compiles cleanly and
        /// throws <see cref="System.NullReferenceException"/> the first time the call arrives through
        /// <see cref="BaseDataAccess.GetPaged{T}"/>.
        /// </param>
        /// <param name="skip">The amount of records to skip.</param>
        /// <param name="take">The amount of records to take.</param>
        /// <returns>
        /// The requested page of entities. Returning an empty collection when the page lies beyond the
        /// available records is the expected behavior of an implementation; a <c>null</c> result is not
        /// intercepted and is forwarded to the caller untouched by <see cref="BaseDataAccess.GetPaged{T}"/>.
        /// </returns>
        /// <remarks>
        /// An array satisfies <see cref="IList{T}"/> while remaining fixed size, so a caller should treat the
        /// returned collection as read-only unless the Data Access Layer it is talking to says otherwise.
        /// </remarks>
        IList<T> GetPaged(T item, int skip, int take);

        /// <summary>
        /// Returns the total number of stored records of type 'T', which is the upper boundary
        /// <see cref="GetPaged"/> is paged against.
        /// </summary>
        /// <param name="item">
        /// A type selector, not data. The parameter exists only to identify which specific Dao/BaseEntity
        /// this call belongs to by selecting the overload for entity type 'T'; it carries no values and
        /// nothing is read from it. When this member is reached through the generic dispatcher on
        /// <see cref="BaseDataAccess"/> it is invoked with a literal <c>null</c> for a reference-type entity,
        /// and when 'T' is a value type the reflection layer materializes that <c>null</c> as
        /// <c>default(T)</c>, so a <c>struct</c> entity arrives zero-initialized rather than null.
        /// <b>An implementation must never read this parameter.</b> Dereferencing it compiles cleanly and
        /// throws <see cref="System.NullReferenceException"/> the first time the call arrives through
        /// <see cref="BaseDataAccess.GetCount{T}"/>.
        /// </param>
        /// <returns>The number of stored records of type 'T', or zero when there are none.</returns>
        /// <remarks>
        /// This member belongs to the paging capability rather than standing on its own: it is what lets a
        /// front end displaying a paged view know how many pages exist and where the last one ends. It is
        /// bundled with <see cref="GetPaged"/> for that reason. A count needed for some other purpose belongs
        /// on the consumer's own Dao interface as a custom method.
        /// </remarks>
        int GetCount(T item);
    }
}
