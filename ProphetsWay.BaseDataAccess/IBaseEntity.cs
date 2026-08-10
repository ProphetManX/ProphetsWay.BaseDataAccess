namespace ProphetsWay.BaseDataAccess
{
	/// <summary>
	/// An interface to identify an entity to be used within the DAL project space
	/// </summary>
	/// <remarks>
	/// A marker with no members. It exists so the generic constraints across this library can name "an entity"
	/// without constraining its shape, and implementing it imposes no obligation beyond that.
	/// </remarks>
	public interface IBaseEntity
	{
		
	}
}