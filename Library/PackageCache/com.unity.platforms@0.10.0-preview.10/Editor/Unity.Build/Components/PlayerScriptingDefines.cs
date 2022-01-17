using System;
using Unity.Properties;
using Unity.Serialization;

namespace Unity.Build.Common
{
    [FormerName("Unity.Build.Common.PlayerScriptingDefines, Unity.Build.Common")]
    public sealed class PlayerScriptingDefines : IBuildComponent
    {
        [CreateProperty]
        public string[] Defines { get; set; } = Array.Empty<string>();
    }
}
