using Unity.Properties.UI;
using UnityEditor;
using UnityEngine.UIElements;

namespace Unity.Build.Classic
{
    sealed class ClassicScriptingSettingsInspector : Inspector<ClassicScriptingSettings>
    {
        VisualElement m_Il2CppCompilerConfiguration;

        public override VisualElement Build()
        {
            var root = new VisualElement();
            DoDefaultGui(root, nameof(ClassicScriptingSettings.ScriptingBackend));
            DoDefaultGui(root, nameof(ClassicScriptingSettings.Il2CppCompilerConfiguration));
            DoDefaultGui(root, nameof(ClassicScriptingSettings.UseIncrementalGC));

            m_Il2CppCompilerConfiguration = root.Q(nameof(ClassicScriptingSettings.Il2CppCompilerConfiguration));
            m_Il2CppCompilerConfiguration.SetEnabled(Target.ScriptingBackend == ScriptingImplementation.IL2CPP);

            return root;
        }

        public override void Update()
        {
            m_Il2CppCompilerConfiguration.SetEnabled(Target.ScriptingBackend == ScriptingImplementation.IL2CPP);
        }
    }
}
