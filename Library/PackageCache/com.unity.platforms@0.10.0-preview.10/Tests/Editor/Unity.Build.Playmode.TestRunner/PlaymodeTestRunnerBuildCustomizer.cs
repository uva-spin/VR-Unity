#if ENABLE_PLAYMODE_EXTENSION
using System;
using Unity.Build.Classic;
using UnityEditor;
using UnityEditor.TestTools.TestRunner;
using UnityEngine;

namespace Unity.Build.Playmode.TestRunner
{
    /// <summary>
    /// Dummy component which purpose is to tell the PlaymodeTestRunnerBuildCustomizer that we're building a player for playmode tests
    /// </summary>
    [HideInInspector]
    class PlaymodeTestRunnerComponent : IBuildComponent
    {

    }

    class PlaymodeTestRunnerBuildCustomizer : ClassicBuildPipelineCustomizer
    {
        public override Type[] UsedComponents { get; } = new[] {typeof(PlaymodeTestRunnerComponent)};

        public override BuildOptions ProvideBuildOptions()
        {
            if (!Context.HasComponent<PlaymodeTestRunnerComponent>())
                return base.ProvideBuildOptions();
            
            var compressionOptions = PlayerLauncherBuildOptions.GetCompressionBuildOptions(BuildPipeline.GetBuildTargetGroup(BuildTarget), BuildTarget);
            // Note: Don't specify BuildOptions.ConnectToHost or BuildOptions.WaitForPlayerConnection, it's being handled by PlayerConnectionSettings component

            return BuildOptions.IncludeTestAssemblies | BuildOptions.StrictMode | compressionOptions;
        }
    }
}
#endif
