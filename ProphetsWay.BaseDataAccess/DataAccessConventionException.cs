using System;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// Signals that a class deriving from <see cref="BaseDataAccess"/> does not satisfy the reflection-based
    /// method convention that <see cref="BaseDataAccess"/> relies on to dispatch its generic operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exception always indicates a programming or wiring error in the derived Data Access Layer — a method
    /// that was never written, was written with the wrong signature, was declared with insufficient visibility,
    /// or returns the wrong type. It never indicates a runtime data condition: a missing row, an empty table,
    /// a null identifier, or a failed query will not produce it.
    /// </para>
    /// <para>
    /// Because the cause is structural, the exception is deterministic. For a given derived type and entity type
    /// the same call will fail the same way on every invocation, on every machine, regardless of the data present.
    /// It should surface during development or in a smoke test, not in production traffic, and callers should not
    /// catch it as part of normal control flow.
    /// </para>
    /// <para>
    /// It is thrown in exactly three circumstances:
    /// </para>
    /// <para>
    /// <b>1. Method not found.</b> <see cref="BaseDataAccess"/> looks on its own concrete runtime type for a
    /// <i>public instance</i> method with a specific name and parameter signature, determined by the entity type
    /// <c>T</c> being operated on:
    /// <list type="table">
    /// <listheader><term>Base member</term><description>Required method on the derived class</description></listheader>
    /// <item><term><c>GetAll&lt;T&gt;()</c></term><description><c>GetAll(T)</c></description></item>
    /// <item><term><c>GetCount&lt;T&gt;()</c></term><description><c>GetCount(T)</c></description></item>
    /// <item><term><c>GetPaged&lt;T&gt;(int, int)</c></term><description><c>GetPaged(T, int, int)</c></description></item>
    /// <item><term><c>Get&lt;T&gt;(object)</c></term><description><c>Get(T)</c></description></item>
    /// <item><term><c>Insert&lt;T&gt;(T)</c></term><description><c>Insert(T)</c></description></item>
    /// <item><term><c>Update&lt;T&gt;(T)</c></term><description><c>Update(T)</c></description></item>
    /// <item><term><c>Delete&lt;T&gt;(T)</c></term><description><c>Delete(T)</c></description></item>
    /// </list>
    /// If no such method exists, this exception is thrown. The parameter of type <c>T</c> exists only to
    /// disambiguate overloads by entity type; for <c>GetAll</c>, <c>GetCount</c> and <c>GetPaged</c> it is invoked
    /// with <c>null</c> and is not expected to be read.
    /// </para>
    /// <para>
    /// The lookup is restricted to public instance methods. A method that is <c>private</c>, <c>protected</c>,
    /// <c>internal</c>, <c>protected internal</c> or <c>private protected</c> is invisible to it, as is a
    /// <c>static</c> method, and each of those therefore produces this exception exactly as if the method had not
    /// been written at all. That is intended behavior and not an oversight — the convention requires the method to
    /// be part of the derived class's public surface. A method with the right name but a parameter list that does
    /// not match the required signature — wrong arity, wrong parameter types, or an entity parameter typed as a
    /// base type or interface rather than <c>T</c> itself — is likewise not a match and produces this exception.
    /// </para>
    /// <para>
    /// <b>2. Identifier property not found.</b> During <c>Get&lt;T&gt;(object id)</c>, a new instance of <c>T</c>
    /// is constructed and its identifier property is assigned the supplied <c>id</c> before the derived
    /// <c>Get(T)</c> method is invoked. The property is resolved by name: first <c>{TypeName}Id</c> — for an entity
    /// type named <c>Company</c> that is <c>CompanyId</c> — falling back to <c>Id</c>. If <c>T</c> exposes neither
    /// property, this exception is thrown. Resolution is by name only; no attribute, base type, or interface member
    /// is consulted, and the property's type is not considered when matching.
    /// </para>
    /// <para>
    /// <b>3. Return type mismatch.</b> A matching derived method was found and invoked without error, but the value
    /// it returned cannot be assigned to the return type the corresponding <see cref="BaseDataAccess"/> member
    /// declares. For example, a <c>GetAll(T)</c> that returns a value not implementing
    /// <see cref="System.Collections.Generic.IList{T}"/>, or a <c>GetCount(T)</c>, <c>Update(T)</c> or
    /// <c>Delete(T)</c> that does not return an <see cref="int"/>. Earlier versions of this library silently
    /// produced <c>null</c> in the collection case, masking the defect at the point of failure and surfacing it
    /// later as an unexplained null reference; the mismatch is now reported directly.
    /// </para>
    /// <para>
    /// An exception thrown by the body of a derived method that was successfully located and invoked is not a
    /// convention failure and is not represented by this type; that failure propagates from the derived
    /// implementation on its own terms.
    /// </para>
    /// </remarks>
    public class DataAccessConventionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessConventionException"/> class with a
        /// system-supplied message describing the error.
        /// </summary>
        /// <remarks>
        /// The library itself always supplies a message identifying the offending method, property, or type, so
        /// this constructor is provided for completeness and for consumers who derive from or rethrow this type.
        /// <see cref="Exception.InnerException"/> is <c>null</c>.
        /// </remarks>
        public DataAccessConventionException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessConventionException"/> class with a specified
        /// message describing the convention that was violated.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error. Expected to name the entity type and the method or property that
        /// was missing or mismatched, so the reader can correct the derived class without a debugger. May be
        /// <c>null</c> or empty, in which case <see cref="Exception.Message"/> falls back to the default
        /// system-supplied text.
        /// </param>
        /// <remarks>
        /// <see cref="Exception.InnerException"/> is <c>null</c>.
        /// </remarks>
        public DataAccessConventionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataAccessConventionException"/> class with a specified
        /// message and a reference to the exception that caused it.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error. May be <c>null</c> or empty, in which case
        /// <see cref="Exception.Message"/> falls back to the default system-supplied text.
        /// </param>
        /// <param name="innerException">
        /// The exception that caused this one — typically a reflection or invocation failure encountered while
        /// locating or calling the derived method. May be <c>null</c>, which is equivalent to supplying no inner
        /// exception.
        /// </param>
        /// <remarks>
        /// The supplied <paramref name="innerException"/> is exposed unchanged through
        /// <see cref="Exception.InnerException"/>.
        /// </remarks>
        public DataAccessConventionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
