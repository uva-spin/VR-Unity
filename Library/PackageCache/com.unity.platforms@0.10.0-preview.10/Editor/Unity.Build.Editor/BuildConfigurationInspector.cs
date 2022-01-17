using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIExtras;

namespace Unity.Build.Editor
{
    class BuildConfigurationInspector : Inspector<BuildConfigurationInspectorData>
    {
        const string k_ShowPipelineUsedComponentsKey = nameof(BuildConfigurationInspector) + "." + nameof(ShowUsedComponents);
        const string k_CurrentActionIndexKey = nameof(BuildConfigurationInspector) + "." + nameof(CurrentActionIndex);
        const string k_DependenciesFoldoutOpenKey = nameof(BuildConfigurationInspector) + "." + nameof(DependenciesFoldoutOpen);

        static readonly string k_ShowUsedComponents = L10n.Tr("Show Suggested Components");

        struct Classes
        {
            public const string BaseClass = "build-configuration";
            public const string ActionPopup = BaseClass + "__action-popup";
        }

        struct BuildAction : IEquatable<BuildAction>
        {
            public string Name { get; }
            public Action<BuildConfiguration> Action { get; }
            public BuildAction(string name, Action<BuildConfiguration> action)
            {
                Name = name;
                Action = action;
            }
            public bool Equals(BuildAction other) => Name == other.Name;
        }

        static readonly BuildAction s_BuildAction = new BuildAction(L10n.Tr("Build"), config =>
        {
            config.Build()?.LogResult();
        });

        static readonly BuildAction s_BuildAndRunAction = new BuildAction(L10n.Tr("Build and Run"), config =>
        {
            var buildResult = config.Build();
            buildResult.LogResult();
            if (buildResult.Failed)
            {
                return;
            }

            using (var runResult = config.Run())
            {
                runResult.LogResult();
            }
        });

        static readonly BuildAction s_RunAction = new BuildAction(L10n.Tr("Run"), config =>
        {
            using (var result = config.Run())
            {
                result.LogResult();
            }
        });

        static readonly BuildAction[] s_Actions = new[] { s_BuildAction, s_BuildAndRunAction, s_RunAction };
        static readonly string k_Dependencies = L10n.Tr("Shared Configurations");
        static readonly string k_AddDependency = L10n.Tr("Add Configuration");
        static readonly string k_AddComponent = L10n.Tr("Add Component");

        SearchElement m_Search;
        VisualElement m_Components;
        Dictionary<BuildComponentInspectorData, PropertyElement> m_ComponentsMap;
        bool m_SearchBindingRegistered;

        public static bool ShowUsedComponents
        {
            get => EditorPrefs.GetBool(k_ShowPipelineUsedComponentsKey, false);
            set => EditorPrefs.SetBool(k_ShowPipelineUsedComponentsKey, value);
        }

        bool DependenciesFoldoutOpen
        {
            get => SessionState.GetBool(k_DependenciesFoldoutOpenKey, false);
            set => SessionState.SetBool(k_DependenciesFoldoutOpenKey, value);
        }

        static int CurrentActionIndex
        {
            get => EditorPrefs.GetInt(k_CurrentActionIndexKey, Array.IndexOf(s_Actions, s_BuildAndRunAction));
            set => EditorPrefs.SetInt(k_CurrentActionIndexKey, value);
        }

        static BuildAction CurrentAction => s_Actions[CurrentActionIndex];

        public static bool IsCurrentActionBuildAndRun => CurrentAction.Equals(s_BuildAndRunAction);

        public override VisualElement Build()
        {
            var root = Resources.BuildConfiguration.Clone();
            root.AddToClassList(Classes.BaseClass);

            var header = root.Q("header");
            var optionsButton = header.Q<Button>("options");
            optionsButton.RegisterCallback<ClickEvent>(e =>
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(k_ShowUsedComponents), ShowUsedComponents, () =>
                {
                    ShowUsedComponents = !ShowUsedComponents;
                    Target.RefreshComponents();
                });
                menu.DropDown(optionsButton.worldBound);
                e.StopPropagation();
            });

            var actions = header.Q("actions");
            var actionButton = actions.Q<Button>("action");
            actionButton.text = CurrentAction.Name;
            actionButton.clickable.clicked += () =>
            {
                var context = GetContext<BuildConfigurationContext>();
                var config = context.ImporterEditor.HandleUnappliedImportSettings();
                if (config != null && config)
                {
                    CurrentAction.Action(config);
                }
            };

            var actionPopup = new PopupField<BuildAction>(s_Actions.ToList(), CurrentActionIndex, a => string.Empty, a => a.Name);
            actionPopup.AddToClassList(Classes.ActionPopup);
            actionPopup.RegisterValueChangedCallback(evt =>
            {
                CurrentActionIndex = Array.IndexOf(s_Actions, evt.newValue);
                actionButton.text = CurrentAction.Name;
            });
            actions.Add(actionPopup);

            var dependenciesFoldout = root.Q<Foldout>("dependencies");
            dependenciesFoldout.RegisterValueChangedCallback(e => DependenciesFoldoutOpen = e.newValue);
            dependenciesFoldout.text = k_Dependencies;
            dependenciesFoldout.value = DependenciesFoldoutOpen;

