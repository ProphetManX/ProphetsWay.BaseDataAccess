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
    /// or declares the wrong return type; or an entity type whose identifier property is missing or cannot be
    /// written to. It never indicates a runtime data condition: a missing row, an empty table, a null identifier,
    /// or a failed query will not produce it.
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
    /// <b>2. Identifier property not found, or found but not writable.</b> This applies to
    /// <c>Get&lt;T&gt;(object id)</c> only. A new instance of <c>T</c> is constructed and its identifier property is
    /// assigned the supplied <c>id</c> before the derived <c>Get(T)</c> method is invoked. The property is resolved
    /// by name: first <c>{TypeName}Id</c> — for an entity type named <c>Company</c> that is <c>CompanyId</c> —
    /// falling back to <c>Id</c>. Resolution is by name only; no attribute, base type, or interface member is
    /// consulted, and the property's type is not considered when matching. Three distinct failures are reported
    /// through this exception:
    /// <list type="bullet">
    /// <item><description>
    /// <b>Neither property exists.</b> <c>T</c> exposes no <c>{TypeName}Id</c> and no <c>Id</c>, so there is nothing
    /// to resolve and no identifier can be assigned.
    /// </description></item>
    /// <item><description>
    /// <b>The resolved property has no set accessor.</b> The name matched, but the property is get-only — declared
    /// with no <c>set</c> or <c>init</c> at all, as an expression-bodied member, or as a <c>readonly</c> computed
    /// value — so the identifier cannot be written to it.
    /// </description></item>
    /// <item><description>
    /// <b>The property exists and is writable, but is not public.</b> The lookup is restricted to public instance
    /// properties, so a <c>private</c>, <c>protected</c> or <c>internal</c> declaration is invisible to it and
    /// fails exactly as if it had not been written. The way this is reached in practice is an <b>explicit
    /// interface implementation</b> — <c>int IBaseIdEntity&lt;int&gt;.Id { get; set; }</c> — which is both
    /// non-public and reflected under its interface-qualified name, so neither the <c>{TypeName}Id</c> nor the
    /// <c>Id</c> lookup matches it. Implementing <see cref="IBaseIdEntity{T}"/> is therefore not by itself
    /// sufficient; the identifier must be declared as an ordinary public member.
    /// </description></item>
    /// </list>
    /// Only the complete absence of a set accessor is a failure. <b>A set accessor that exists but is not public is
    /// fully supported</b>: a <c>private set</c>, <c>protected set</c>, <c>internal set</c> or <c>init</c> accessor
    /// is resolved and invoked by reflection exactly as a public one is, and an entity that hides its identifier
    /// setter from ordinary callers in this way works correctly and does not produce this exception. That
    /// distinction is deliberate — the convention requires the identifier to be <i>assignable</i>, not to be
    /// <i>publicly</i> assignable — and it is the opposite of the visibility rule that governs circumstance 1,
    /// where a non-public method is invisible to the lookup. It applies to the <i>accessor</i> only: the property
    /// declaration itself must be public to be found at all, exactly as a derived method must be.
    /// </para>
    /// <para>
    /// The check on the identifier property is made before the probe entity is constructed, so like the return type
    /// check in circumstance 3 it costs nothing and runs nothing when it fails.
    /// </para>
    /// <para>
    /// No other member requires an identifier property. <c>GetAll&lt;T&gt;()</c>, <c>GetCount&lt;T&gt;()</c> and
    /// <c>GetPaged&lt;T&gt;(int, int)</c> pass <c>null</c> as the entity argument; they never construct a probe
    /// entity and never look for an identifier property. An entity type exposing neither <c>{TypeName}Id</c> nor
    /// <c>Id</c>, or exposing one of them get-only, works correctly with those three members and fails only on
    /// <c>Get&lt;T&gt;(object id)</c>.
    /// </para>
    /// <para>
    /// <b>3. Return type mismatch.</b> A matching derived method was found, but the return type it <i>declares</i>
    /// is not compatible with the return type the corresponding <see cref="BaseDataAccess"/> member declares. The
    /// check is made against the declared type before the method is invoked; the value the method would have
    /// returned is never examined, and its runtime type is irrelevant.
    /// <list type="table">
    /// <listheader><term>Base member</term><description>Required declared return type on the derived method</description></listheader>
    /// <item><term><c>GetAll&lt;T&gt;()</c></term><description>Assignable to <see cref="System.Collections.Generic.IList{T}"/> — <c>List&lt;T&gt;</c>, <c>T[]</c> and <c>Collection&lt;T&gt;</c> all qualify; <c>IEnumerable&lt;T&gt;</c> does not.</description></item>
    /// <item><term><c>GetPaged&lt;T&gt;(int, int)</c></term><description>Assignable to <see cref="System.Collections.Generic.IList{T}"/>, as above.</description></item>
    /// <item><term><c>GetCount&lt;T&gt;()</c></term><description><see cref="int"/>.</description></item>
    /// <item><term><c>Get&lt;T&gt;(object)</c></term><description><c>T</c>, or a subclass of <c>T</c>.</description></item>
    /// <item><term><c>Update&lt;T&gt;(T)</c></term><description><see cref="int"/>.</description></item>
    /// <item><term><c>Delete&lt;T&gt;(T)</c></term><description><see cref="int"/>.</description></item>
    /// <item><term><c>Insert&lt;T&gt;(T)</c></term><description>Unconstrained. The derived method may declare any return type, including <c>void</c>; whatever it returns is ignored entirely and this circumstance can never arise for <c>Insert</c>.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Because <c>T[]</c> satisfies <see cref="System.Collections.Generic.IList{T}"/> while remaining fixed-size,
    /// a collection obtained from <c>GetAll&lt;T&gt;()</c> or <c>GetPaged&lt;T&gt;(int, int)</c> should be treated
    /// as read-only: the static type permits <c>Add</c>, <c>Remove</c>, <c>Insert</c> and <c>Clear</c>, but the
    /// convention does not promise they are supported, and on an array-backed result they throw
    /// <see cref="NotSupportedException"/>.
    /// </para>
    /// <para>
    /// Two consequences follow from validating the declared type ahead of invocation. First, <b>a derived method
    /// that violates the rule is never called.</b> No query runs and no side effect occurs before the exception is
    /// thrown, which matters most for <c>Update</c> and <c>Delete</c>: a mis-declared method cannot write to the
    /// database and then fail.
    /// </para>
    /// <para>
    /// Second, <b>a <c>null</c> returned at runtime is never a convention failure.</b> It is forwarded to the
    /// caller untouched. A <c>Get(T)</c> returning <c>null</c> means no such row exists; a <c>GetAll(T)</c>
    /// returning <c>null</c> means the Data Access Layer produced no collection. Neither is a wiring error, and
    /// neither produces this exception. <c>GetCount</c>, <c>Update</c> and <c>Delete</c> cannot return <c>null</c>
    /// at all, because their declared return type is already constrained to <see cref="int"/>.
    /// </para>
    /// <para>
    /// An exception thrown by the body of a derived method that was successfully located and invoked is not a
    /// convention failure and is not represented by this type; that failure propagates from the derived
    /// implementation on its own terms. The same principle applies to the <c>new T()</c> call that
    /// <c>Get&lt;T&gt;(object id)</c> makes before dispatching: the <c>new()</c> generic constraint means the
    /// compiler has already guaranteed <c>T</c> has an accessible parameterless constructor, so the only runtime
    /// possibility is a constructor that exists but throws. Such an exception propagates to the caller unchanged
    /// and is never wrapped in a <see cref="DataAccessConventionException"/>.
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
