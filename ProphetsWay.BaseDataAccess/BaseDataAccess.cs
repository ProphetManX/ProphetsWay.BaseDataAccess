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
        /// the convention. The return type is non-nullable, so a <c>null</c> result is a defect in the derived
        /// Data Access Layer rather than a permitted way of reporting "none" — and it is an unpoliced one: the
        /// convention check examines the derived method's <i>declared</i> return type and never inspects the value
        /// it returns, so a <c>null</c> is forwarded to the caller untouched. That holds for every entity type:
        /// <see cref="IList{T}"/> is a reference type whether or not <typeparamref name="T"/> is.
        /// </para>
        /// <para>
        /// <b>On this class specifically, that makes the non-null annotation an assertion made on the derived
        /// Data Access Layer's behalf rather than a promise this class can keep.</b> It states an obligation and
        /// does not verify that the obligation was met - this member returns whatever the derived method
        /// returned. The gap is accepted deliberately rather than overlooked: a guard here would be a runtime
        /// check inside a member whose entire job is to forward, and it was decided against. Read the annotation
        /// accordingly - a caller relying on it is relying on the derived Data Access Layer's conformance, not on
        /// anything this class enforces.
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
        /// the convention. The return type is non-nullable, so a <c>null</c> result is a defect in the derived
        /// Data Access Layer rather than a permitted way of reporting "none" — and it is an unpoliced one: the
        /// convention check examines the derived method's <i>declared</i> return type and never inspects the value
        /// it returns, so a <c>null</c> is forwarded to the caller untouched. That holds for every entity type:
        /// <see cref="IList{T}"/> is a reference type whether or not <typeparamref name="T"/> is.
        /// </para>
        /// <para>
        /// <b>On this class specifically, that makes the non-null annotation an assertion made on the derived
        /// Data Access Layer's behalf rather than a promise this class can keep.</b> It states an obligation and
        /// does not verify that the obligation was met - this member returns whatever the derived method
        /// returned. The gap is accepted deliberately rather than overlooked: a guard here would be a runtime
        /// check inside a member whose entire job is to forward, and it was decided against. Read the annotation
        /// accordingly - a caller relying on it is relying on the derived Data Access Layer's conformance, not on
        /// anything this class enforces.
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
        /// Only the complete absence of a set accessor is a failure. That leniency applies to the <i>accessor</i>
        /// only: the property declaration itself must be public to be found at all, so an <b>explicit</b>
        /// implementation of <see cref="IBaseIdEntity{T}"/> — non-public and reflected under its
        /// interface-qualified name — is matched by neither lookup and fails as though it had not been written.
        /// </para>
        /// <para>
        /// <b>Overriding this member needs one piece of syntax that is not obvious.</b> An override may not
        /// restate the interface constraint on <typeparamref name="T"/>, so <c>T?</c> is read there as
        /// <c>Nullable&lt;T&gt;</c> unless the <c>default</c> constraint says otherwise. Without it the compiler
        /// reports <c>CS0453</c> — <typeparamref name="T"/> "must be a non-nullable value type" to serve as the
        /// argument to <c>Nullable&lt;T&gt;</c>, a type the developer never wrote and has no reason to expect the
        /// mention of — and then <c>CS0508</c> for the return type that consequently no longer matches. Naming
        /// the constraint fixes both. It is C# 9 or later, and it exists for exactly this case - an override, or
        /// an explicit interface implementation, that needs to say <typeparamref name="T"/> is not constrained
        /// to a value type:
        /// <code>
        /// public override T? Get&lt;T&gt;(object? id) where T : default =&gt; default;
        /// </code>
        /// </para>
        /// <para>
        /// <b>An existing override is not disturbed by any of this.</b> One written in a null-oblivious file -
        /// no <c>#nullable enable</c>, no project-wide setting - still compiles clean against the older
        /// <c>T Get&lt;T&gt;(object id)</c> shape, which is why this reaches only someone modernizing. What that
        /// someone meets arrives in three steps, and the first two differ in <b>severity</b>:
        /// </para>
        /// <para>
        /// <b>1. Enabling nullable over the override left unchanged produces two <i>warnings</i>, and the build
        /// still succeeds.</b> <c>CS8765</c> is the parameter - <c>object id</c> declared where this member says
        /// <c>object? id</c> is stricter than the base allows; it is the override-side counterpart of the
        /// <c>CS8767</c> the type selectors on <see cref="IBaseGetAllDao{T}"/> and
        /// <see cref="IBasePagedDao{T}"/> already describe. <c>CS8603</c> is "possible null reference return",
        /// and it is the diagnostic that pushes a developer toward a <c>T?</c> return type in the first place.
        /// Nothing has broken yet, and neither <c>CS0453</c> nor <c>CS0508</c> appears at this point.
        /// </para>
        /// <para>
        /// <b>2. Following <c>CS8603</c> to a <c>T?</c> return type while omitting the <c>default</c> constraint
        /// is what produces <i>errors</i> and fails the build.</b> <c>CS0453</c> and <c>CS0508</c> belong to this
        /// step and not to the one before it - they are what an incorrect first attempt at the fix produces,
        /// rather than what modernizing produces. They are named here, and in the paragraph above, in the order
        /// one causes the other; MSBuild prints them the other way round, <c>CS0508</c> first.
        /// </para>
        /// <para>
        /// <b>3. The form that compiles clean is the whole signature shown above</b> - the <c>T?</c> return, the
        /// <c>object? id</c> parameter and <c>where T : default</c> together. The constraint clears
        /// <c>CS0453</c> and <c>CS0508</c>; restating <c>object? id</c> clears <c>CS8765</c>.
        /// </para>
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance <c>Get(T)</c> exists, it declares an incompatible return type,
        /// <typeparamref name="T"/> exposes neither a <c>{TypeName}Id</c> nor an <c>Id</c> property, exposes one
        /// that is not public, or exposes one with no set accessor at all.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="id"/> is of a type the identifier property cannot hold, which includes <c>null</c>
        /// when that property is a non-nullable value type. This is a caller error rather than a wiring error,
        /// and is deliberately not a <see cref="DataAccessConventionException"/>.
        /// </exception>
        public virtual T? Get<T>(object? id) where T : IBaseEntity, new()
        {
            return this.GetMethodFindAndSetIdPropertyAndInvoke<T>(id);
        }

        /// <summary>
        /// Dispatches to the public instance method <c>Insert(T)</c> on the derived class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The declared return type is unconstrained, including <c>void</c>; whatever the derived method returns
        /// is discarded.
        /// </para>
        /// <para>
        /// <c>null</c> is not a legal <paramref name="item"/> - the plain <typeparamref name="T"/> asserts it,
        /// as <see cref="IBaseDataAccess.Insert{TEntityType}"/> sets out - and <b>this class does not check</b>.
        /// A <c>null</c> reaches the derived method exactly as it arrived; what happens to it there belongs to
        /// the derived Data Access Layer, not to this class.
        /// </para>
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
        /// <para>
        /// The return type is validated before the method is invoked, so a mis-declared <c>Update</c> cannot
        /// write to the database and only then report the defect.
        /// </para>
        /// <para>
        /// <c>null</c> is not a legal <paramref name="item"/> - the plain <typeparamref name="T"/> asserts it,
        /// as <see cref="IBaseDataAccess.Update{TEntityType}"/> sets out - and <b>this class does not check</b>.
        /// A <c>null</c> reaches the derived method exactly as it arrived; what happens to it there belongs to
        /// the derived Data Access Layer, not to this class.
        /// </para>
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
        /// <para>
        /// The return type is validated before the method is invoked, so a mis-declared <c>Delete</c> cannot
        /// write to the database and only then report the defect.
        /// </para>
        /// <para>
        /// <c>null</c> is not a legal <paramref name="item"/> - the plain <typeparamref name="T"/> asserts it,
        /// as <see cref="IBaseDataAccess.Delete{TEntityType}"/> sets out - and <b>this class does not check</b>.
        /// A <c>null</c> reaches the derived method exactly as it arrived; what happens to it there belongs to
        /// the derived Data Access Layer, not to this class.
        /// </para>
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
