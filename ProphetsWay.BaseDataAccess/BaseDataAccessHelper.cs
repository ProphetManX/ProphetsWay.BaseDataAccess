using System;
using System.Reflection;

namespace ProphetsWay.BaseDataAccess
{
    internal static class BaseDataAccessHelper
    {
        public static MethodInfo GetMethodByNameForType<T>(this BaseDataAccess instance, string methodName, Type[] types = null) where T : IBaseEntity
        {
            var mtd = instance.GetType().GetMethod(methodName, types ?? new[] { typeof(T) });

            if (mtd == null)
                throw new Exception($"Unable to find a '{methodName}' method for the type [{typeof(T).Name}] specified.");

            return mtd;
        }

        public static T GetMethodFindAndSetIdPropertyAndInvoke<T>(this BaseDataAccess instance, object id) where T : IBaseEntity, new()
        {
            var mtd = instance.GetMethodByNameForType<T>("Get");

            var tType = typeof(T);
            var prop = tType.GetProperty($"{tType.Name}Id") ?? tType.GetProperty("Id");

            if (prop == null)
                throw new Exception($"Unable to find the 'Id' field on this type of object:  {typeof(T).Name}");

            var input = new T();
            prop.SetValue(input, id, null);

            return (T)mtd.Invoke(instance, new object[] { input });
        }
    }
}
