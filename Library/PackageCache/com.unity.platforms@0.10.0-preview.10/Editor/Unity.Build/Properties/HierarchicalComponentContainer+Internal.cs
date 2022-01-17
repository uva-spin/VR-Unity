using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.Properties.Editor;
using UnityEngine.Pool;

namespace Unity.Build
{
    public abstract partial class HierarchicalComponentContainer<TContainer, TComponent> : ScriptableObjectPropertyContainer<TContainer>
        where TContainer : HierarchicalComponentContainer<TContainer, TComponent>
    {
        protected override void Reset()
        {
            base.Reset();
            Dependencies.Clear();
            Components.Clear();
        }

        protected override void Sanitize()
        {
            base.Sanitize();
            // Note: We do not remove null dependencies because we want them to appear on the UI
            Components.RemoveAll(component => component == null);
        }

        bool Internal_HasComponentOnSelf(Type componentType)
        {
            return Components.Any(c => componentType.IsAssignableFrom(c.GetType()));
        }

        bool Internal_HasComponentOnDependency(Type componentType)
        {
            foreach (var dependency in Dependencies.Select(d => d.asset).Reverse())
            {
                if (dependency == null || !dependency)
                    continue;

                if (dependency.Internal_HasComponentOnSelf(componentType))
                    return true;

                if (dependency.Internal_HasComponentOnDependency(componentType))
                    return true;
            }
            return false;
        }

        bool Internal_HasComponent(Type componentType)
        {
            return Internal_HasComponentOnSelf(componentType) || Internal_HasComponentOnDependency(componentType);
        }

        bool Internal_IsComponentInherited(Type componentType)
        {
            return !Internal_HasComponentOnSelf(componentType) && Internal_HasComponentOnDependency(componentType);
        }

        bool Internal_IsComponentOverriding(Type componentType)
        {
            return Internal_HasComponentOnSelf(componentType) && Internal_HasComponentOnDependency(componentType);
        }

        bool Internal_GetComponent<T>(Type componentType, out T value) where T : TComponent
        {
            foreach (var component in Components)
            {
                if (componentType.IsAssignableFrom(component.GetType()))
                {
                    value = (T)component;
                    return true;
                }
            }

            foreach (var dependency in Dependencies.Select(d => d.asset).Reverse())
            {
                if (dependency == null || !dependency)
                    continue;

                if (dependency.Internal_GetComponent(componentType, out value))
                    return true;
            }

            value = default;
            return false;
        }

        bool Internal_GetComponentSource(Type componentType, bool dependenciesOnly, out TContainer value)
        {
            if (!dependenciesOnly)
            {
                if (Components.Any(c => componentType.IsAssignableFrom(c.GetType())))
                {
                    value = (TContainer)this;
                    return true;
                }
            }

            foreach (var dependency in Dependencies.Select(d => d.asset).Reverse())
            {
                if (dependency == null || !dependency)
                    continue;

                if (dependency.Internal_GetComponentSource(componentType, false, out value))
                    return true;
            }

            value = default;
            return false;
        }

        void Internal_SetComponent(TComponent value)
        {
            var componentType = value.GetType();
            for (var i = 0; i < Components.Count; ++i)
            {
                if (Components[i].GetType() == componentType)
                {
                    Components[i] = value;
                    return;
                }
            }
            Components.Add(value);
        }

        bool Internal_RemoveComponent(Type componentType)
        {
            return Components.RemoveAll(c => componentType.IsAssignableFrom(c.GetType())) > 0;
        }

        IEnumerable<TComponent> Internal_GetAllComponents()
        {
            using (var pooledList = ListPool<Type>.Get(out var componentTypes))
            using (var pooledDict = DictionaryPool<Type, TComponent>.Get(out var componentValues))
            {
                Internal_GetAllComponentsRecurse(componentTypes, componentValues);

                // Return component values sorted by component types appearance
                var components = new List<TComponent>(componentTypes.Count);
                foreach (var componentType in componentTypes)
                {
                    components.Add(componentValues[componentType]);
                }
                return components;
            }
        }

        void Internal_GetAllComponentsRecurse(List<Type> componentTypes, Dictionary<Type, TComponent> componentValues)
        {
            foreach (var dependency in Dependencies.Select(d => d.asset).Reverse())
            {
                if (dependency == null || !dependency)
                    continue;

                dependency.Internal_GetAllComponentsRecurse(componentTypes, componentValues);
            }

            foreach (var component in Components)
            {
                var componentType = component.GetType();
                if (!componentValues.ContainsKey(componentType))
                    componentTypes.Add(componentType);

                componentValues[componentType] = component;
            }
        }

        IEnumerable<Type> Internal_GetAllComponentTypes()
        {
            var componentTypes = new List<Type>();
            using (var pooledHashSet = HashSetPool<Type>.Get(out var componentTypesHashSet))
            {
                Internal_GetAllComponentTypesRecurse(componentTypes, componentTypesHashSet);
            }
            return componentTypes;
        }

        void Internal_GetAllComponentTypesRecurse(List<Type> componentTypes, HashSet<Type> componentTypesHashSet)
        {
            foreach (var dependency in Dependencies.Select(d => d.asset).Reverse())
            {
                if (dependency == null || !dependency)
                    continue;

                dependency.Internal_GetAllComponentTypesRecurse(componentTypes, componentTypesHashSet);
            }

            foreach (var component in Components)
            {
                var componentType = component.GetType();
                if (!componentTypesHashSet.Contains(componentType))
                    componentTypes.Add(componentType);

                componentTypesHashSet.Add(componentType);
            }
        }

        bool Internal_HasDependency(TContainer container)
        {
            foreach (var dependency in Dependencies.Select(d => d.asset))
            {
                if (dependency == null || !dependency)
                    continue;

                if (dependency == container)
                    return true;

                if (dependency.Internal_HasDependency(container))
                    return true;
            }
            return false;
        }

        IEnumerable<TContainer> Internal_GetAllDependencies()
        {
            var dependencies = new List<TContainer>();
            Internal_GetAllDependenciesRecurse(dependencies);
            return dependencies;
        }

        void Internal_GetAllDependenciesRecurse(List<TContainer> dependencies)
        {
            foreach (var dependency in Dependencies.Select(d => d.asset))
            {
                if (dependency == null || !dependency)
                    continue;

                dependencies.Add(dependency);
                dependency.Internal_GetAllDependenciesRecurse(dependencies);
            }
        }

        TComponent Internal_Construct(Type componentType)
        {
            var component = Construct(componentType);
            if (component is IHierarchicalComponentInitialize<TContainer, TComponent> initializable)
                initializable.Initialize(AsReadOnly());

            return component;
        }

        internal static void ValidateComponentTypeAndThrow(Type componentType)
        {
            if (componentType == null)
                throw new ArgumentNullException(nameof(componentType));

            if (componentType == typeof(object))
                throw new InvalidOperationException("Component type cannot be object.");

            if (!typeof(TComponent).IsAssignableFrom(componentType))
                throw new InvalidOperationException($"Component type {componentType.FullName} does not derive from {typeof(TComponent).FullName}.");
        }

        internal static void ValidateComponentTypeSetAndThrow(Type componentType)
        {
            if (componentType.IsInterface || componentType.IsAbstract)
                throw new InvalidOperationException($"Component type {componentType.FullName} is interface or abstract. This is not allowed.");
        }

        internal static void ValidateComponentValueAndThrow(TComponent value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
        }

        internal static TComponent Construct(Type componentType)
        {
            if (!TypeConstruction.CanBeConstructed(componentType))
                throw new InvalidOperationException($"Component type {componentType.FullName} cannot be constructed because it does not have a default, implicit or registered constructor.");

            return TypeConstruction.Construct<TComponent>(componentType);
        }

        internal static TComponent Clone(TComponent component)
        {
            using (var pooledVisitor = CopyVisitor.Get(out var visitor))
            {
                visitor.Result = Construct(component.GetType());
                PropertyContainer.Visit(ref component, visitor);
                return visitor.Result;
            }
        }

        class CopyVisitor : PropertyVisitor
        {
            static ObjectPool<CopyVisitor> s_Pool = new ObjectPool<CopyVisitor>(() => new CopyVisitor(), defaultCapacity: 1);
            public static PooledObject<CopyVisitor> Get(out CopyVisitor visitor) => s_Pool.Get(out visitor);

            TComponent m_DstComponent;

            public TComponent Result
            {
                get => m_DstComponent;
                set => m_DstComponent = value;
            }

            protected override void VisitProperty<TSrcContainer, TSrcValue>(Property<TSrcContainer, TSrcValue> property, ref TSrcContainer container, ref TSrcValue value)
            {
                PropertyContainer.TrySetValue(ref m_DstComponent, property.Name, value);
            }

            protected override void VisitList<TSrcContainer, TSrcList, TSrcElement>(Property<TSrcContainer, TSrcList> property, ref TSrcContainer container, ref TSrcList value)
            {
                TSrcList list;
                if (typeof(TSrcList).IsArray)
                {
                    list = TypeConstruction.ConstructArray<TSrcList>(value.Count);
                    for (var i = 0; i < value.Count; ++i)
                    {
                        list[i] = value[i];
                    }
                }
                else
                {
                    list = TypeConstruction.Construct<TSrcList>();
                    foreach (var item in value)
                    {
                        list.Add(item);
                    }
                }
                base.VisitList<TSrcContainer, TSrcList, TSrcElement>(property, ref container, ref list);
            }

            protected override void VisitSet<TSrcContainer, TSrcSet, TSrcValue>(Property<TSrcContainer, TSrcSet> property, ref TSrcContainer container, ref TSrcSet value)
            {
                var set = TypeConstruction.Construct<TSrcSet>();
                foreach (var item in value)
                {
                    set.Add(item);
                }
                base.VisitSet<TSrcContainer, TSrcSet, TSrcValue>(property, ref container, ref set);
            }

            protected override void VisitDictionary<TSrcContainer, TSrcDictionary, TSrcKey, TSrcValue>(Property<TSrcContainer, TSrcDictionary> property, ref TSrcContainer container, ref TSrcDictionary value)
            {
                var dictionary = TypeConstruction.Construct<TSrcDictionary>();
                foreach (var item in value)
                {
                    dictionary.Add(item);
                }
                base.VisitDictionary<TSrcContainer, TSrcDictionary, TSrcKey, TSrcValue>(property, ref container, ref dictionary);
            }
        }
    }
}
