using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using Unity.Serialization;
using UnityEditor;

namespace Unity.Build
{
    /// <summary>
    /// Type construction utilities.
    /// </summary>
    public static class TypeConstructionUtility
    {
        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/>.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <returns>New instance of derived type.</returns>
        public static T ConstructTypeDerivedFrom<T>()
        {
            return TypeConstruction.Construct<T>(GetConstructibleTypesDerivedFrom<T>().FirstOrDefault());
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies the filtering function.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <returns>New instance of derived type.</returns>
        public static T ConstructTypeDerivedFrom<T>(Func<Type, bool> filter)
        {
            return TypeConstruction.Construct<T>(GetConstructibleTypesDerivedFrom<T>(filter).FirstOrDefault());
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies the condition function.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="condition">The condition function.</param>
        /// <returns>New instance of derived type.</returns>
        public static T ConstructTypeDerivedFrom<T>(Func<T, bool> condition)
        {
            return ConstructTypesDerivedFrom<T>().FirstOrDefault(condition);
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies both the filtering and condition functions.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <param name="condition">The condition function.</param>
        /// <returns>New instance of derived type.</returns>
        public static T ConstructTypeDerivedFrom<T>(Func<Type, bool> filter, Func<T, bool> condition)
        {
            return ConstructTypesDerivedFrom<T>(filter).FirstOrDefault(condition);
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/>.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="value">New instance of derived type.</param>
        /// <returns><see langword="true"/> if new instance was successfully constructed, <see langword="false"/> otherwise.</returns>
        public static bool TryConstructTypeDerivedFrom<T>(out T value)
        {
            var types = GetConstructibleTypesDerivedFrom<T>();
            if (!types.Any())
            {
                value = default;
                return false;
            }
            value = TypeConstruction.Construct<T>(types.FirstOrDefault());
            return true;
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies the filtering function.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <param name="value">New instance of derived type.</param>
        /// <returns><see langword="true"/> if new instance was successfully constructed, <see langword="false"/> otherwise.</returns>
        public static bool TryConstructTypeDerivedFrom<T>(Func<Type, bool> filter, out T value)
        {
            var types = GetConstructibleTypesDerivedFrom<T>(filter);
            if (!types.Any())
            {
                value = default;
                return false;
            }
            value = TypeConstruction.Construct<T>(types.FirstOrDefault());
            return true;
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies the condition function.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="condition">The condition function.</param>
        /// <param name="value">New instance of derived type.</param>
        /// <returns><see langword="true"/> if new instance was successfully constructed, <see langword="false"/> otherwise.</returns>
        public static bool TryConstructTypeDerivedFrom<T>(Func<T, bool> condition, out T value)
        {
            var instances = ConstructTypesDerivedFrom(condition);
            if (!instances.Any())
            {
                value = default;
                return false;
            }
            value = instances.FirstOrDefault();
            return true;
        }

        /// <summary>
        /// Construct instance of first type derived from <typeparamref name="T"/> that satisfies both the filtering and condition functions.
        /// Derived type must have an implicit, default or registered constructor.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <param name="condition">The condition function.</param>
        /// <param name="value">New instance of derived type.</param>
        /// <returns><see langword="true"/> if new instance was successfully constructed, <see langword="false"/> otherwise.</returns>
        public static bool TryConstructTypeDerivedFrom<T>(Func<Type, bool> filter, Func<T, bool> condition, out T value)
        {
            var instances = ConstructTypesDerivedFrom(filter, condition);
            if (!instances.Any())
            {
                value = default;
                return false;
            }
            value = instances.FirstOrDefault();
            return true;
        }

        /// <summary>
        /// Construct instance of all types derived from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <returns>Enumeration of constructed instance of all types derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> ConstructTypesDerivedFrom<T>()
        {
            return GetConstructibleTypesDerivedFrom<T>().Select(TypeConstruction.Construct<T>);
        }

        /// <summary>
        /// Construct instance of all types derived from <typeparamref name="T"/> that satisfies the filtering function.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <returns>Enumeration of constructed instance of all types derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> ConstructTypesDerivedFrom<T>(Func<Type, bool> filter)
        {
            return GetConstructibleTypesDerivedFrom<T>(filter).Select(TypeConstruction.Construct<T>);
        }

        /// <summary>
        /// Construct instance of all types derived from <typeparamref name="T"/> that satisfies the condition function.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="condition">The condition function.</param>
        /// <returns>Enumeration of constructed instance of all types derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> ConstructTypesDerivedFrom<T>(Func<T, bool> condition)
        {
            return ConstructTypesDerivedFrom<T>().Where(condition);
        }

        /// <summary>
        /// Construct instance of all types derived from <typeparamref name="T"/> that satisfies both the filtering and condition functions.
        /// </summary>
        /// <typeparam name="T">The derived type.</typeparam>
        /// <param name="filter">The filtering function.</param>
        /// <param name="condition">The condition function.</param>
        /// <returns>Enumeration of constructed instance of all types derived from <typeparamref name="T"/>.</returns>
        public static IEnumerable<T> ConstructTypesDerivedFrom<T>(Func<Type, bool> filter, Func<T, bool> condition)
        {
            return ConstructTypesDerivedFrom<T>(filter).Where(condition);
        }

        /// <summary>
        /// Construct instance of type resolved from assembly qualified type name, and return it as <typeparamref name="T"/>.
        /// Resolved type must have an implicit, default or registered constructor, and not have <see cref="ObsoleteAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The returned type.</typeparam>
        /// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
        /// <returns>New instance of type resolved from assembly qualified type name.</returns>
        public static T ConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName)
        {
            var type = GetTypeFromAssemblyQualifiedTypeName(assemblyQualifiedTypeName);
            return type != null ? TypeConstruction.Construct<T>(type) : default;
        }

        /// <summary>
        /// Construct instance of type resolved from assembly qualified type name, and return it as <typeparamref name="T"/>.
        /// Resolved type must have an implicit, default or registered constructor, and not have <see cref="ObsoleteAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The returned type.</typeparam>
        /// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
        /// <param name="value">New instance of type resolved from assembly qualified type name.</param>
        /// <returns><see langword="true"/> if type resolved from assembly qualified type name was successfully constructed, <see langword="false"/> otherwise.</returns>
        public static bool TryConstructFromAssemblyQualifiedTypeName<T>(string assemblyQualifiedTypeName, out T value)
        {
            try
            {
                var type = GetTypeFromAssemblyQualifiedTypeName(assemblyQualifiedTypeName);
                value = type != null ? TypeConstruction.Construct<T>(type) : throw null;
                return true;
            }
            catch
            {
                value = default;
                return false;
            }
        }

        static IEnumerable<Type> GetConstructibleTypesDerivedFrom<T>()
        {
            return TypeCache.GetTypesDerivedFrom<T>()
                .Where(type => !type.IsAbstract && !type.IsGenericType)
                .Where(type => TypeConstruction.CanBeConstructed(type));
        }

        static IEnumerable<Type> GetConstructibleTypesDerivedFrom<T>(Func<Type, bool> condition)
        {
            return GetConstructibleTypesDerivedFrom<T>().Where(condition);
        }

        static Type GetTypeFromAssemblyQualifiedTypeName(string assemblyQualifiedTypeName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedTypeName))
            {
                throw new ArgumentException(nameof(assemblyQualifiedTypeName));
            }

            var type = Type.GetType(assemblyQualifiedTypeName);
            if ((type == null || type.HasAttribute<ObsoleteAttribute>()) &&
                FormerNameAttribute.TryGetCurrentTypeName(assemblyQualifiedTypeName, out var currentTypeName))
            {
                type = Type.GetType(currentTypeName);
            }

            if (type == null || !TypeConstruction.CanBeConstructed(type))
                return null;

            return type;
        }
    }
}
