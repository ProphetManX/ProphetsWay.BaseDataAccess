namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// An entity that carries a single identifier property named <c>Id</c>.
	/// </summary>
	/// <typeparam name="T">The type of the identifier, such as <c>Guid</c>, <c>int</c> or <c>long</c>.</typeparam>
	/// <remarks>
	/// This interface describes the shape of the entity and nothing more. <see cref="BaseDataAccess.Get{T}"/>
	/// does not key on it: that member resolves the identifier property <b>by name</b>, preferring
	/// <c>{TypeName}Id</c> and falling back to <c>Id</c>. Implementing this interface satisfies the fallback,
	/// but an entity that also exposes a <c>{TypeName}Id</c> property has that one used instead.
	/// </remarks>
	public interface IBaseIdEntity<T> : IBaseEntity
	{
		T Id { get; set; }
	}
}