            var dependencies = dependenciesFoldout.Q<ListView>();
            dependencies.SetEnabled(!Target.IsReadOnly);
            dependencies.itemsSource = Target.Dependencies;
            dependencies.makeItem = () =>
            {
                var dependency = Resources.BuildConfigurationDependency.Clone();
                var objectField = dependency.Q<ObjectField>("object");
                objectField.objectType = typeof(BuildConfiguration);
                objectField.allowSceneObjects = false;
                return dependency;
            };
            dependencies.bindItem = (element, index) =>
            {
                var objectField = element.Q<ObjectField>("object");
                objectField.RegisterValueChangedCallback(e => Target.Dependencies[index] = e.newValue as BuildConfiguration);
                objectField.value = Target.Dependencies[index];
                element.Q<Button>("remove").clickable = new Clickable(() =>
                {
                    Target.Dependencies.RemoveAt(index);
                    dependencies.style.minHeight = Target.Dependencies.Count * dependencies.itemHeight;
                    dependencies.Refresh();
                });
            };
            dependencies.itemHeight = 22;
            dependencies.selectionType = SelectionType.Single;
            dependencies.reorderable = true;
            dependencies.style.flexGrow = 1;
            dependencies.style.minHeight = Target.Dependencies.Count * dependencies.itemHeight;

            var addDependency = dependenciesFoldout.Q<Button>("add");
            addDependency.SetEnabled(!Target.IsReadOnly);
            addDependency.text = "+ " + k_AddDependency;
            addDependency.clickable.clicked += () =>
            {
                Target.Dependencies.Add(default);
                dependencies.style.minHeight = Target.Dependencies.Count * dependencies.itemHeight;
                dependencies.Refresh();
            };

            m_Search = root.Q<SearchElement>("search");
            m_Search.AddSearchDataCallback<BuildComponentInspectorData>(c => new[] { c.ComponentName }.Concat(c.FieldNames));

            m_Components = root.Q("components");
            m_ComponentsMap = new Dictionary<BuildComponentInspectorData, PropertyElement>(Target.Components.Length);
            UpdateComponents();

            var addComponentButton = root.Q<Button>("add-component");
            addComponentButton.SetEnabled(!Target.IsReadOnly);
            addComponentButton.text = k_AddComponent;
            addComponentButton.clickable.clicked += () =>
            {
                var items = new List<SearchView.Item>();
                var types = TypeCache.GetTypesDerivedFrom<IBuildComponent>();
                foreach (var type in types)
                {
                    if (type.IsAbstract || type.IsInterface ||
                        type.HasAttribute<HideInInspector>() ||
                        type.HasAttribute<ObsoleteAttribute>())
                    {
                        continue;
                    }

                    if (Target.ComponentTypes.Contains(type))
                    {
                        continue;
                    }

                    string name;
                    if (type.HasAttribute<DisplayNameAttribute>())
                    {
                        name = type.GetCustomAttribute<DisplayNameAttribute>().Name;
                    }
                    else
                    {
                        name = ObjectNames.NicifyVariableName(type.Name);
                    }
                    var category = type.Namespace ?? "Global";

                    items.Add(new SearchView.Item
                    {
                        Path = !string.IsNullOrEmpty(category) ? $"{category}/{name}" : name,
                        Data = type
                    });
                }
                items = items.OrderBy(item => item.Path).ToList();

                SearchWindow searchWindow = SearchWindow.Create();
                searchWindow.Title = "Component";
                searchWindow.Items = items;
                searchWindow.OnSelection += item => Target.SetComponent((Type)item.Data);

                var rect = EditorWindow.focusedWindow.position;
                var button = addComponentButton.worldBound;
                searchWindow.position = new Rect(rect.x + button.x, rect.y + button.y + button.height, button.width, 315);
                searchWindow.ShowPopup();
            };

            Target.OnComponentsChanged += () =>
            {
                m_Search.value = string.Empty;
                m_Components.Clear();
                m_ComponentsMap.Clear();
                UpdateComponents();
            };
            Target.Dependencies.OnChanged += () => Target.RefreshComponents();

            return root;
        }

        public override void Update()
        {
            if (!m_SearchBindingRegistered)
            {
                var handler = m_Search.GetUxmlSearchHandler() as SearchHandler<BuildComponentInspectorData>;
                if (handler == null)
                {
                    return;
                }

                handler.OnBeginSearch += query =>
                {
                    foreach (var element in m_ComponentsMap.Values)
                    {
                        element.style.display = DisplayStyle.None;
                    }
                };

                handler.OnFilter += (query, filtered) =>
                {
                    foreach (var component in filtered)
                    {
                        m_ComponentsMap[component].style.display = DisplayStyle.Flex;
                    }
                };

                m_SearchBindingRegistered = true;
            }
        }

        void UpdateComponents()
        {
            foreach (var component in Target.Components)
            {
                var element = new PropertyElement();
                element.SetTarget(component);
                m_Components.Add(element);
                m_ComponentsMap.Add(component, element);
            }
        }
    }
}
