using Unity.Properties;
using Unity.Serialization;
using Unity.Serialization.Json;
using UnityEngine;

namespace Unity.Build.DotsRuntime
{
    [FormerName("Unity.Entities.Runtime.Build.IL2CPPSettings, Unity.Entities.Runtime.Build")]
    public sealed class IL2CPPSettings : IDotsRuntimeBuildModifier
    {
        [CreateProperty]
        [Tooltip("Controls if script debugging in IL2CPP builds is enabled. UseBuildConfiguration: Debug and Develop builds enable ScriptDebugging.")]
        public BuildSettingToggle ScriptDebugging = BuildSettingToggle.UseBuildConfiguration;

        [CreateProperty]
        public bool WaitForDebugger { get; set; } = false;

        public void Modify(JsonObject jsonObject)
        {
            jsonObject["EnableManagedDebugging"] = ScriptDebugging.ToString();
            jsonObject["WaitForManagedDebugger"] = WaitForDebugger;
        }
    }
}
