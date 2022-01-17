using UnityEditor;

namespace Unity.Build.Classic
{
    internal static class ConstructExtensions
    {
        public static BuildTargetGroup GetBuildTargetGroup(this BuildConfiguration.ReadOnly config)
        {
            if (!config.TryGetComponent<IBuildPipelineComponent>(out var value))
                return BuildTargetGroup.Unknown;

            var profile = value as ClassicBuildProfile;
            if (profile == null)
                return BuildTargetGroup.Unknown;

            return BuildPipeline.GetBuildTargetGroup(profile.Platform.GetBuildTarget());
        }
    }
}
