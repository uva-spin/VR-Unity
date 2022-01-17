using System;
using System.Collections.Generic;
using System.Text;
using Unity.Properties.Internal;

namespace Unity.Properties.Editor
{
    /// <summary>
    /// Utility class around <see cref="System.Type"/>.
    /// </summary>
    public static class TypeUtility
    {
        static readonly Dictionary<Type, string> s_CachedResolvedName;
        static readonly Unity.Properties.Internal.Pool<StringBuilder> s_Builders;

        static TypeUtility()
        {
            s_CachedResolvedName = new Dictionary<Type, string>();
            s_Builders = new Pool<StringBuilder>(()=> new StringBuilder(), sb => sb.Clear());
        }
        
        /// <summary>
        /// Utility method to get the name of a type which includes the parent type(s).
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> we want the name of.</param>
        /// <returns>The display name of the type.</returns>
        public static string GetTypeDisplayName(Type type)
        {
            if (s_CachedResolvedName.TryGetValue(type, out var name)) 
                return name;
            
            var index = 0;
            name = GetTypeDisplayName(type, type.GetGenericArguments(), ref index);
            s_CachedResolvedName[type] = name;
            return name;
        }

        static string GetTypeDisplayName(Type type, IReadOnlyList<Type> args, ref int argIndex)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(short))
                return "short";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(char))
                return "char";
            if (type == typeof(bool)) 
                return "bool";
            if (type == typeof(long))
                return "long";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(string))
                return "string";
            
            var name = type.Name;

            if (type.IsGenericParameter)
            {
                return name;
            }

            if (type.IsNested)
            {
                name = $"{GetTypeDisplayName(type.DeclaringType, args, ref argIndex)}.{name}";
            }

            if (!type.IsGenericType) 
                return name;
            
            var tickIndex = name.IndexOf('`');

            var count = type.GetGenericArguments().Length;

            if (tickIndex > -1)
            {
                count = int.Parse(name.Substring(tickIndex + 1));
                name = name.Remove(tickIndex);
            }

            var genericTypeNames = s_Builders.Get();
            try
            {
                for (var i = 0; i < count && argIndex < args.Count; i++, argIndex++)
                {
                    if (i != 0) genericTypeNames.Append(", ");
                    genericTypeNames.Append(GetTypeDisplayName(args[argIndex]));
                }

                if (genericTypeNames.Length > 0)
                {
                    name = $"{name}<{genericTypeNames}>";
                }
            }
            finally
            {
                s_Builders.Release(genericTypeNames);
            }

            return name;
        }
        
        /// <summary>
        /// Utility method to return the base type.
        /// </summary>
        /// <param name="type">The <see cref="System.Type"/> for which we want the base type.</param>
        /// <returns>The base type.</returns>
        public static Type GetRootType(this Type type)
        {
            if (type.IsInterface)
                return null;
            
            var baseType = type.IsValueType ? typeof(ValueType) : typeof(object);
            while (baseType != type.BaseType)
            {
                type = type.BaseType;
            }

            return type;
        }
    }
}