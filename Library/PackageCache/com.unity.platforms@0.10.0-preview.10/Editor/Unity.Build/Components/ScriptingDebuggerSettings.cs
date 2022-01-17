using Unity.Properties;
using Unity.Serialization;

namespace Unity.Build.Common
{
    [FormerName("Unity.Build.Common.ScriptingDebuggerSettings, Unity.Build.Common")]
    public sealed class ScriptingDebuggerSettings : IBuildComponent
    {
        [CreateProperty]
        public bool WaitForManagedDebugger { get; set; } = false;
    }
}
