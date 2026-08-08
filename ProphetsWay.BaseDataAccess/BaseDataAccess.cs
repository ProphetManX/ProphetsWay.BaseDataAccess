using System;
using System.Collections.Generic;

namespace ProphetsWay.BaseDataAccess
{
    /// <summary>
    /// Utilizes Reflection to identify which methods to call, if you prefer to manually check for the sake of speed, do not inherit this class
    /// </summary>
    /// <remarks>
    /// The convention each member dispatches through - the method name and signature it looks for, the
    /// visibility that method must have, the return type it must declare, and how the identifier property is
    /// resolved for <see cref="Get{T}(object)"/> - is specified in full on
    /// <see cref="DataAccessConventionException"/>, the exception thrown when a derived class does not satisfy
    /// it. Read that type before writing a class that inherits this one.
    /// </remarks>
    public abstract class BaseDataAccess : IBaseDataAccess
    {
        /// <summary>
        /// Dispatches to the public instance method <c>GetAll(T)</c> on the derived class, invoked with a
        /// <c>null</c> entity argument that exists only to disambiguate the overload by entity type.
        /// </summary>
        /// <remarks>
        /// The derived method must declare a return type assignable to <see cref="IList{T}"/>. Because an array
        /// qualifies while remaining fixed size, treat the returned collection as read-only: <c>Add</c>,
        /// <c>Remove</c>, <c>Insert</c> and <c>Clear</c> are permitted by the static type but are not promised by
        /// the convention. A <c>null</c> result means the Data Access Layer produced no collection and is
        /// forwarded to the caller untouched.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>GetAll(T)</c> exists, or it declares an incompatible return type.
        /// </exception>
        public virtual IList<T> GetAll<T>() where T : IBaseEntity
        {
            var mtd = this.GetMethodByNameForType<T>("GetAll", typeof(IList<T>));
            return (IList<T>)mtd.InvokeUnwrapped(this, new object[] { null });
        }

        /// <summary>
        /// Dispatches to the public instance method <c>GetPaged(T, int, int)</c> on the derived class, invoked
        /// with a <c>null</c> entity argument followed by <paramref name="skip"/> and <paramref name="take"/>.
        /// </summary>
        /// <remarks>
        /// The derived method must declare a return type assignable to <see cref="IList{T}"/>. Because an array
        /// qualifies while remaining fixed size, treat the returned collection as read-only: <c>Add</c>,
        /// <c>Remove</c>, <c>Insert</c> and <c>Clear</c> are permitted by the static type but are not promised by
        /// the convention. A <c>null</c> result is forwarded to the caller untouched.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>GetPaged(T, int, int)</c> exists, or it declares an incompatible return
        /// type.
        /// </exception>
        public virtual IList<T> GetPaged<T>(int skip, int take) where T : IBaseEntity
        {
            var tType = typeof(T);
            var iType = typeof(int);
            var mtd = this.GetMethodByNameForType<T>("GetPaged", typeof(IList<T>), new[] { tType, iType, iType });
            return (IList<T>)mtd.InvokeUnwrapped(this, new object[] { null, skip, take });
        }

        /// <summary>
        /// Dispatches to the public instance method <c>GetCount(T)</c> on the derived class, which must declare
        /// a return type of <see cref="int"/>.
        /// </summary>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>GetCount(T)</c> exists, or it declares a return type other than
        /// <see cref="int"/>.
        /// </exception>
        public virtual int GetCount<T>() where T : IBaseEntity
        {
            var mtd = this.GetMethodByNameForType<T>("GetCount", typeof(int));
            return (int)mtd.InvokeUnwrapped(this, new object[] { null });
        }

        public abstract void TransactionCommit();

        public abstract void TransactionRollBack();

        public abstract void TransactionStart();

        /// <summary>
        /// Assumes that your ID property on your entities is either named "Id" or "EntityTypeNameId".  A probe
        /// entity is constructed, that property is assigned <paramref name="id"/>, and the public instance
        /// method <c>Get(T)</c> on the derived class is invoked with it.
        /// </summary>
        /// <remarks>
        /// The derived method must declare a return type of <typeparamref name="T"/> or a subclass of it. A
        /// <c>null</c> result means no such row exists and is forwarded to the caller untouched.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Get(T)</c> exists, it declares an incompatible return type, or
        /// <typeparamref name="T"/> exposes neither a <c>{TypeName}Id</c> nor an <c>Id</c> property.
        /// </exception>
        public virtual T Get<T>(object id) where T : IBaseEntity, new()
        {
            return this.GetMethodFindAndSetIdPropertyAndInvoke<T>(id);
        }

        /// <summary>
        /// Dispatches to the public instance method <c>Insert(T)</c> on the derived class.
        /// </summary>
        /// <remarks>
        /// The declared return type is unconstrained, including <c>void</c>; whatever the derived method returns
        /// is discarded.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Insert(T)</c> exists.
        /// </exception>
        public virtual void Insert<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Insert", null);
            mtd.InvokeUnwrapped(this, new object[] { item });
        }

        /// <summary>
        /// Dispatches to the public instance method <c>Update(T)</c> on the derived class, which must declare a
        /// return type of <see cref="int"/>.
        /// </summary>
        /// <remarks>
        /// The return type is validated before the method is invoked, so a mis-declared <c>Update</c> cannot
        /// write to the database and only then report the defect.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Update(T)</c> exists, or it declares a return type other than
        /// <see cref="int"/>.
        /// </exception>
        public virtual int Update<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Update", typeof(int));
            return (int)mtd.InvokeUnwrapped(this, new object[] { item });
        }

        /// <summary>
        /// Dispatches to the public instance method <c>Delete(T)</c> on the derived class, which must declare a
        /// return type of <see cref="int"/>.
        /// </summary>
        /// <remarks>
        /// The return type is validated before the method is invoked, so a mis-declared <c>Delete</c> cannot
        /// write to the database and only then report the defect.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Delete(T)</c> exists, or it declares a return type other than
        /// <see cref="int"/>.
        /// </exception>
        public virtual int Delete<T>(T item) where T : IBaseEntity, new()
        {
            var mtd = this.GetMethodByNameForType<T>("Delete", typeof(int));
            return (int)mtd.InvokeUnwrapped(this, new object[] { item });
        }
    }
}
