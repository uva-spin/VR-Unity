using Unity.Build.Classic.Private.MissingPipelines;
using UnityEngine;

namespace Unity.Build.Classic.Private
{
    class BuildPipelineSelector : BuildPipelineSelectorBase
    {
        internal static bool IsBuildPipelineValid(ClassicPipelineBase pipeline, Platform platform)
        {
            var namezpace = pipeline.GetType().Namespace;

            if (string.IsNullOrEmpty(namezpace))
                return false;

            return pipeline.Platform.Equals(platform) &&
                   namezpace.StartsWith("Unity.Build.") &&
                   (namezpace.EndsWith(".Classic") || namezpace.EndsWith(".Classic.Private.MissingPipelines")) &&
                   !namezpace.Contains("Test");
        }

        private BuildPipelineBase ConstructPipeline(Platform platform)
        {
            return TypeConstructionUtility.TryConstructTypeDerivedFrom<ClassicNonIncrementalPipelineBase>(p =>
                p.GetType() != typeof(MissingNonIncrementalPipeline) && IsBuildPipelineValid(p, platform), out var pipeline) ? pipeline : null;
        }

        public override BuildPipelineBase SelectFor(Platform platform)
        {
            if (platform == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(platform.PackageName))
            {
                // Sanity check
                var potentialPipeline = ConstructPipeline(platform);
                if (potentialPipeline != null)
                    Debug.LogWarning($"{platform.Name} specifies that its platform is not implemented, yet a pipeline {potentialPipeline.GetType().FullName} was found");

                return new MissingNonIncrementalPipeline(platform);
            }

            // This platform requires its package to be included
            // Since the package was not include, we won't be able to find the platform pipeline
            if (platform.GetType() == typeof(MissingPlatform))
                return null;

            return ConstructPipeline(platform);
        }
    }
}
