using System;
using System.IO;
using System.Linq;
using Unity.Build.Common;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Classic.Private
{
    public abstract class ClassicPipelineBase : BuildPipelineBase
    {
        ClassicBuildPipelineCustomizer[] CreateCustomizers() =>
            TypeConstructionUtility.ConstructTypesDerivedFrom<ClassicBuildPipelineCustomizer>().ToArray();

        Type[] CustomizersUsedComponents =>
            CreateCustomizers().SelectMany(customizer => customizer.UsedComponents).Distinct().ToArray();

        public override Type[] UsedComponents =>
            base.UsedComponents.Concat(CustomizersUsedComponents).Distinct().ToArray();

        protected BuildTarget BuildTarget => Platform.GetBuildTarget();

        public abstract Platform Platform { get; }

        protected virtual void PrepareContext(BuildContext context)
        {
            var buildType = context.GetComponentOrDefault<ClassicBuildProfile>().Configuration;
            bool isDevelopment = buildType == BuildType.Debug || buildType == BuildType.Develop;
            var playbackEngineDirectory = UnityEditor.BuildPipeline.GetPlaybackEngineDirectory(BuildTarget, isDevelopment ? BuildOptions.Development : BuildOptions.None);

            var customizers = CreateCustomizers();
            foreach (var customizer in customizers)
                customizer.m_Info = new CustomizerInfoImpl(context);
            var workingDir = Path.GetFullPath(Path.Combine(Application.dataPath, $"../Library/BuildWorkingDir/{context.BuildConfigurationName}"));
            Directory.CreateDirectory(workingDir);
            context.SetValue(new ClassicSharedData()
            {
                BuildTarget = BuildTarget,
                PlaybackEngineDirectory = playbackEngineDirectory,
                BuildToolsDirectory = Path.Combine(playbackEngineDirectory, "Tools"),
                DevelopmentPlayer = isDevelopment,
                Customizers = customizers,
                WorkingDirectory = workingDir.ToString()
            });

            var scenes = context.GetComponentOrDefault<SceneList>().GetScenePathsForBuild();
            foreach (var modifier in customizers)
                scenes = modifier.ModifyEmbeddedScenes(scenes);
            context.SetValue(new EmbeddedScenesValue() { Scenes = scenes });

            foreach (var customizer in customizers)
                customizer.OnBeforeBuild();
        }

        protected override CleanResult OnClean(CleanContext context)
        {
            var buildDirectory = context.GetOutputBuildDirectory();
            if (Directory.Exists(buildDirectory))
                Directory.Delete(buildDirectory, true);
            return context.Success();
        }
    }
}
