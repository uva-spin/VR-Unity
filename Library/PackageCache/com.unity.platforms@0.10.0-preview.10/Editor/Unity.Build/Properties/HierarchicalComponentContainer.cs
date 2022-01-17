using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;

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
        [CreateProperty] internal readonly List<LazyLoadReference<TContainer>> Dependencies = new List<LazyLoadReference<TContainer>>();
        [CreateProperty] internal readonly List<TComponent> Components = new List<TComponent>();

        /// <summary>
        /// Create a new instance using the specified container as a template to copy from.
        /// </summary>
        /// <param name="container">The template container.</param>
        /// <returns>The new instance.</returns>
        public static TContainer CreateInstance(TContainer container)
        {
            var instance = CreateInstance<TContainer>();
            foreach (var component in container.Components)
            {
                instance.SetComponent(component);
            }
            foreach (var dependency in container.Dependencies)
            {
                instance.Dependencies.Add(dependency);
            }
            return instance;
        }

        /// <summary>
        /// Determine if a component exist in this container or dependencies recursively.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
        public bool HasComponent(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_HasComponent(componentType);
        }

        /// <summary>
        /// Determine if a component exist in this container or dependencies recursively.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
        public bool HasComponent<T>() where T : TComponent => Internal_HasComponent(typeof(T));

        /// <summary>
        /// Determine if a component is inherited from a dependency.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns><see langword="true"/> if the component is inherited, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_IsComponentInherited(componentType);
        }

        /// <summary>
        /// Determine if a component is inherited from a dependency.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is inherited, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited<T>() where T : TComponent => Internal_IsComponentInherited(typeof(T));

        /// <summary>
        /// Determines if component overrides a dependency.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_IsComponentOverriding(componentType);
        }

        /// <summary>
        /// Determines if component overrides a dependency.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding<T>() where T : TComponent => Internal_IsComponentOverriding(typeof(T));

        /// <summary>
        /// Get a component value.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The component value if found, throws otherwise.</returns>
        public TComponent GetComponent(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            if (!Internal_GetComponent(componentType, out TComponent value))
                throw new InvalidOperationException($"Component type {componentType.FullName} not found.");

            return Clone(value);
        }

        /// <summary>
        /// Get a component value.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The component value if found, throws otherwise.</returns>
        public T GetComponent<T>() where T : TComponent
        {
            var componentType = typeof(T);
            if (!Internal_GetComponent(componentType, out T value))
                throw new InvalidOperationException($"Component type {componentType.FullName} not found.");

            return (T)Clone(value);
        }

        /// <summary>
        /// Get a component value.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
        public bool TryGetComponent(Type componentType, out TComponent value)
        {
            try
            {
                ValidateComponentTypeAndThrow(componentType);
            }
            catch
            {
                value = default;
                return false;
            }

            if (Internal_GetComponent(componentType, out TComponent component))
            {
                value = Clone(component);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get a component value.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, <see langword="false"/> otherwise.</returns>
        public bool TryGetComponent<T>(out T value) where T : TComponent
        {
            if (Internal_GetComponent(typeof(T), out T component))
            {
                value = (T)Clone(component);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get a component value if found, default value otherwise.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>The component value if found, default value otherwise.</returns>
        public TComponent GetComponentOrDefault(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_GetComponent(componentType, out TComponent value) ? Clone(value) : Internal_Construct(componentType);
        }

        /// <summary>
        /// Get a component value if found, default value otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The component value if found, default value otherwise.</returns>
        public T GetComponentOrDefault<T>() where T : TComponent => Internal_GetComponent(typeof(T), out T value) ? (T)Clone(value) : (T)Internal_Construct(typeof(T));

        /// <summary>
        /// Get all component values from all dependencies recursively.
        /// </summary>
        /// <returns>Enumeration of component values.</returns>
        public IEnumerable<TComponent> GetComponents() => Internal_GetAllComponents().Select(c => Clone(c));

        /// <summary>
        /// Get all component values from all dependencies recursively, that matches specified component type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns>Enumeration of component values.</returns>
        public IEnumerable<TComponent> GetComponents(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_GetAllComponents().Where(c => componentType.IsAssignableFrom(c.GetType())).Select(c => Clone(c));
        }

        /// <summary>
        /// Get all component values from all dependencies recursively, that matches specified component type.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>Enumeration of component values.</returns>
        public IEnumerable<T> GetComponents<T>() where T : TComponent => Internal_GetAllComponents().Where(c => typeof(T).IsAssignableFrom(c.GetType())).Select(c => Clone(c)).Cast<T>();

        /// <summary>
        /// Get all component types from all dependencies recursively.
        /// </summary>
        /// <returns>Enumeration of component types.</returns>
        public IEnumerable<Type> GetComponentTypes() => Internal_GetAllComponentTypes();

        /// <summary>
        /// Get the source container from which the component value is coming from.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
        /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
        public TContainer GetComponentSource(Type componentType, bool dependenciesOnly = false)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_GetComponentSource(componentType, dependenciesOnly, out var source) ? source : null;
        }

        /// <summary>
        /// Get the source container from which the component value is coming from.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="dependenciesOnly">If <see langword="true"/>, only look in dependencies, otherwise also look into this container.</param>
        /// <returns>A container if component is found, <see langword="null"/> otherwise.</returns>
        public TContainer GetComponentSource<T>(bool dependenciesOnly = false) where T : TComponent => Internal_GetComponentSource(typeof(T), dependenciesOnly, out var source) ? source : null;

        /// <summary>
        /// Set a component to the value specified.
        /// </summary>
        /// <param name="value">The component value.</param>
        public void SetComponent(TComponent value)
        {
            ValidateComponentValueAndThrow(value);
            Internal_SetComponent(Clone(value));
        }

        /// <summary>
        /// Set a component to the value specified.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="value">The component value.</param>
        public void SetComponent<T>(T value) where T : TComponent
        {
            ValidateComponentValueAndThrow(value);
            Internal_SetComponent(Clone(value));
        }

        /// <summary>
        /// Set a component to default value.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        public void SetComponent(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            ValidateComponentTypeSetAndThrow(componentType);
            Internal_SetComponent(Internal_Construct(componentType));
        }

        /// <summary>
        /// Set a component to default value.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        public void SetComponent<T>() where T : TComponent
        {
            var componentType = typeof(T);
            ValidateComponentTypeSetAndThrow(componentType);
            Internal_SetComponent(Internal_Construct(componentType));
        }

        /// <summary>
        /// Remove all components that matches the component type.
        /// </summary>
        /// <param name="componentType">The component type.</param>
        /// <returns><see langword="true"/> if one or more component was removed, <see langword="false"/> otherwise.</returns>
        public bool RemoveComponent(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_RemoveComponent(componentType);
        }

        /// <summary>
        /// Remove all components that matches the component type.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <returns><see langword="true"/> if one or more component was removed, <see langword="false"/> otherwise.</returns>
        public bool RemoveComponent<T>() where T : TComponent => Internal_RemoveComponent(typeof(T));

        /// <summary>
        /// Remove all components from this container.
        /// </summary>
        public void ClearComponents() => Components.Clear();

        /// <summary>
        /// Determine if a dependency exist in this container or its dependencies.
        /// </summary>
        /// <param name="dependency">The dependency to search.</param>
        /// <returns><see langword="true"/> if the dependency is found, <see langword="false"/> otherwise.</returns>
        public bool HasDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
                throw new ArgumentNullException("Dependency is null or invalid.");

            return Internal_HasDependency(dependency);
        }

        /// <summary>
        /// Add a dependency to this container.
        /// Circular dependencies or dependencies on self are not allowed.
        /// </summary>
        /// <param name="dependency">The dependency to add.</param>
        /// <returns><see langword="true"/> if the dependency was added, <see langword="false"/> otherwise.</returns>
        public bool AddDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
                throw new ArgumentNullException("Dependency is null or invalid.");

            if (dependency == this || HasDependency(dependency) || dependency.HasDependency((TContainer)this))
                return false;

            Dependencies.Add(dependency.GetInstanceID());
            return true;
        }

        /// <summary>
        /// Get all dependencies recursively.
        /// </summary>
        /// <returns>Enumeration of container.</returns>
        public IEnumerable<TContainer> GetDependencies() => Internal_GetAllDependencies();

        /// <summary>
        /// Remove a dependency from this container.
        /// </summary>
        /// <param name="dependency">The dependency to remove.</param>
        public bool RemoveDependency(TContainer dependency)
        {
            if (dependency == null || !dependency)
                throw new ArgumentNullException(nameof(dependency));

            return Dependencies.Remove(dependency.GetInstanceID());
        }

        /// <summary>
        /// Remove all dependencies from this container.
        /// </summary>
        public void ClearDependencies() => Dependencies.Clear();

        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
        public bool IsComponentOverridden(Type componentType)
        {
            ValidateComponentTypeAndThrow(componentType);
            return Internal_IsComponentOverriding(componentType);
        }

        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
        public bool IsComponentOverridden<T>() where T : TComponent => Internal_IsComponentOverriding(typeof(T));

        [Obsolete("The separate component type parameter is no longer required, please remove it. (RemovedAfter 2021-01-01)")]
        public void SetComponent(Type componentType, TComponent value)
        {
            ValidateComponentTypeAndThrow(componentType);
            ValidateComponentValueAndThrow(value);
            Internal_SetComponent(Clone(value));
        }

        [Obsolete("Functionality has been replaced by IHierarchicalComponentInitialize interface. (RemovedAfter 2021-01-01)")]
        protected virtual void OnComponentConstruct(ref TComponent component) { }
    }
}
