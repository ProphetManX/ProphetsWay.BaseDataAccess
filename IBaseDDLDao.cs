namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An interface to require that any "DropDown" item types will automatically have a "GetAll" method in their Dao
    /// </summary>
    /// <typeparam name="TDropDownEntityType"></typeparam>
    public interface IBaseDDLDao<TDropDownEntityType> : IBaseGetAllDao<TDropDownEntityType> where TDropDownEntityType : IBaseDDLEntity
    {

    }
}
