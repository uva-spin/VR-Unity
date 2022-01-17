using Unity.Build.Editor;
using Unity.Properties.UI;
using UnityEngine.UIElements;

namespace Unity.Build.Classic
{
    sealed class ClassicBuildProfileInspector : Inspector<ClassicBuildProfile>
    {
        VisualElement m_HelpBox;
        Label m_Message;

        public override VisualElement Build()
        {
            var root = Resources.ClassicBuildProfile.Clone();

            var platform = root.Q("platform");
            DoDefaultGui(platform, nameof(ClassicBuildProfile.Platform));

            m_HelpBox = root.Q("helpbox");
            m_Message = m_HelpBox.Q<Label>("message");
            UpdateHelpBox();

            var configuration = root.Q("configuration");
            DoDefaultGui(configuration, nameof(ClassicBuildProfile.Configuration));

            return root;
        }

        public override void Update()
        {
            UpdateHelpBox();
        }

        public void UpdateHelpBox()
        {
            if (Target.Pipeline == null && Target.Platform != null)
            {
                string message;
                if (!string.IsNullOrEmpty(Target.Platform.PackageName))
                {
                    message = $"Platform {Target.Platform.DisplayName} requires package '{Target.Platform.PackageName}' to be installed.";
                }
                else
                {
                    message = $"Platform {Target.Platform.DisplayName} requires a package to be installed.";
                }

                if (m_Message.text != message)
                {
                    m_Message.text = message;
                }
                m_HelpBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (m_Message.text != string.Empty)
                {
                    m_Message.text = string.Empty;
                }
                m_HelpBox.style.display = DisplayStyle.None;
            }
        }
    }
}
