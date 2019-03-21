namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An interface to identify an entity that is meant to be used as a 
    /// reference value/option in the context of a "Drop Down List" style situation,
    /// it should be assumed that you will generally want to get all of these items from the database at once
    /// </summary>
	public interface IBaseDDLEntity : IBaseEntity
	{
		 
	}
}