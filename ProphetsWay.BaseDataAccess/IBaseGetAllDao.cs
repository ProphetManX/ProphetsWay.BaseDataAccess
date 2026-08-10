using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An interface to specify a particular entity should have a "GetAll" method
    /// </summary>
    public interface IBaseGetAllDao<T> : IBaseDao<T> where T : IBaseEntity
    {
        /// <summary>
        /// Will return all the items from the database of the given type 'T'.
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
        /// <see cref="BaseDataAccess.GetAll{T}"/>.
        /// </param>
        /// <returns>
        /// Every stored entity of type 'T'. Returning an empty collection when there are none is the expected
        /// behavior of an implementation; a <c>null</c> result is not intercepted and is forwarded to the
        /// caller untouched by <see cref="BaseDataAccess.GetAll{T}"/>.
        /// </returns>
        /// <remarks>
        /// An array satisfies <see cref="IList{T}"/> while remaining fixed size, so a caller should treat the
        /// returned collection as read-only unless the Data Access Layer it is talking to says otherwise.
        /// </remarks>
        IList<T> GetAll(T item);
    }
}
