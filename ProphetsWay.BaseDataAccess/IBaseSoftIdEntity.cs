namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An entity that is both identified by an <c>Id</c> property and soft deleted by date.
    /// </summary>
    /// <typeparam name="T">The type of the identifier, such as <c>Guid</c>, <c>int</c> or <c>long</c>.</typeparam>
    /// <remarks>
    /// A convenience composition of <see cref="IBaseSoftEntity"/> and <see cref="IBaseIdEntity{T}"/> that adds
    /// no members and no behavior of its own.
    /// </remarks>
    public interface IBaseSoftIdEntity<T> : IBaseSoftEntity, IBaseIdEntity<T> 
    {

    }
}
