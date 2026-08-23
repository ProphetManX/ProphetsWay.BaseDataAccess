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
        /// <para>
        /// The <c>?</c> is therefore the truthful annotation - the dispatcher really does pass <c>null</c> -
        /// and it carries a cost worth knowing about in advance. Because this is a <i>parameter</i>,
        /// nullability runs the opposite way from a return type: an implementation declaring
        /// <c>GetAll(T item)</c> is stricter than this interface allows, and a nullable-enabled compilation
        /// reports that as <c>CS8767</c>. Declare the parameter as <c>T?</c> to match. The annotation is
        /// metadata only and breaks no compiled consumer.
        /// </para>
        /// </param>
        /// <returns>
        /// Every stored entity of type 'T', and <b>never <c>null</c></b>. An empty collection is the expected
        /// answer when there are none: no results is not the same as no result, and a caller is not expected
        /// to null-check this. Returning <c>null</c> is a defect in the implementation rather than a permitted
        /// alternative.
        /// </returns>
        /// <remarks>
        /// <para>
        /// An array satisfies <see cref="IList{T}"/> while remaining fixed size, so a caller should treat the
        /// returned collection as read-only unless the Data Access Layer it is talking to says otherwise.
        /// </para>
        /// <para>
        /// <b>The elements are asserted non-null as well as the collection, and this is the one place that is
        /// written down.</b> It holds for every <see cref="IList{T}"/> this library returns - this member,
        /// <c>GetPaged</c> on <see cref="IBasePagedDao{T}"/>, and both of their counterparts on
        /// <see cref="IBaseDataAccess"/>: a returned list carries no <c>null</c> entries, and an implementation
        /// with nothing to return returns an empty list rather than one padded with <c>null</c>. It is
        /// unpoliced in exactly the way the collection itself is, for the same reason.
        /// </para>
        /// <para>
        /// <b>Nothing enforces the non-null guarantee, which is why it is stated as an obligation.</b>
        /// <see cref="BaseDataAccess.GetAll{T}"/> does not inspect the returned value and would forward a
        /// <c>null</c> to the caller untouched; it is not reported as
        /// <see cref="DataAccessConventionException"/>, because the convention checks declared types rather
        /// than returned values. The guarantee rests on the implementation honouring it.
        /// </para>
        /// </remarks>
        IList<T> GetAll(T? item);
    }
}
