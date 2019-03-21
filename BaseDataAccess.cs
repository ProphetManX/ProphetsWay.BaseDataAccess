﻿using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    public abstract class BaseDataAccess : IBaseDataAccess
    {
        public IList<T> GetAll<T>() where T : IBaseDDLEntity, new()
        {
            var tType = typeof(T);
            var mtd = GetType().GetMethod("GetAll", new[] { tType });

            if (mtd == null)
                throw new Exception($"Unable to find a 'GetAll' method for the type [{typeof(T).Name}] specified.");

            var input = new T();

            return mtd.Invoke(this, new object[] { input }) as IList<T>;
        }

        public IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity, new()
        {
            var tType = typeof(T);
            var mtd = GetType().GetMethod("GetPaged", new[] { tType });

            if (mtd == null)
                throw new Exception($"Unable to find a 'GetPaged' method for the type [{typeof(T).Name}] specified.");

            var input = new T();

            return mtd.Invoke(this, new object[] { input, skip, take }) as IList<T>;
        }

        public abstract void TransactionCommit();

        public abstract void TransactionRollBack();

        public abstract void TransactionStart();
    }

    public abstract class BaseDataAccessInt : BaseDataAccess, IBaseDataAccessInt
    {
        /// <summary>
        /// Assumes that your ID property on your entities is either named "Id" or "EntityTypeNameId"
        /// </summary>
        public T Get<T>(int id) where T : IBaseEntity, new()
        {
            var tType = typeof(T);
            var mtd = GetType().GetMethod("Get", new[] { tType });

            if (mtd == null)
                throw new Exception($"Unable to find a 'Get' method for the type [{typeof(T).Name}] specified.");

            var prop = tType.GetProperty($"{tType.Name}Id") ?? tType.GetProperty("Id");

            if (prop == null)
                throw new Exception($"Unable to find the 'Id' field on this type of object:  {typeof(T).Name}");

            var input = new T();
            prop.SetValue(input, id, null);

            return (T)mtd.Invoke(this, new object[] { input });
        }
    }

    public abstract class BaseDataAccessLong : BaseDataAccess, IBaseDataAccessLong
    {
        /// <summary>
        /// Assumes that your ID property on your entities is either named "Id" or "EntityTypeNameId"
        /// </summary>
        public T Get<T>(long id) where T : IBaseEntity, new()
        {
            var tType = typeof(T);
            var mtd = GetType().GetMethod("Get", new[] { tType });

            if (mtd == null)
                throw new Exception($"Unable to find a 'Get' method for the type [{typeof(T).Name}] specified.");

            var prop = tType.GetProperty($"{tType.Name}Id") ?? tType.GetProperty("Id");

            if (prop == null)
                throw new Exception($"Unable to find the 'Id' field on this type of object:  {typeof(T).Name}");

            var input = new T();
            prop.SetValue(input, id, null);

            return (T)mtd.Invoke(this, new object[] { input });
        }
    }
}