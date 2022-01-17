using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEditor;

namespace Unity.Build.Editor
{
    class BuildConfigurationInspectorData
    {
        BuildConfiguration Source { get; }
        BuildConfiguration Target { get; }
        public bool IsReadOnly { get; }
        public Type[] ComponentTypes { get; private set; }
        public Action OnComponentsChanged = delegate { };

        [CreateProperty]
        public string Name { get; }

        [CreateProperty]
        public BuildConfigurationDependencyList Dependencies { get; set; }

        [CreateProperty]
        public BuildComponentInspectorData[] Components { get; private set; }

        [CreateProperty]
        public List<BuildComponentInspectorData> FilteredComponents { get; private set; }

        public BuildConfigurationInspectorData(BuildConfiguration source, BuildConfiguration target)
        {
            Source = source;
            Target = target;
            IsReadOnly = !AssetDatabase.IsOpenForEdit(source);
            Name = $"{Source.name} ({nameof(BuildConfiguration)})";
            Dependencies = new BuildConfigurationDependencyList(source, target);
            RefreshComponents();
        }

        public bool HasPipeline => Target.GetBuildPipeline() != null;
        public bool HasComponent(Type type) => Target.HasComponent(type);
        public bool IsComponentInherited(Type type) => Target.IsComponentInherited(type);
        public bool IsComponentOverriding(Type type) => Target.IsComponentOverriding(type);
        public bool IsComponentUsed(Type type) => Target.IsComponentUsed(type);
        public BuildConfiguration GetComponentSource(Type type) => Target.GetComponentSource(type, true);

        public void SetComponent(Type type)
        {
            Target.SetComponent(type);
            RefreshComponents();
        }

        public void SetComponent(IBuildComponent value)
        {
            Target.SetComponent(value);
        }

        public void RemoveComponent(Type type)
        {
            Target.RemoveComponent(type);
            RefreshComponents();
        }

        public void RefreshComponents()
        {
            var components = Target.GetComponents();
            if (BuildConfigurationInspector.ShowUsedComponents)
            {
                var pipeline = Target.GetBuildPipeline();
                if (pipeline != null)
                {
                    components = components.Concat(pipeline.UsedComponents
                        .Where(c => !c.IsAbstract && !c.IsInterface)
                        .Where(c => !Target.HasComponent(c))
                        .Select(c => Target.GetComponentOrDefault(c)));
                }
            }

            Components = components.Select(c => new BuildComponentInspectorData(this, c)).ToArray();
            ComponentTypes = Components.Select(c => c.ComponentType).ToArray();
            FilteredComponents = Components.ToList();

            OnComponentsChanged?.Invoke();
        }
    }
}
