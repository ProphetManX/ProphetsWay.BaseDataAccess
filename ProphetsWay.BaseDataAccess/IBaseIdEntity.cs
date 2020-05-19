namespace ProphetsWay.BaseDataAccess
{
	public interface IBaseIdEntity<T> : IBaseEntity
	{
		T Id { get; set; }
	}
}
