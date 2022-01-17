using System;
using System.Collections.Generic;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Properties.UI.Internal
{
    /// <summary>
    /// Maintains a database of all the inspector-related types and allows creation of new instances of inspectors.
    /// </summary>
    static class CustomInspectorDatabase
    {
        [Flags]
        internal enum RegistrationStatus
        {
            Registered                                   = 0, /* Unused at the moment */
            MissingDefaultConstructor                    = 1,
            GenericArgumentsDoNotMatchInspectedType      = 1 << 1,
            UnsupportedUserDefinedGenericInspector       = 1 << 2,
            UnsupportedPartiallyResolvedGenericInspector = 1 << 3,
            UnsupportedGenericInspectorForNonGenericType = 1 << 4,
            UnsupportedGenericArrayInspector             = 1 << 5,
        }

        class RegistrationInfo
        {
            readonly Dictionary<RegistrationStatus, List<Type>> s_UnregisteredTypesPerStatus = new Dictionary<RegistrationStatus, List<Type>>();
            readonly Dictionary<Type, RegistrationStatus> s_StatusPerType = new Dictionary<Type, RegistrationStatus>();
            
            public void CacheInvalidInspectorType(Type type, RegistrationStatus status)
            {
                if (s_StatusPerType.TryGetValue(type, out var s))
                    status |= s;
                s_StatusPerType[type] = status;
                
                var data = s_UnregisteredTypesPerStatus;
                if (!data.TryGetValue(status, out var list))
                {
                    data[status] = list = new List<Type>();
                }
                list.Add(type);
            }

            public RegistrationStatus GetStatus(Type t)
                =>  s_StatusPerType.TryGetValue(t, out var status) ? status : RegistrationStatus.Registered;
        }
        
        static readonly Dictionary<Type, List<Type>> s_InspectorsPerType;
        static readonly Dictionary<Type, Type[]> s_GenericArgumentsPerType;
        static readonly Dictionary<Type, Type[]> s_RootGenericArgumentsPerType;
        static readonly RegistrationInfo s_RegistrationInfo;

        static CustomInspectorDatabase()
        {
            s_InspectorsPerType = new Dictionary<Type, List<Type>>();
            s_GenericArgumentsPerType = new Dictionary<Type, Type[]>();
            s_RootGenericArgumentsPerType = new Dictionary<Type, Type[]>();
            
            s_RegistrationInfo = new RegistrationInfo();
            RegisterCustomInspectors(s_RegistrationInfo);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="IInspector{TValue}"/> that can act as a root inspector.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate inspector types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>The inspector instance or null</returns>
        public static Inspector<TValue> GetRootInspector<TValue>(params IInspectorConstraint[] constraints)
        {
            return GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.Not.AssignableTo<IPropertyDrawer>(), constraints));
        }

        /// <summary>
        /// Creates a new instance of a <see cref="PropertyDrawer{TValue, TAttribute}"/> that can act as a property drawer
        /// for a given field.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <typeparam name="TAttribute">The attribute type the value was tagged with</typeparam>
        /// <returns>The property drawer instance or null</returns>
        public static PropertyDrawer<TValue, TAttribute> GetPropertyDrawer<TValue, TAttribute>(
            params IInspectorConstraint[] constraints)
            where TAttribute : UnityEngine.PropertyAttribute
        {
            return (PropertyDrawer<TValue, TAttribute>) GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.AssignableTo<IPropertyDrawer<TAttribute>>(),
                    constraints));
        }

        /// <summary>
        /// Returns all the inspector candidate types that satisfy the constraints.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>The candidate inspector types</returns>
        internal static IEnumerable<Type> GetInspectorTypes<TValue>(params IInspectorConstraint[] constraints)
        {
            var valueType = typeof(TValue);
            
            foreach (var inspectorType in GetInspectorTypes(s_InspectorsPerType, valueType, constraints))
            {
                yield return inspectorType;
            }

            if (valueType.IsArray)
            {
                foreach (var inspectorType in GetArrayInspectorTypes(s_InspectorsPerType, valueType, constraints))
                {
                    yield return inspectorType;
                }    
            }

            if (!valueType.IsGenericType)
                yield break;
            
            foreach (var inspectorType in GetInspectorTypes(s_InspectorsPerType, valueType.GetGenericTypeDefinition(), constraints))
            {
                yield return inspectorType;
            }
        }

        /// <summary>
        /// Creates an inspector instance that satisfy the constraints.
        /// </summary>
        /// <param name="constraints">Constraints that filter the candidate property drawer types.</param>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <returns>An inspector instance of null</returns>
        internal static Inspector<TValue> GetInspector<TValue>(params IInspectorConstraint[] constraints)
        {
            var valueType = typeof(TValue);
            var genericArguments = valueType.IsGenericType? valueType.GetGenericArguments() : Array.Empty<Type>();
            var candidates = ListPool<Type>.Get();
            try
            {
                foreach (var type in GetInspectorTypes<TValue>(constraints))
                {
                    var t = type;
                    if (type.IsGenericType)
                    {
                        if (!OrderGenericArguments(genericArguments, type, out var types))
                            continue; 
                        
                        if (type.IsGenericTypeDefinition)
                            t = type.MakeGenericType(types);
                    }

                    candidates.Add(t);
                }

                if (candidates.Count == 0)
                    return null;

                Type bestType = null;
                var parameters = int.MaxValue;

                foreach (var type in candidates)
                {
                    if (!type.IsGenericType)
                    {
                        bestType = type;
                        break;
                    }

                    if (null == type.GetInterface(nameof(IExperimentalInspector)))
                        continue;
                    
                    if (type.GetRootType() != typeof(Inspector<TValue>))
                        continue;

                    var rootCount = 0;
                    foreach (var argument in GetRootGenericArguments(type)[0].GetGenericArguments())
                    {
                        if (argument.IsGenericParameter)
                            ++rootCount;
                    }

                    var argumentCount = 0;
                    foreach (var argument in GetGenericArguments(type))
                    {
                        if (argument.IsGenericParameter)
                            ++argumentCount;
                    }
                    
                    if (rootCount != genericArguments.Length)
                        continue;

                    if (argumentCount >= parameters) 
                        continue;
                    
                    parameters = argumentCount;
                    bestType = type;
                }

                return null != bestType
                    ? (Inspector<TValue>) Activator.CreateInstance(bestType)
                    : null;
            }
            finally
            {
                ListPool<Type>.Release(candidates);
            }
        }

        static Type[] GetGenericArguments(Type type)
        {
            if (!type.IsGenericType)
                return Array.Empty<Type>();

            if (!s_GenericArgumentsPerType.TryGetValue(type, out var array))
                s_GenericArgumentsPerType[type] = array = type.GetGenericTypeDefinition().GetGenericArguments();
            return array;
        }
        
        static Type[] GetRootGenericArguments(Type type)
        {
            if (!type.IsGenericType)
                return Array.Empty<Type>();

            if (!s_RootGenericArgumentsPerType.TryGetValue(type, out var array))
                s_RootGenericArgumentsPerType[type] = array = type.GetGenericTypeDefinition().GetRootType().GetGenericArguments();
            return array;
        }

        internal static IInspector<TValue> GetPropertyDrawer<TValue>(IProperty property)
        {
            return GetPropertyDrawer<TValue>(property.GetAttributes<UnityEngine.PropertyAttribute>() 
                                             ?? Array.Empty<UnityEngine.PropertyAttribute>());
        }
        
        internal static IInspector<TValue> GetPropertyDrawer<TValue>(IEnumerable<Attribute> attributes)
        {
            foreach(var drawerAttribute in attributes)
            {
                if (!(drawerAttribute is PropertyAttribute)) 
                    continue;
                
                var drawer = typeof(IPropertyDrawer<>).MakeGenericType(drawerAttribute.GetType());
                var inspector = GetPropertyDrawer<TValue>(InspectorConstraint.AssignableTo(drawer));
                if (null != inspector)
                {
                    return inspector;
                }
            }

            return null;
        }

        internal static IInspector<TValue> GetBestInspectorType<TValue>(IProperty property)
        {
            var inspector = default(IInspector<TValue>);
            foreach(var drawerAttribute in property.GetAttributes<UnityEngine.PropertyAttribute>() ?? Array.Empty<UnityEngine.PropertyAttribute>())
            {
                var drawer = typeof(IPropertyDrawer<>).MakeGenericType(drawerAttribute.GetType());
                inspector = GetPropertyDrawer<TValue>(InspectorConstraint.AssignableTo(drawer));
                if (null != inspector)
                {
                    break;
                }
            }
            return inspector ?? GetRootInspector<TValue>();
        }
        
        internal static IInspector<TValue> GetPropertyDrawer<TValue>(params IInspectorConstraint[] constraints)
        {
            return GetInspector<TValue>(
                InspectorConstraint.Combine(InspectorConstraint.AssignableTo<IPropertyDrawer>(), constraints));
        }

        static IEnumerable<Type> GetInspectorTypes(Dictionary<Type, List<Type>> lookup, Type type, params IInspectorConstraint[] constraints)
        {
            if (!lookup.TryGetValue(type, out var inspectors))
            {
                yield break;
            }

            foreach (var inspector in inspectors)
            {
                var any = false;
                foreach (var r in constraints)
                {
                    if (r.Satisfy(inspector)) 
                        continue;
                    
                    any = true;
                    break;
                }

                if (any)
                {
                    continue;
                }

                yield return inspector;
            }
        }

        static IEnumerable<Type> GetArrayInspectorTypes(Dictionary<Type, List<Type>> lookup, Type valueType, params IInspectorConstraint[] constraints)
        {
            if (!lookup.TryGetValue(valueType, out var inspectors))
            {
                yield break;
            }

            foreach (var inspector in inspectors)
            {
                var any = false;
                foreach (var r in constraints)
                {
                    if (r.Satisfy(inspector)) 
                        continue;
                    
                    any = true;
                    break;
                }

                if (any)
                {
                    continue;
                }

                yield return inspector;
            }
        }

        static void RegisterCustomInspectors(RegistrationInfo info)
        {
            foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(IInspector<>)))
            {
                RegisterInspectorType(s_InspectorsPerType, typeof(IInspector<>), type, info);
            }
        }

        static void RegisterInspectorType(IDictionary<Type, List<Type>> typeMap, Type interfaceType, Type inspectorType, RegistrationInfo info)
        {
            if (!ValidateCustomInspectorRegistration(inspectorType, info))
                return;
            
            var inspectorInterface = inspectorType.GetInterface(interfaceType.FullName);
            if (null == inspectorInterface || inspectorType.IsAbstract)
                return;

            var genericArguments = inspectorInterface.GetGenericArguments();
            var targetType = genericArguments[0];

            // Generic inspector for generic type
            if (inspectorType.ContainsGenericParameters && targetType.IsGenericType)
                targetType = targetType.GetGenericTypeDefinition();

            if (null == inspectorType.GetConstructor(Array.Empty<Type>()))
            {
                info.CacheInvalidInspectorType(inspectorType, RegistrationStatus.MissingDefaultConstructor);
                return;
            }

            if (!typeMap.TryGetValue(targetType, out var list))
                typeMap[targetType] = list = new List<Type>();

            list.Add(inspectorType);
        }

        static bool ValidateCustomInspectorRegistration(Type type, RegistrationInfo info)
        {
            if (type.IsAbstract)
                return false;

            if (!type.IsGenericType)
                return true;

            var inspectedType = type.GetRootType().GetGenericArguments()[0];
            if (inspectedType.IsArray)
            {
                info.CacheInvalidInspectorType(type, RegistrationStatus.UnsupportedGenericArrayInspector);
                return false;
            }
            
            if (!inspectedType.IsGenericType)
            {
                info.CacheInvalidInspectorType(type, RegistrationStatus.UnsupportedGenericInspectorForNonGenericType);
                return false;
            }
            
            if (null == type.GetInterface(nameof(IExperimentalInspector)))
            {
                info.CacheInvalidInspectorType(type, RegistrationStatus.UnsupportedUserDefinedGenericInspector);
                return false;
            }

            var rootArguments = inspectedType.GetGenericArguments();
            var arguments = GetGenericArguments(type);

            if (arguments.Length > rootArguments.Length)
            {
                info.CacheInvalidInspectorType(type, RegistrationStatus.GenericArgumentsDoNotMatchInspectedType);
                return false;
            }
            
            var set = new HashSet<Type>(rootArguments);
            foreach (var argument in arguments)
            {
                set.Remove(argument);
            }

            if (set.Count == 0) 
                return true;
            
            info.CacheInvalidInspectorType(type, RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector);
            return false;
        }

        static bool OrderGenericArguments(Type[] instanceArguments, Type inspector, out Type[] types)
        {
            var inspectorArguments = inspector.GetGenericArguments();
            var result = new Type[inspectorArguments.Length];
            
            var root = inspector.GetRootType();
            var rootArguments = root.GetGenericArguments()[0].GetGenericArguments();
            
            for (var i = 0; i < inspectorArguments.Length; ++i)
            {
                var index = Array.IndexOf(rootArguments, inspectorArguments[i]);
                if (index < 0)
                {
                    types = Array.Empty<Type>();
                    return false;
                }
                result[i] = instanceArguments[index];
            }

            types = result;
            return true;
        }

        internal static RegistrationStatus GetRegistrationStatusForInspectorType(Type t)
        {
            return s_RegistrationInfo.GetStatus(t);
        }
    }
}