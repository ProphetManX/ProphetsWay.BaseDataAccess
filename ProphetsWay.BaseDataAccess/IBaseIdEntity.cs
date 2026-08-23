namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// An entity that carries a single identifier property named <c>Id</c>.
	/// </summary>
	/// <typeparam name="T">The type of the identifier, such as <c>Guid</c>, <c>int</c> or <c>long</c>.</typeparam>
	/// <remarks>
	/// <para>
	/// This interface describes the shape of the entity and nothing more. <see cref="BaseDataAccess.Get{T}"/>
	/// does not key on it: that member resolves the identifier property <b>by name</b>, preferring
	/// <c>{TypeName}Id</c> and falling back to <c>Id</c>. An entity that exposes a <c>{TypeName}Id</c> property
	/// has that one used instead of the <c>Id</c> declared here.
	/// </para>
	/// <para>
	/// <b>Implementing this interface is not by itself sufficient.</b> It plays no runtime role in identifier
	/// resolution, which considers only the name and the visibility of the property. An <b>explicit</b>
	/// implementation — <c>int IBaseIdEntity&lt;int&gt;.Id { get; set; }</c> — declares a non-public property
	/// whose reflected name is the interface-qualified form, so neither lookup finds it and
	/// <see cref="BaseDataAccess.Get{T}"/> throws <see cref="DataAccessConventionException"/> before dispatching.
	/// Declare the property as an ordinary public member.
	/// </para>
	/// </remarks>
	public interface IBaseIdEntity<T> : IBaseEntity
	{
		/// <summary>
		/// The identifier value of the entity.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <b>Annotated as nullable because an entity legitimately carries no identifier yet.</b> An entity
		/// implements this as a settable property with no initializer, so from the moment it is constructed
		/// until something assigns it, <c>Id</c> holds <c>default(T)</c> - <c>null</c> for a reference-type
		/// identifier, zero for an <c>int</c> or a <c>long</c>, the empty <c>Guid</c> for a <c>Guid</c>. That
		/// is true <b>regardless of the identifier type</b> and it lasts for the whole of the entity's
		/// pre-insert life, which normally ends when the store assigns a value on insert. A contract claiming
		/// the property always holds a real identifier would therefore be false for every newly constructed
		/// entity, whatever <typeparamref name="T"/> is.
		/// </para>
		/// <para>
		/// <b>Read the <c>?</c> as "may be default", not as "may be null".</b> <typeparamref name="T"/> is
		/// unconstrained, so for the <c>int</c>, <c>long</c> and <c>Guid</c> identifiers this interface is
		/// usually closed over the annotation resolves to <c>default(T)</c> - zero, or the empty
		/// <c>Guid</c> - and not to <c>null</c>. It does <b>not</b> turn the property into
		/// <c>Nullable&lt;T&gt;</c>: <c>IBaseIdEntity&lt;int&gt;.Id</c> remains an <c>int</c> at runtime, so
		/// this is metadata only and breaks no compiled consumer. <b>For a value-type identifier the annotation
		/// is inert:</b> it raises no diagnostic against any implementation and changes nothing about how the
		/// property behaves or how a caller must treat it.
		/// </para>
		/// <para>
		/// One source-level consequence is worth knowing before it is met. Because a property setter takes
		/// its value as a parameter, nullability runs the opposite way there from a getter: an implementation
		/// declaring a non-nullable <c>Id</c> under a nullable-enabled compilation is stricter than this
		/// interface allows and raises <c>CS8767</c> on the setter. Declaring the property as <c>T?</c>
		/// matches the contract and silences it.
		/// </para>
		/// </remarks>
		T? Id { get; set; }
	}
}
