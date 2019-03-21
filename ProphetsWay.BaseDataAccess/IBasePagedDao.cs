using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    public interface IBasePagedDao<T> : IBaseDao<T> where T: IBaseEntity
    {
        /// <summary>
        /// Will return a subset of all items from the database of the given type 'T',
        /// the parameter object just needs to be an instance of itself, no values need to be established
        /// The reason for the parameter and not just 'T' as a generic, 
        /// is to identify which specific Dao/BaseEntity GetAll is being called
        /// </summary>
        /// <param name="item">A Typed object to specify which DAO this call belongs to.</param>
        /// <param name="skip">The amount of records to skip.</param>
        /// <param name="take">The amount of records to take.</param>
        IList<T> GetPaged(T item, int skip, int take);
    }
}
