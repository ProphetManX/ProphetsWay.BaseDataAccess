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
		T Id { get; set; }
	}
}
