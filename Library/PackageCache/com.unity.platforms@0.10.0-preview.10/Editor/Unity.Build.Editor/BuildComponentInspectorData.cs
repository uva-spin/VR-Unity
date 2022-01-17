using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using Unity.Properties.UI;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Editor
{
    class BuildComponentInspectorData
    {
        readonly BuildConfigurationInspectorData m_Config;

        public Type ComponentType { get; }
        public BuildConfiguration ComponentSource { get; }
        public bool IsReadOnly => m_Config.IsReadOnly;
        public bool IsComponentPresent { get; private set; }
        public bool IsComponentInherited { get; private set; }
        public bool IsComponentOverriding { get; private set; }
        public bool IsComponentUsed { get; private set; }
        public bool IsPipelineComponent { get; }
        public bool IsTagComponent { get; }
        public bool ConfigHasPipeline { get; private set; }

        [CreateProperty]
        public Texture2D Icon { get; }

        [CreateProperty]
        public string ComponentName { get; }

        [CreateProperty]
        public string[] FieldNames { get; }

        [CreateProperty]
        public IBuildComponent Value { get; set; }

        public BuildComponentInspectorData(BuildConfigurationInspectorData config, IBuildComponent value)
        {
            m_Config = config;

            ComponentType = value.GetType();
            ComponentSource = config.GetComponentSource(ComponentType);
            IsComponentPresent = config.HasComponent(ComponentType);
            IsComponentInherited = config.IsComponentInherited(ComponentType);
            IsComponentOverriding = config.IsComponentOverriding(ComponentType);
            IsComponentUsed = config.IsComponentUsed(ComponentType);
            ConfigHasPipeline = config.HasPipeline;
            ComponentName = ObjectNames.NicifyVariableName(ComponentType.Name);

            var visitor = new ComponentVisitor();
            PropertyContainer.Visit(ref value, visitor);
            FieldNames = visitor.FieldNames;

            IsPipelineComponent = typeof(IBuildPipelineComponent).IsAssignableFrom(ComponentType);
            IsTagComponent = FieldNames.Length == 0;
            Icon = Resources.BuildComponentIcon;

            Value = value;
        }

        public void SetComponent()
        {
            // This will add the component to the config and rebuild it
            m_Config.SetComponent(ComponentType);
        }

        public void SetComponent(IBuildComponent value)
        {
            m_Config.SetComponent(value);
            Update(); // Update since we're not rebuilding
        }

        public void RemoveComponent()
        {
            // This will remove the component from the config and rebuild it
            m_Config.RemoveComponent(ComponentType);
        }

        void Update()
        {
            IsComponentPresent = m_Config.HasComponent(ComponentType);
            IsComponentInherited = m_Config.IsComponentInherited(ComponentType);
            IsComponentOverriding = m_Config.IsComponentOverriding(ComponentType);
            IsComponentUsed = m_Config.IsComponentUsed(ComponentType);
            ConfigHasPipeline = m_Config.HasPipeline;
        }

        class ComponentVisitor : PropertyVisitor
        {
            readonly HashSet<string> m_FieldNames = new HashSet<string>();

            public string[] FieldNames => m_FieldNames.ToArray();

            protected override void VisitProperty<TContainer, TValue>(Property<TContainer, TValue> property, ref TContainer container, ref TValue value)
            {
                if (!property.HasAttribute<HideInInspector>() && !(container is IList<TValue>))
                {
                    string name;
                    if (property.HasAttribute<DisplayNameAttribute>())
                    {
                        name = property.GetAttribute<DisplayNameAttribute>().Name;
                    }
                    else
                    {
                        name = ObjectNames.NicifyVariableName(property.Name);
                    }
                    m_FieldNames.Add(name);
                }
                base.VisitProperty(property, ref container, ref value);
            }
        }
    }
}
