using System;
using System.Reflection;

namespace Unity.Build
{
    /// <summary>
    /// <see cref="Type"/> extensions.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Get assembly qualified type name.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The assembly qualified type name.</returns>
        public static string GetAssemblyQualifiedTypeName(this Type type) => $"{type}, {type.Assembly.GetName().Name}";

        /// <summary>
        /// Determine if the type has an attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if type has attribute, <see langword="false"/> otherwise.</returns>
        public static bool HasAttribute<T>(this Type type) where T : Attribute => type.GetCustomAttribute<T>() != null;

        /// <summary>
        /// Determine if the enum value has an attribute.
        /// </summary>
        /// <typeparam name="T">The attribute type.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns></returns>
        public static bool HasAttribute<T>(this Enum enumValue) where T : Attribute
        {
            var type = enumValue.GetType();
            var memInfo = type.GetMember(enumValue.ToString());
            return memInfo[0].GetCustomAttribute<T>() != null;
        }
    }
}
