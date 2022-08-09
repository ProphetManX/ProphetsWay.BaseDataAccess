using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// Utilizes Reflection to identify which methods to call, if you prefer to manually check for the sake of speed, do not inherit this class
    /// </summary>
    public abstract class BaseDataAccess : IBaseDataAccess
    {
        public virtual IList<T> GetAll<T>() where T : IBaseEntity
        {
            var mtd = this.GetMethodByNameForType<T>("GetAll");
            return mtd.Invoke(this, new object[] { null }) as IList<T>;
        }

        public virtual IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity
        {
            var tType = typeof(T);
            var iType = typeof(int);
            var mtd = this.GetMethodByNameForType<T>("GetPaged", new[] { tType, iType, iType });
            return mtd.Invoke(this, new object[] { null, skip, take }) as IList<T>;
        }

        public virtual int GetCount<T>() where T : IBaseEntity
        {
            var mtd = this.GetMethodByNameForType<T>("GetCount");
            return (int)mtd.Invoke(this, new object[] { null });
        }

        public abstract void TransactionCommit();

        public abstract void TransactionRollBack();

        public abstract void TransactionStart();

        /// <summary>
        /// Assumes that your ID property on your entities is either named "Id" or "EntityTypeNameId"
        /// </summary>
        public virtual T Get<T>(object id) where T : IBaseEntity, new()
        {
            return this.GetMethodFindAndSetIdPropertyAndInvoke<T>(id);
        }

        public virtual void Insert<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Insert");
            mtd.Invoke(this, new object[] { item });
        }

        public virtual int Update<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Update");
            return (int)mtd.Invoke(this, new object[] { item });
        }

        public virtual int Delete<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Delete");
            return (int)mtd.Invoke(this, new object[] { item });
        }
    }

    [Obsolete("You no longer need to use this Generic type of BaseDataAccess, you can use the normal BaseDataAccess. (just remove the generic assignment)", false)]    
    public abstract class BaseDataAccess<TIdType> : BaseDataAccess, IBaseDataAccess<TIdType>
    {

    }
}
