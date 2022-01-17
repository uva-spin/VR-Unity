using Unity.Properties;
using Unity.Properties.UI;
using Unity.Serialization;
using UnityEditor;

namespace Unity.Build.Classic
{
    [FormerName("Unity.Build.Common.ClassicScriptingSettings, Unity.Build.Common")]
    public sealed class ClassicScriptingSettings : IBuildComponent, IBuildComponentInitialize
    {
        [CreateProperty]
        public ScriptingImplementation ScriptingBackend { get; set; } = ScriptingImplementation.Mono2x;

        [CreateProperty, DisplayName("IL2CPP Compiler Configuration")]
        public Il2CppCompilerConfiguration Il2CppCompilerConfiguration { get; set; } = Il2CppCompilerConfiguration.Release;

        [CreateProperty]
        public bool UseIncrementalGC { get; set; } = false;

        // Note: We haven't exposed ScriptingDefineSymbols, ApiCompatibilityLevel, AllowUnsafeCode. Because those affect scripting compilation pipeline, this raises few questions:
        //       - Editor will not reflect the same set compilation result as building to player, which is not very good.
        //       - We need to either decide to have somekind of global project settings (used both in Editor and while building to player) or have
        //         "active build settings" property which would be used as information what kind of enviromnent to simulate, in this it may make sense to but 'ScriptingDefineSymbols, ApiCompatibilityLevel, AllowUnsafeCode' here

        public void Initialize(BuildConfiguration.ReadOnly config)
        {
            var group = config.GetBuildTargetGroup();
            if (group == BuildTargetGroup.Unknown)
                return;

            ScriptingBackend = PlayerSettings.GetScriptingBackend(group);
            Il2CppCompilerConfiguration = PlayerSettings.GetIl2CppCompilerConfiguration(group);
        }
    }
}
