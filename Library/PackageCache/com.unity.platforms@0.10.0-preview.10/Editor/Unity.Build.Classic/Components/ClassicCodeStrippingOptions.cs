using Unity.Properties;
using UnityEditor;

namespace Unity.Build.Classic
{
    public sealed class ClassicCodeStrippingOptions : IBuildComponent, IBuildComponentInitialize
    {
        [CreateProperty]
        public bool StripEngineCode { get; set; } = true;

        [CreateProperty]
        public ManagedStrippingLevel ManagedStrippingLevel { get; set; } = ManagedStrippingLevel.Disabled;

        public void Initialize(BuildConfiguration.ReadOnly config)
        {
            var group = config.GetBuildTargetGroup();
            if (group == BuildTargetGroup.Unknown)
                return;

            StripEngineCode = PlayerSettings.stripEngineCode;
            ManagedStrippingLevel = PlayerSettings.GetManagedStrippingLevel(group);
        }
    }
}
