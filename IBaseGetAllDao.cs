using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// An interface to specify a particular entity should have a "GetAll" method
    /// </summary>
    public interface IBaseGetAllDao<T> : IBaseDao<T> where T : IBaseEntity
    {
        /// <summary>
        /// Will return all the items from the database of the given type 'T', 
        /// the parameter object just needs to be an instance of itself, no values need to be established
        /// The reason for the parameter and not just 'T' as a generic, 
        /// is to identify which specific Dao/BaseEntity GetAll is being called
        /// </summary>
        IList<T> GetAll(T item);
    }
}
