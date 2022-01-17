using Unity.Properties.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Build.Editor
{
    class BuildComponentInspector : Inspector<BuildComponentInspectorData>
    {
        struct Classes
        {
            public const string BaseClass = "build-component";
            public const string FoldoutOpen = BaseClass + "__foldout-open";
            public const string FoldoutClosed = BaseClass + "__foldout-closed";
            public const string BodyOverriding = BaseClass + "__body-overriding";
        }

        static readonly string k_Reset = L10n.Tr("Reset");
        static readonly string k_AddComponent = L10n.Tr("Add Component");
        static readonly string k_RemoveComponent = L10n.Tr("Remove Component");
        static readonly string k_RemoveOverrides = L10n.Tr("Remove Overrides");
        static readonly string k_GoToSource = L10n.Tr("Go to Configuration");

        VisualElement m_Foldout;
        Label m_Name;
        VisualElement m_Warning;
        Button m_AddButton;
        Button m_OptionsButton;
        VisualElement m_BodyElement;
        PropertyElement m_ValueElement;

        string FoldedKey => nameof(BuildComponentInspector) + "." + Target.ComponentType.FullName;

        bool Folded
        {
            get => SessionState.GetBool(FoldedKey, false);
            set => SessionState.SetBool(FoldedKey, value);
        }

        public override VisualElement Build()
        {
            var root = Resources.BuildComponent.Clone();
            var header = root.Q("header");
            header.RegisterCallback<ClickEvent>(e =>
            {
                Folded = !Folded;
                if (!Target.IsTagComponent)
                {
                    m_Foldout.AddToClassList(Folded, Classes.FoldoutClosed, Classes.FoldoutOpen);
                }
                m_BodyElement.style.display = Folded || Target.IsTagComponent ? DisplayStyle.None : DisplayStyle.Flex;
                e.StopPropagation();
            });

            m_Foldout = header.Q("foldout");
            if (!Target.IsTagComponent)
            {
                m_Foldout.AddToClassList(Folded, Classes.FoldoutClosed, Classes.FoldoutOpen);
            }

            m_Name = header.Q<Label>("name");
            m_Name.SetEnabled(Target.IsComponentPresent);

            m_Warning = header.Q("warning");
            m_Warning.tooltip = "This component is not used by the build pipeline.";
            m_Warning.style.display = Target.ConfigHasPipeline && !Target.IsComponentUsed ? DisplayStyle.Flex : DisplayStyle.None;

            m_AddButton = header.Q<Button>("add");
            m_AddButton.SetEnabled(!Target.IsReadOnly);
            m_AddButton.RegisterCallback<ClickEvent>(e =>
            {
                Target.SetComponent();
                e.StopPropagation();
            });
            m_AddButton.tooltip = k_AddComponent;
            m_AddButton.style.display = Target.IsComponentPresent ? DisplayStyle.None : DisplayStyle.Flex;

            m_OptionsButton = header.Q<Button>("options");
            m_OptionsButton.RegisterCallback<ClickEvent>(e =>
            {
                var menu = new GenericMenu();
                menu.AddItem(k_Reset, false, !Target.IsReadOnly, () => Target.SetComponent());
                menu.AddSeparator();

                var isUsedComponentOverride = Target.ConfigHasPipeline && Target.IsComponentUsed && !Target.IsPipelineComponent && !Target.IsComponentInherited;
                if (Target.IsComponentOverriding || isUsedComponentOverride)
                {
                    menu.AddItem(k_RemoveOverrides, false, !Target.IsReadOnly, () => Target.RemoveComponent());
                }
                else if (!Target.IsComponentInherited)
                {
                    menu.AddItem(k_RemoveComponent, false, !Target.IsReadOnly, () => Target.RemoveComponent());
                }
                else
                {
                    menu.AddDisabledItem(k_RemoveComponent);
                }

                menu.AddItem(k_GoToSource, false, Target.IsComponentInherited || Target.IsComponentOverriding, () =>
                {
                    if (Target.ComponentSource != null && Target.ComponentSource)
                    {
                        Selection.activeObject = Target.ComponentSource;
                    }
                });

                menu.DropDown(m_OptionsButton.worldBound);
                e.StopPropagation();
            });
            m_OptionsButton.style.display = Target.IsComponentPresent ? DisplayStyle.Flex : DisplayStyle.None;

            m_BodyElement = root.Q("body");
            m_BodyElement.AddToClassList(Target.IsComponentOverriding, Classes.BodyOverriding);
            m_BodyElement.RegisterCallback<ClickEvent>(OnBodyClicked, TrickleDown.TrickleDown);
            m_BodyElement.style.display = Folded || Target.IsTagComponent ? DisplayStyle.None : DisplayStyle.Flex;

            m_ValueElement = root.Q<PropertyElement>("value");
            m_ValueElement.OnChanged += (element, path) => Target.SetComponent(element.GetTarget<IBuildComponent>());
            m_ValueElement.SetEnabled(!Target.IsReadOnly && Target.IsComponentPresent);
            root.RegisterCallback<GeometryChangedEvent>(e => StylingUtility.AlignInspectorLabelWidth(m_ValueElement));

            return root;
        }

        public override void Update()
        {
            var component = Target;
            var folded = Folded;

            if (!component.IsTagComponent)
            {
                m_Foldout.AddToClassList(folded, Classes.FoldoutClosed, Classes.FoldoutOpen);
            }

            m_Name.SetEnabled(component.IsComponentPresent);
            m_Warning.style.display = component.ConfigHasPipeline && !component.IsComponentUsed ? DisplayStyle.Flex : DisplayStyle.None;

            m_AddButton.SetEnabled(!component.IsReadOnly);
            m_AddButton.style.display = component.IsComponentPresent ? DisplayStyle.None : DisplayStyle.Flex;

            m_OptionsButton.style.display = component.IsComponentPresent ? DisplayStyle.Flex : DisplayStyle.None;

            m_BodyElement.AddToClassList(component.IsComponentOverriding, Classes.BodyOverriding);
            m_BodyElement.style.display = folded || component.IsTagComponent ? DisplayStyle.None : DisplayStyle.Flex;

            m_ValueElement.SetEnabled(!component.IsReadOnly && component.IsComponentPresent);
        }

        static void OnBodyClicked(ClickEvent e)
        {
            var element = (VisualElement)e.target;
            var foldouts = element.Query<Foldout>().ToList();
            foreach (var foldout in foldouts)
            {
                if (element == foldout)
                    continue;

                if (!foldout.Q<Toggle>().worldBound.Contains(e.position))
                    continue;

                foldout.value = !foldout.value;
                break;
            }

            var fields = element.Query<ObjectField>().ToList();
            foreach (var field in fields)
            {
                var display = field.Q(className: "unity-object-field-display");
                if (null == display)
                    continue;

                if (!display.worldBound.Contains(e.position))
                    continue;

                if (e.clickCount == 1)
                {
                    EditorGUIUtility.PingObject(field.value);
                }
                else
                {
                    var value = field.value;
                    if (null != value && value)
                    {
                        Selection.activeObject = value;
                    }
                }

                break;
            }
        }
    }
}
