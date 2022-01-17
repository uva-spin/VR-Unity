using Unity.Properties.UI;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Unity.Build.Common
{
    sealed class ScreenOrientationsInspector : Inspector<ScreenOrientations>
    {
        EnumField m_defaultOrientation;
        VisualElement m_allowedOrientations;

        public override VisualElement Build()
        {
            var root = new VisualElement();
            DoDefaultGui(root, nameof(ScreenOrientations.DefaultOrientation));

            m_allowedOrientations = new VisualElement();
            var allowedOrientationsHeader = new TextElement();
            allowedOrientationsHeader.text = "Allowed Orientations for Auto Rotation";
            m_allowedOrientations.Add(allowedOrientationsHeader);
            DoDefaultGui(m_allowedOrientations, nameof(ScreenOrientations.AllowAutoRotateToPortrait));
            DoDefaultGui(m_allowedOrientations, nameof(ScreenOrientations.AllowAutoRotateToReversePortrait));
            DoDefaultGui(m_allowedOrientations, nameof(ScreenOrientations.AllowAutoRotateToLandscape));
            DoDefaultGui(m_allowedOrientations, nameof(ScreenOrientations.AllowAutoRotateToReverseLandscape));
            root.Add(m_allowedOrientations);

            m_defaultOrientation = root.Q<EnumField>(nameof(ScreenOrientations.DefaultOrientation));
            m_allowedOrientations.Q<Toggle>(nameof(ScreenOrientations.AllowAutoRotateToPortrait)).label = "Portrait";
            m_allowedOrientations.Q<Toggle>(nameof(ScreenOrientations.AllowAutoRotateToReversePortrait)).label = "Reverse Portrait (Upside Down)";
            m_allowedOrientations.Q<Toggle>(nameof(ScreenOrientations.AllowAutoRotateToLandscape)).label = "Landscape (Left)";
            m_allowedOrientations.Q<Toggle>(nameof(ScreenOrientations.AllowAutoRotateToReverseLandscape)).label = "Reverse Landscape (Right)";

            return root;
        }

        public override void Update()
        {
            var style = (UIOrientation)m_defaultOrientation.value == UIOrientation.AutoRotation ? DisplayStyle.Flex : DisplayStyle.None;
            m_allowedOrientations.style.display = style;
        }
    }
}
