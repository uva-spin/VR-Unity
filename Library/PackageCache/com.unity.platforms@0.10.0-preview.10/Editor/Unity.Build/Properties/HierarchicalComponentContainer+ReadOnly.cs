using System;
using System.Collections.Generic;

namespace Unity.Build
{
    /// <summary>
    /// Base class that stores a set of hierarchical components by type.
    /// Other containers can be added as dependencies to get inherited or overridden components.
    /// </summary>
    /// <typeparam name="TContainer">Type of the component container.</typeparam>
    /// <typeparam name="TComponent">Components base type.</typeparam>
    public abstract partial class HierarchicalComponentContainer<TContainer, TComponent> : ScriptableObjectPropertyContainer<TContainer>
        where TContainer : HierarchicalComponentContainer<TContainer, TComponent>
    {
        /// <summary>
        /// Returns a read-only wrapper for this container.
        /// </summary>
        /// <returns></returns>
        public ReadOnly AsReadOnly() => new ReadOnly(this);

        /// <summary>
        /// A read-only wrapper for this container, which does not expose methods that modify the container.
        /// If changes are made to the underlying container, the read-only wrapper reflects those changes.
        /// </summary>
        public class ReadOnly
        {
            readonly HierarchicalComponentContainer<TContainer, TComponent> m_Container;

            internal ReadOnly(HierarchicalComponentContainer<TContainer, TComponent> container)
            {
                m_Container = container;
            }

            /// <summary>
            /// Determine if a component exist in this container or dependencies recursively.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
            public bool HasComponent(Type type) => m_Container.HasComponent(type);

            /// <summary>
            /// Determine if a component exist in this container or dependencies recursively.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
            public bool HasComponent<T>() where T : TComponent => m_Container.HasComponent<T>();

            /// <summary>
            /// Determine if a component is inherited from a dependency.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns><see langword="true"/> if the component is inherited, <see langword="false"/> otherwise.</returns>
            public bool IsComponentInherited(Type type) => m_Container.IsComponentInherited(type);

            /// <summary>
            /// Determine if a component is inherited from a dependency.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns><see langword="true"/> if the component is inherited, <see langword="false"/> otherwise.</returns>
            public bool IsComponentInherited<T>() where T : TComponent => m_Container.IsComponentInherited<T>();

            /// <summary>
            /// Determines if component overrides a dependency.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise.</returns>
            public bool IsComponentOverriding(Type type) => m_Container.IsComponentOverriding(type);

            /// <summary>
            /// Determines if component overrides a dependency.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns><see langword="true"/> if component overrides a dependency, <see langword="false"/> otherwise.</returns>
            public bool IsComponentOverriding<T>() where T : TComponent => m_Container.IsComponentOverriding<T>();

            /// <summary>
            /// Get a component value.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns>The component value if found, throws otherwise.</returns>
            public TComponent GetComponent(Type type) => m_Container.GetComponent(type);

            /// <summary>
            /// Get a component value.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns>The component value if found, throws otherwise.</returns>
            public T GetComponent<T>() where T : TComponent => m_Container.GetComponent<T>();

            /// <summary>
            /// Get a component value.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <param name="value">The component value.</param>
            /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
            public bool TryGetComponent(Type type, out TComponent value) => m_Container.TryGetComponent(type, out value);

            /// <summary>
            /// Get a component value.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <param name="value">The component value.</param>
            /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
            public bool TryGetComponent<T>(out T value) where T : TComponent => m_Container.TryGetComponent(out value);

            /// <summary>
            /// Get a component value if found, default value otherwise.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns>The component value if found, default value otherwise.</returns>
            public TComponent GetComponentOrDefault(Type type) => m_Container.GetComponentOrDefault(type);

            /// <summary>
            /// Get a component value if found, default value otherwise.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns>The component value if found, default value otherwise.</returns>
            public T GetComponentOrDefault<T>() where T : TComponent => m_Container.GetComponentOrDefault<T>();

            /// <summary>
            /// Get all component values from all dependencies recursively.
            /// </summary>
            /// <returns>Enumeration of component values.</returns>
            public IEnumerable<TComponent> GetComponents() => m_Container.GetComponents();

            /// <summary>
            /// Get all component values from all dependencies recursively, that matches specified component type.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <returns>Enumeration of component values.</returns>
            public IEnumerable<TComponent> GetComponents(Type type) => m_Container.GetComponents(type);

            /// <summary>
            /// Get all component values from all dependencies recursively, that matches specified component type.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <returns>Enumeration of component values.</returns>
            public IEnumerable<T> GetComponents<T>() where T : TComponent => m_Container.GetComponents<T>();

            /// <summary>
            /// Get all component types from all dependencies recursively.
            /// </summary>
            /// <returns>Enumeration of component types.</returns>
            public IEnumerable<Type> GetComponentTypes() => m_Container.GetComponentTypes();

            /// <summary>
            /// Get the source container from which the component value is coming from.
            /// </summary>
            /// <param name="componentType">The component type.</param>
            /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
            /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
            public ReadOnly GetComponentSource(Type componentType, bool dependenciesOnly = false) => m_Container.GetComponentSource(componentType, dependenciesOnly)?.AsReadOnly();

            /// <summary>
            /// Get the source container from which the component value is coming from.
            /// </summary>
            /// <typeparam name="T">The component type.</typeparam>
            /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
            /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
            public ReadOnly GetComponentSource<T>(bool dependenciesOnly = false) where T : TComponent => m_Container.GetComponentSource<T>(dependenciesOnly)?.AsReadOnly();

            [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
            public bool IsComponentOverridden(Type type) => m_Container.IsComponentOverriding(type);

            [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
            public bool IsComponentOverridden<T>() where T : TComponent => m_Container.IsComponentOverriding<T>();
        }
    }
}
