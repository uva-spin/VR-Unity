using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    public abstract class ContextBase : IDisposable
    {
        readonly Dictionary<Type, object> m_Values = new Dictionary<Type, object>();
        readonly Dictionary<Type, IBuildComponent> m_Components = new Dictionary<Type, IBuildComponent>();

        internal BuildPipelineBase BuildPipeline { get; }
        internal BuildConfiguration BuildConfiguration { get; }

        /// <summary>
        /// List of all values stored.
        /// </summary>
        public object[] Values => m_Values.Values.ToArray();

        /// <summary>
        /// List of build component types used by the build pipeline.
        /// </summary>
        public Type[] UsedComponents => BuildPipeline.UsedComponents;

        /// <summary>
        /// The build configuration name.
        /// </summary>
        /// <returns>The build configuration name.</returns>
        public string BuildConfigurationName => BuildConfiguration.name;

        /// <summary>
        /// The build configuration asset path.
        /// </summary>
        /// <returns>The build configuration asset path.</returns>
        public string BuildConfigurationAssetPath => AssetDatabase.GetAssetPath(BuildConfiguration);

        /// <summary>
        /// The build configuration asset GUID.
        /// </summary>
        /// <returns>The build configuration asset GUID.</returns>
        public string BuildConfigurationAssetGUID => AssetDatabase.AssetPathToGUID(BuildConfigurationAssetPath);

        /// <summary>
        /// Determine if the value of type <typeparamref name="T"/> exists.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if value is found, <see langword="false"/> otherwise.</returns>
        public bool HasValue<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));

            if (typeof(IBuildArtifact).IsAssignableFrom(typeof(T)))
                return HasBuildArtifact(typeof(T));

            return m_Values.Keys.Any(type => typeof(T).IsAssignableFrom(type));
        }

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value of type <typeparamref name="T"/> if found, otherwise <see langword="null"/>.</returns>
        public T GetValue<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));

            if (typeof(IBuildArtifact).IsAssignableFrom(typeof(T)))
                return (T)GetBuildArtifact(typeof(T));

            return m_Values.FirstOrDefault(pair => typeof(T).IsAssignableFrom(pair.Key)).Value as T;
        }

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility and then set on this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetOrCreateValue<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));
            var value = GetValue<T>();
            if (value == null)
            {
                value = TypeConstruction.Construct<T>();
                SetValue(value);
            }
            return value;
        }

        /// <summary>
        /// Get value of type <typeparamref name="T"/> if found.
        /// Otherwise a new instance of type <typeparamref name="T"/> constructed using <see cref="TypeConstruction"/> utility.
        /// The build context is not modified.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns>The value or new instance of type <typeparamref name="T"/>.</returns>
        public T GetValueOrDefault<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));
            return GetValue<T>() ?? TypeConstruction.Construct<T>();
        }

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="value">The value to set.</param>
        public void SetValue<T>(T value) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var type = value.GetType();
            ValidateValueTypeAndThrow(type);

            if (typeof(IBuildArtifact).IsAssignableFrom(type))
            {
                if (this is BuildContext buildContext)
                    buildContext.SetBuildArtifact((IBuildArtifact)value);
                else
                    throw new NotSupportedException($"Setting build artifact value on {GetType().FullName} is not supported.");
            }
            else
            {
                m_Values[type] = value;
            }
        }

        /// <summary>
        /// Set value of type <typeparamref name="T"/> to this build context to its default using <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        public void SetValue<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));
            SetValue(TypeConstruction.Construct<T>());
        }

        /// <summary>
        /// Remove value of type <typeparamref name="T"/> from this build context.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <returns><see langword="true"/> if the value was removed, otherwise <see langword="false"/>.</returns>
        public bool RemoveValue<T>() where T : class
        {
            ValidateValueTypeAndThrow(typeof(T));

            if (typeof(IBuildArtifact).IsAssignableFrom(typeof(T)))
            {
                if (this is BuildContext buildContext)
                    return buildContext.RemoveBuildArtifact(typeof(T));
                else
                    throw new NotSupportedException($"Removing build artifact value on {GetType().FullName} is not supported.");
            }
            else
            {
                return m_Values.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Determine if a component type is stored in this container or its dependencies.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <returns><see langword="true"/> if the is found, otherwise <see langword="false"/>.</returns>
        public bool HasComponent(Type type)
        {
            ValidateUsedComponentTypesAndThrow(type);
            BuildConfiguration.ValidateComponentTypeAndThrow(type);
            return m_Components.ContainsKey(type) || BuildConfiguration.HasComponent(type);
        }

        /// <summary>
        /// Determine if a component type is stored in this container or its dependencies.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the is found, otherwise <see langword="false"/>.</returns>
        public bool HasComponent<T>() where T : IBuildComponent => HasComponent(typeof(T));

        /// <summary>
        /// Determine if a component type is inherited from a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component is inherited from dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited(Type type)
        {
            ValidateUsedComponentTypesAndThrow(type);
            return BuildConfiguration.IsComponentInherited(type);
        }

        /// <summary>
        /// Determine if a component type is inherited from a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is inherited from dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentInherited<T>() where T : IBuildComponent => IsComponentInherited(typeof(T));

        /// <summary>
        /// Determines if component overrides a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding(Type type)
        {
            ValidateUsedComponentTypesAndThrow(type);
            return BuildConfiguration.IsComponentOverriding(type);
        }

        /// <summary>
        /// Determines if component overrides a dependency.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if component overrides a dependency, <see langword="false"/> otherwise.</returns>
        public bool IsComponentOverriding<T>() where T : IBuildComponent => IsComponentOverriding(typeof(T));

        /// <summary>
        /// Determine if component is used by the build pipeline.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed(Type type) => BuildPipeline.IsComponentUsed(type);

        /// <summary>
        /// Determine if component is used by the build pipeline.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed<T>() where T : IBuildComponent => IsComponentUsed(typeof(T));

        /// <summary>
        /// Try to get the value of a component type.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, otherwise <see langword="false"/>.</returns>
        public bool TryGetComponent(Type type, out IBuildComponent value)
        {
            ValidateUsedComponentTypesAndThrow(type);

            BuildConfiguration.ValidateComponentTypeAndThrow(type);
            if (m_Components.TryGetValue(type, out value))
            {
                return true;
            }

            return BuildConfiguration.TryGetComponent(type, out value);
        }

        /// <summary>
        /// Try to get the value of a component type.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="value">The component value.</param>
        /// <returns><see langword="true"/> if the component is found, otherwise <see langword="false"/>.</returns>
        public bool TryGetComponent<T>(out T value) where T : IBuildComponent
        {
            if (TryGetComponent(typeof(T), out var result))
            {
                value = (T)result;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Get the value of a component type if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns>The component value.</returns>
        public IBuildComponent GetComponentOrDefault(Type type)
        {
            BuildConfiguration.ValidateComponentTypeAndThrow(type);
            if (m_Components.TryGetValue(type, out var value))
            {
                return value;
            }

            ValidateUsedComponentTypesAndThrow(type);
            return BuildConfiguration.GetComponentOrDefault(type);
        }

        /// <summary>
        /// Get the value of a component type if found.
        /// Otherwise an instance created using <see cref="TypeConstruction"/> utility.
        /// The container is not modified.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns>The component value.</returns>
        public T GetComponentOrDefault<T>() where T : IBuildComponent => (T)GetComponentOrDefault(typeof(T));

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies.
        /// Throws if a component type is not in UsedComponents list.
        /// </summary>
        /// <returns>The list of components.</returns>
        public IEnumerable<IBuildComponent> GetComponents()
        {
            var lookup = new Dictionary<Type, IBuildComponent>();
            var components = BuildConfiguration.GetComponents();
            foreach (var component in components)
            {
                var componentType = component.GetType();
                ValidateUsedComponentTypesAndThrow(componentType);
                lookup[componentType] = component;
            }

            foreach (var pair in m_Components)
            {
                lookup[pair.Key] = pair.Value;
            }

            return lookup.Values;
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <see cref="Type"/>.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <param name="type">Type of the components.</param>
        /// <returns>The list of components.</returns>
        public IEnumerable<IBuildComponent> GetComponents(Type type)
        {
            ValidateUsedComponentTypesAndThrow(type);

            var lookup = new Dictionary<Type, IBuildComponent>();
            var components = BuildConfiguration.GetComponents(type);
            foreach (var component in components)
            {
                lookup[component.GetType()] = component;
            }

            foreach (var pair in m_Components)
            {
                if (type.IsAssignableFrom(pair.Key))
                {
                    lookup[pair.Key] = pair.Value;
                }
            }

            return lookup.Values;
        }

        /// <summary>
        /// Get a flatten list of all components recursively from this container and its dependencies, that matches <typeparamref name="T"/>.
        /// Throws if component type is not in UsedComponents list.
        /// </summary>
        /// <typeparam name="T">Type of the components.</typeparam>
        /// <returns>The list of components.</returns>
        public IEnumerable<T> GetComponents<T>() where T : IBuildComponent => GetComponents(typeof(T)).Cast<T>();

        /// <summary>
        /// Get a flatten list of all component types from this container and its dependencies.
        /// </summary>
        /// <returns>List of component types.</returns>
        public IEnumerable<Type> GetComponentTypes()
        {
            var types = BuildConfiguration.GetComponentTypes();
            return types.Concat(m_Components.Keys).Distinct();
        }

        /// <summary>
        /// Set the value of a component type on this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        /// <param name="value">Value of the component to set.</param>
        public void SetComponent(Type type, IBuildComponent value)
        {
            BuildConfiguration.ValidateComponentTypeAndThrow(type);
            if (type.IsInterface || type.IsAbstract)
            {
                throw new InvalidOperationException($"{nameof(type)} cannot be interface or abstract.");
            }
            m_Components[type] = value;
        }

        /// <summary>
        /// Set the value of a component type on this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="value">Value of the component to set.</param>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>(T value) where T : IBuildComponent => SetComponent(typeof(T), value);

        /// <summary>
        /// Set the value of a component type on this context using an instance created using <see cref="TypeConstruction"/> utility.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type">Type of the component.</param>
        public void SetComponent(Type type) => SetComponent(type, TypeConstruction.Construct<IBuildComponent>(type));

        /// <summary>
        /// Set the value of a component type on this context using an instance created using <see cref="TypeConstruction"/> utility.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public void SetComponent<T>() where T : IBuildComponent => SetComponent(typeof(T));

        /// <summary>
        /// Remove a component type from this context.
        /// NOTE: The build configuration asset is not modified.
        /// </summary>
        /// <param name="type"><see cref="Type"/> of the component.</param>
        public bool RemoveComponent(Type type)
        {
            BuildConfiguration.ValidateComponentTypeAndThrow(type);
            return m_Components.RemoveAll((key, value) => type.IsAssignableFrom(key)) > 0;
        }

        /// <summary>
        /// Remove all <typeparamref name="T"/> components from this container.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        public bool RemoveComponent<T>() where T : IBuildComponent => RemoveComponent(typeof(T));

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public virtual bool HasBuildArtifact(Type buildArtifactType) => BuildConfiguration.HasBuildArtifact(buildArtifactType);

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public virtual bool HasBuildArtifact<T>() where T : class, IBuildArtifact, new() => BuildConfiguration.HasBuildArtifact<T>();

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public virtual IBuildArtifact GetBuildArtifact(Type buildArtifactType) => BuildConfiguration.GetBuildArtifact(buildArtifactType);

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public virtual T GetBuildArtifact<T>() where T : class, IBuildArtifact, new() => BuildConfiguration.GetBuildArtifact<T>();

        /// <summary>
        /// Get all build artifact values.
        /// </summary>
        /// <returns>Enumeration of all build artifact values.</returns>
        public virtual IEnumerable<IBuildArtifact> GetAllBuildArtifacts() => BuildConfiguration.GetAllBuildArtifacts();

        /// <summary>
        /// Get the output build directory override used in this build context.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <returns>The output build directory.</returns>
        public string GetOutputBuildDirectory()
        {
            return BuildPipeline.GetOutputBuildDirectory(BuildConfiguration).ToString();
        }

        /// <summary>
        /// Provides a mechanism for releasing unmanaged resources.
        /// </summary>
        public virtual void Dispose() { }

        internal ContextBase() { }

        internal ContextBase(BuildPipelineBase pipeline, BuildConfiguration config)
        {
            BuildPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
            BuildConfiguration = config ?? throw new ArgumentNullException(nameof(config));

            // Prevent the build configuration asset from being destroyed during a build
            BuildConfiguration.hideFlags |= HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        }

        void ValidateValueTypeAndThrow(Type valueType)
        {
            if (valueType == null)
                throw new ArgumentNullException(nameof(valueType));

            if (valueType == typeof(object))
                throw new InvalidOperationException("Value type cannot be object.");

            if (!valueType.IsClass)
                throw new InvalidOperationException("Value type is not a class.");
        }

        void ValidateUsedComponentTypesAndThrow(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            // 'type' must be in UsedComponents, or be derived from any of them
            if (!BuildPipeline.IsComponentUsed(type))
            {
                throw new InvalidOperationException($"Type '{type.Name}' is missing / is not derived from any type in build pipeline '{BuildPipeline.GetType().Name}' {nameof(BuildPipeline.UsedComponents)} list.");
            }
        }

        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
        public bool IsComponentOverridden(Type type) => IsComponentOverriding(type);

        [Obsolete("IsComponentOverridden has been renamed to IsComponentOverriding. (RemovedAfter 2021-01-01)")]
        public bool IsComponentOverridden<T>() where T : IBuildComponent => IsComponentOverriding(typeof(T));

        [Obsolete("GetLastBuildArtifact has been renamed to GetBuildArtifact. (RemovedAfter 2021-02-01)")]
        public IBuildArtifact GetLastBuildArtifact(Type type) => GetBuildArtifact(type);

        [Obsolete("GetLastBuildArtifact has been renamed to GetBuildArtifact. (RemovedAfter 2021-02-01)")]
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => (T)GetBuildArtifact(typeof(T));

        [Obsolete("GetLastBuildResult will be removed from ContextBase to be re-introduced as GetBuildResult in RunContext and CleanContext. (RemovedAfter 2021-02-01)")]
        public BuildResult GetLastBuildResult()
        {
            if (this is RunContext runContext)
                return runContext.GetBuildResult();
            else if (this is CleanContext cleanContext)
                return cleanContext.GetBuildResult();

            throw new NotSupportedException($"Retrieving {nameof(BuildResult)} on {GetType().FullName} is not supported.");
        }
    }
}
