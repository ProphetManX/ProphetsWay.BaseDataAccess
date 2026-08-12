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
        /// <para>
        /// The derived method must declare a return type assignable to <see cref="IList{T}"/>. Because an array
        /// qualifies while remaining fixed size, treat the returned collection as read-only: <c>Add</c>,
        /// <c>Remove</c>, <c>Insert</c> and <c>Clear</c> are permitted by the static type but are not promised by
        /// the convention. A <c>null</c> result means the Data Access Layer produced no collection and is
        /// forwarded to the caller untouched. That holds for every entity type: <see cref="IList{T}"/> is a
        /// reference type whether or not <typeparamref name="T"/> is.
        /// </para>
        /// <para>
        /// The entity argument is only ever <c>null</c> as written here. When <typeparamref name="T"/> is a value
        /// type the reflection layer materializes that <c>null</c> as <c>default(T)</c>, so a <c>struct</c>
        /// entity reaches the derived method zero-initialized rather than null. Either way the argument selects
        /// the overload and is not expected to be read.
        /// </para>
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
        /// <para>
        /// The derived method must declare a return type assignable to <see cref="IList{T}"/>. Because an array
        /// qualifies while remaining fixed size, treat the returned collection as read-only: <c>Add</c>,
        /// <c>Remove</c>, <c>Insert</c> and <c>Clear</c> are permitted by the static type but are not promised by
        /// the convention. A <c>null</c> result is forwarded to the caller untouched, for every entity type:
        /// <see cref="IList{T}"/> is a reference type whether or not <typeparamref name="T"/> is.
        /// </para>
        /// <para>
        /// The entity argument is only ever <c>null</c> as written here. When <typeparamref name="T"/> is a value
        /// type the reflection layer materializes that <c>null</c> as <c>default(T)</c>, so a <c>struct</c>
        /// entity reaches the derived method zero-initialized rather than null. Either way the argument selects
        /// the overload and is not expected to be read.
        /// </para>
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
        /// <remarks>
        /// The entity argument is only ever <c>null</c> as written here. When <typeparamref name="T"/> is a value
        /// type the reflection layer materializes that <c>null</c> as <c>default(T)</c>, so a <c>struct</c>
        /// entity reaches the derived method zero-initialized rather than null. Either way the argument selects
        /// the overload and is not expected to be read.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>GetCount(T)</c> exists, or it declares a return type other than
        /// <see cref="int"/>.
        /// </exception>
        public virtual int GetCount<T>() where T : IBaseEntity
        {
            var mtd = this.GetMethodByNameForType<T>("GetCount", typeof(int));
            return (int)mtd.InvokeUnwrapped(this, new object[] { null });
        }

        /// <inheritdoc cref="IBaseDataAccess.TransactionCommit"/>
        public abstract void TransactionCommit();

        /// <inheritdoc cref="IBaseDataAccess.TransactionRollBack"/>
        public abstract void TransactionRollBack();

        /// <inheritdoc cref="IBaseDataAccess.TransactionStart"/>
        public abstract void TransactionStart();

        /// <summary>
        /// Releases the resources the derived Data Access Layer holds. Abstract because this class dispatches by
        /// reflection and owns no connection, context or transaction state of its own; the binding disposal rules
        /// are specified on <see cref="IBaseDataAccess"/>.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Assumes that your ID property on your entities is either named "Id" or "EntityTypeNameId".  A probe
        /// entity is constructed, that property is assigned <paramref name="id"/>, and the public instance
        /// method <c>Get(T)</c> on the derived class is invoked with it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The derived method must declare a return type of <typeparamref name="T"/> or a subclass of it. When
        /// <typeparamref name="T"/> is a <b>reference type</b>, a <c>null</c> result means no such row exists and
        /// is forwarded to the caller untouched.
        /// </para>
        /// <para>
        /// <b>A value-type entity cannot report "not found" as <c>null</c>.</b> The constraint on
        /// <typeparamref name="T"/> is satisfied by a <c>struct</c> as readily as by a <c>class</c>, and for a
        /// value-type entity <c>null</c> is simply not representable in the return type — the derived <c>Get</c>
        /// must declare a return type assignable to <typeparamref name="T"/>, which for a value type admits only
        /// <typeparamref name="T"/> itself. A Data Access Layer keying on a value-type entity must therefore
        /// signal a miss some other way: return a recognizable default or sentinel value that the caller checks
        /// for, expose the lookup through a member outside <see cref="IBaseDataAccess"/> that can express
        /// absence, or model the entity as a reference type so that <c>null</c> is available. Nothing here
        /// distinguishes "found the default value" from "found nothing" for a value-type entity; a design
        /// needing that distinction should not put the entity in a <c>struct</c>.
        /// </para>
        /// <para>
        /// The identifier property is resolved <b>by name</b> only — <c>{TypeName}Id</c> first, falling back to
        /// <c>Id</c> — and must have a set accessor. That accessor <b>need not be public</b>: a
        /// <c>private set</c>, <c>protected set</c>, <c>internal set</c> or <c>init</c> is resolved and invoked
        /// by reflection exactly as a public one is, and an entity hiding its identifier setter that way works
        /// correctly rather than failing. That is the opposite of the visibility rule governing the method
        /// lookup, where a non-public <c>Get(T)</c> is invisible and fails as though it had never been written.
        /// Only the complete absence of a set accessor is a failure.
        /// </para>
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Get(T)</c> exists, it declares an incompatible return type,
        /// <typeparamref name="T"/> exposes neither a <c>{TypeName}Id</c> nor an <c>Id</c> property, or the
        /// property it does expose has no set accessor at all.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="id"/> is of a type the identifier property cannot hold, which includes <c>null</c>
        /// when that property is a non-nullable value type. This is a caller error rather than a wiring error,
        /// and is deliberately not a <see cref="DataAccessConventionException"/>.
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
