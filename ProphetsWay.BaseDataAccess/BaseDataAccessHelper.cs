using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace ProphetsWay.BaseDataAccess
{
    internal static class BaseDataAccessHelper
    {
        /// <summary>
        /// The convention requires the derived method to be part of the public surface of the Data Access Layer,
        /// so private, protected, internal and static methods are deliberately invisible to the lookup.
        /// </summary>
        private const BindingFlags ConventionMethodFlags = BindingFlags.Public | BindingFlags.Instance;

        /// <summary>
        /// The C# keyword for each type that has one, so a message reads as the signature the developer wrote
        /// rather than as the framework type name behind it.
        /// </summary>
        private static readonly Dictionary<Type, string> TypeKeywords = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(object), "object" },
            { typeof(string), "string" },
            { typeof(void), "void" }
        };

        /// <summary>
        /// Locates the public instance method the convention requires on the concrete Data Access Layer, and
        /// verifies the return type it declares before it is ever invoked.
        /// </summary>
        /// <param name="instance">The Data Access Layer to search.</param>
        /// <param name="methodName">The convention method name, such as <c>GetAll</c> or <c>Update</c>.</param>
        /// <param name="expectedReturnType">
        /// The type the declared return type must be assignable to, or <c>null</c> when the return value is
        /// discarded and the declared type is therefore unconstrained.
        /// </param>
        /// <param name="parameterTypes">
        /// The exact parameter types the method must declare, in order. Defaults to a single parameter of type
        /// <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="DataAccessConventionException">
        /// No matching public instance method exists, or the one that does declares an incompatible return type.
        /// </exception>
        public static MethodInfo GetMethodByNameForType<T>(this BaseDataAccess instance, string methodName, Type expectedReturnType, Type[] parameterTypes = null) where T : IBaseEntity
        {
            var entityType = typeof(T);
            var dalType = instance.GetType();
            var required = parameterTypes ?? new[] { entityType };

            var mtd = FindExactMatch(dalType, methodName, required);

            if (mtd == null)
                throw new DataAccessConventionException($"Unable to find a public instance method named '{methodName}' accepting ({DescribeParameters(required)}) on the data access type [{DescribeType(dalType)}], required for the entity type [{DescribeType(entityType)}].");

            if (expectedReturnType != null && !expectedReturnType.IsAssignableFrom(mtd.ReturnType))
                throw new DataAccessConventionException($"The method named '{methodName}' on the data access type [{DescribeType(dalType)}], required for the entity type [{DescribeType(entityType)}], declares a return type of [{DescribeType(mtd.ReturnType)}] which cannot be used as [{DescribeType(expectedReturnType)}].");

            return mtd;
        }

        /// <summary>
        /// Builds the probe entity carrying the identifier, then dispatches to the derived <c>Get</c> method.
        /// </summary>
        /// <remarks>
        /// The order of operations is load bearing. The method is located and its declared return type validated
        /// first, so a wiring error is reported without constructing an entity, and the identifier property is
        /// resolved - and confirmed writable - before the entity is constructed, so a property that cannot carry
        /// the identifier is reported the same way.
        /// </remarks>
        /// <exception cref="DataAccessConventionException">
        /// The derived <c>Get</c> method is missing or mis-declared, or <typeparamref name="T"/> exposes neither
        /// a <c>{TypeName}Id</c> nor an <c>Id</c> property, or the property it does expose has no set accessor.
        /// </exception>
        public static T GetMethodFindAndSetIdPropertyAndInvoke<T>(this BaseDataAccess instance, object id) where T : IBaseEntity, new()
        {
            var entityType = typeof(T);
            var mtd = instance.GetMethodByNameForType<T>("Get", entityType);

            var prop = entityType.GetProperty($"{entityType.Name}Id") ?? entityType.GetProperty("Id");

            if (prop == null)
                throw new DataAccessConventionException($"The entity type [{DescribeType(entityType)}] exposes neither a '{entityType.Name}Id' nor an 'Id' property, so no identifier can be assigned to it.");

            //CanWrite is true for a non-public set accessor, which reflection resolves and invokes happily, and
            //false only when there is no set accessor at all - which is the wiring error being reported here
            if (!prop.CanWrite)
                throw new DataAccessConventionException($"The entity type [{DescribeType(entityType)}] exposes an identifier property '{prop.Name}' with no set accessor, so no identifier can be assigned to it.");

            //boxed once, deliberately: a probe held in a variable typed T would be boxed separately by the
            //assignment and by the invocation, and a struct entity would reach the derived method unmodified
            object input = CreateEntity<T>();
            SetIdentifier(prop, input, id);

            return (T)mtd.InvokeUnwrapped(instance, new object[] { input });
        }

        /// <summary>
        /// Assigns the identifier and lets anything the setter threw reach the caller as its original type, with
        /// its original stack, rather than as a <see cref="TargetInvocationException"/> wrapper.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An identifier of a type the property cannot hold is the caller's mistake rather than a wiring error,
        /// so the <see cref="ArgumentException"/> the reflection layer raises for it is left to surface on its
        /// own terms and is deliberately not reinterpreted as a <see cref="DataAccessConventionException"/>.
        /// </para>
        /// <para>
        /// <c>null</c> is such an identifier whenever the property is a non-nullable value type, but the
        /// reflection layer is lenient about that one case alone and writes <c>default</c> instead of throwing,
        /// which would send a probe for identifier zero out as though the caller had asked for it. It is
        /// rejected here so that case is reported the same way every other unusable identifier already is. The
        /// test is against the property's type rather than the entity's, which is both the correct question to
        /// ask and what extends the same protection to a value-type entity without a second branch.
        /// </para>
        /// </remarks>
        private static void SetIdentifier(PropertyInfo property, object entity, object id)
        {
            var propertyType = property.PropertyType;

            if (id == null && propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null)
                throw new ArgumentException($"A null identifier cannot be assigned to the identifier property '{property.Name}' of type [{DescribeType(propertyType)}] on the entity type [{DescribeType(entity.GetType())}], because that type cannot hold null.");

            try
            {
                property.SetValue(entity, id, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        /// Invokes the located method and lets anything its body threw reach the caller as its original type,
        /// with its original stack, rather than as a <see cref="TargetInvocationException"/> wrapper.
        /// </summary>
        public static object InvokeUnwrapped(this MethodInfo method, BaseDataAccess instance, object[] arguments)
        {
            try
            {
                return method.Invoke(instance, arguments);
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        /// The <c>new()</c> constraint guarantees an accessible parameterless constructor, so the only failure
        /// possible here is a constructor that throws - and that must not surface wrapped either.
        /// </summary>
        /// <remarks>
        /// <c>new T()</c> compiles to <see cref="Activator.CreateInstance{T}()"/>, which wraps a throwing
        /// constructor in a <see cref="TargetInvocationException"/> on .NET Framework but rethrows it unwrapped
        /// on .NET Core and later. The catch below therefore only ever fires on <c>net48</c>; it is not dead
        /// code, and removing it because a modern target never reaches it silently regresses that target.
        /// </remarks>
        private static T CreateEntity<T>() where T : new()
        {
            try
            {
                return new T();
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw;
            }
        }

        /// <summary>
        /// Matches on exact positional parameter types rather than through <see cref="Type.DefaultBinder"/>,
        /// which would also accept a parameter typed as a base class or interface of the entity. Filtering the
        /// full method list rather than asking for one signature also avoids <see cref="AmbiguousMatchException"/>.
        /// </summary>
        /// <remarks>
        /// The hierarchy is walked one level at a time, most derived first, because a method hidden with
        /// <c>new</c> leaves two entries with the same name and the same parameter types on the same method
        /// list, and the order that list is returned in is unspecified. Taking each level on its own resolves
        /// the shadowed pair to the method a compile-time call on the same static type would bind to, and leaves
        /// plain inheritance - where only one declaration exists - reached exactly as before.
        /// </remarks>
        private static MethodInfo FindExactMatch(Type dalType, string methodName, Type[] required)
        {
            for (var level = dalType; level != null; level = level.BaseType)
            {
                foreach (var candidate in level.GetMethods(ConventionMethodFlags | BindingFlags.DeclaredOnly))
                {
                    //the generic members of BaseDataAccess itself are never the convention method being sought
                    if (candidate.IsGenericMethodDefinition)
                        continue;

                    if (!string.Equals(candidate.Name, methodName, StringComparison.Ordinal))
                        continue;

                    var parameters = candidate.GetParameters();

                    if (parameters.Length != required.Length)
                        continue;

                    var matched = true;

                    for (var i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].ParameterType == required[i])
                            continue;

                        matched = false;
                        break;
                    }

                    if (matched)
                        return candidate;
                }
            }

            return null;
        }

        private static string DescribeParameters(Type[] parameterTypes)
        {
            var names = new string[parameterTypes.Length];

            for (var i = 0; i < parameterTypes.Length; i++)
                names[i] = DescribeType(parameterTypes[i]);

            return string.Join(", ", names);
        }

        /// <summary>
        /// Renders a type the way it would have been written in source, so the message a reader is given matches
        /// the declaration they have to go and correct.
        /// </summary>
        /// <remarks>
        /// Every type named in a <see cref="DataAccessConventionException"/> goes through here, so a signature
        /// reads as <c>(Company, int, int)</c> rather than <c>(Company, Int32, Int32)</c> and a return type as
        /// <c>IList&lt;Company&gt;</c> rather than in namespace-qualified backtick-arity form. The message is the
        /// entire debugging experience for this exception, and a mismatch between what it prints and what the
        /// developer wrote is exactly the friction it exists to remove.
        /// </remarks>
        private static string DescribeType(Type type)
        {
            string keyword;

            if (TypeKeywords.TryGetValue(type, out keyword))
                return keyword;

            if (type.IsArray)
                return $"{DescribeType(type.GetElementType())}[]";

            var underlying = Nullable.GetUnderlyingType(type);

            if (underlying != null)
                return $"{DescribeType(underlying)}?";

            if (!type.IsGenericType)
                return type.Name;

            var name = type.Name;
            var arity = name.IndexOf('`');

            if (arity > 0)
                name = name.Substring(0, arity);

            var arguments = type.GetGenericArguments();
            var described = new string[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
                described[i] = DescribeType(arguments[i]);

            return $"{name}<{string.Join(", ", described)}>";
        }
    }
}
