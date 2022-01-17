using System;
using System.Collections.Generic;

namespace Unity.Build
{
    /// <summary>
    /// Can stores a set of hierarchical build components per type, which can be inherited or overridden using dependencies.
    /// </summary>
    public sealed class BuildConfiguration : HierarchicalComponentContainer<BuildConfiguration, IBuildComponent>
    {
        /// <summary>
        /// File extension for build configuration assets.
        /// </summary>
        public const string AssetExtension = ".buildconfiguration";

        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        public BuildPipelineBase GetBuildPipeline() => TryGetComponent<IBuildPipelineComponent>(out var component) ? component.Pipeline : null;

        /// <summary>
        /// Determine if component is used by the build pipeline of this build configuration.
        /// Returns <see langword="false"/> if this build configuration does not have a build pipeline.
        /// </summary>
        /// <param name="type">The component type.</param>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed(Type type) => GetBuildPipeline()?.IsComponentUsed(type) ?? false;

        /// <summary>
        /// Determine if component is used by the build pipeline of this build configuration.
        /// Returns <see langword="false"/> if this build configuration does not have a build pipeline.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <returns><see langword="true"/> if the component is used by the build pipeline, <see langword="false"/> otherwise.</returns>
        public bool IsComponentUsed<T>() where T : IBuildComponent => IsComponentUsed(typeof(T));

        /// <summary>
        /// Determine if the build pipeline of this build configuration can build.
        /// </summary>
        /// <returns>A result describing if the pipeline can build or not.</returns>
        public BoolResult CanBuild()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.CanBuild(this) : canUse;
        }

        /// <summary>
        /// Run the build pipeline of this build configuration to build the target.
        /// </summary>
        /// <returns>The result of the build pipeline build.</returns>
        public BuildResult Build()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            if (!canUse.Result)
            {
                return BuildResult.Failure(pipeline, this, canUse.Reason);
            }

            var what = !string.IsNullOrEmpty(name) ? $" {name}" : string.Empty;
            using (var progress = new BuildProgress($"Building{what}", "Please wait..."))
            {
                return pipeline.Build(this, progress);
            }
        }

        /// <summary>
        /// Determine if the build pipeline of this build configuration can run.
        /// </summary>
        /// <param name="runTargets">List of run targets to deploy and run on.</param>
        /// <returns>A result describing if the pipeline can run or not.</returns>
        public BoolResult CanRun(params RunTargetBase[] runTargets)
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.CanRun(this, runTargets) : canUse;
        }

        /// <summary>
        /// Run the resulting target from building the build pipeline of this build configuration.
        /// </summary>
        /// <param name="runTargets">List of run targets to deploy and run on.</param>
        /// <returns></returns>
        public RunResult Run(params RunTargetBase[] runTargets)
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.Run(this, runTargets) : RunResult.Failure(pipeline, this, canUse.Reason);
        }

        /// <summary>
        /// Clean the build result from building the build pipeline of this build configuration.
        /// </summary>
        public CleanResult Clean()
        {
            var pipeline = GetBuildPipeline();
            var canUse = CanUsePipeline(pipeline);
            return canUse.Result ? pipeline.Clean(this) : CleanResult.Failure(pipeline, this, canUse.Reason);
        }

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public bool HasBuildArtifact(Type buildArtifactType) => BuildArtifacts.HasBuildArtifact(this, buildArtifactType);

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public bool HasBuildArtifact<T>() where T : class, IBuildArtifact, new() => BuildArtifacts.HasBuildArtifact<T>(this);

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public IBuildArtifact GetBuildArtifact(Type buildArtifactType) => BuildArtifacts.GetBuildArtifact(this, buildArtifactType);

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public T GetBuildArtifact<T>() where T : class, IBuildArtifact, new() => BuildArtifacts.GetBuildArtifact<T>(this);

        /// <summary>
        /// Get all build artifact values.
        /// </summary>
        /// <returns>Enumeration of all build artifact values.</returns>
        public IEnumerable<IBuildArtifact> GetAllBuildArtifacts() => BuildArtifacts.GetAllBuildArtifacts(this);

        /// <summary>
        /// Get the build result of the last <see cref="Build"/> performed.
        /// </summary>
        /// <returns>The build result if found, <see langword="null"/> otherwise.</returns>
        public BuildResult GetBuildResult() => BuildArtifacts.GetBuildResult(this);

        /// <summary>
        /// Clean the build result of the last <see cref="Build"/> performed.
        /// </summary>
        public void CleanBuildArtifact() => BuildArtifacts.CleanBuildArtifact(this);

        /// <summary>
        /// Clean all build results.
        /// </summary>
        public static void CleanAllBuildArtifacts() => BuildArtifacts.CleanAllBuildArtifacts();

        /// <summary>
        /// Get the output build directory override for this build configuration.
        /// The output build directory can be overridden using a <see cref="OutputBuildDirectory"/> component.
        /// </summary>
        /// <returns>The output build directory.</returns>
        public string GetOutputBuildDirectory()
        {
            var pipeline = GetBuildPipeline();
            if (pipeline == null)
                throw new NullReferenceException("The BuildConfiguration must have a BuildPipline in order to retrieve the OutputBuildDirectory");

            return pipeline.GetOutputBuildDirectory(this).ToString();
        }

        BoolResult CanUsePipeline(BuildPipelineBase pipeline)
        {
            if (pipeline == null)
            {
                return BoolResult.False($"No valid build pipeline found for {this.ToHyperLink()}. At least one component that derives from {nameof(IBuildPipelineComponent)} must be present.");
            }
            return BoolResult.True();
        }

        [Obsolete("GetLastBuildArtifact has been renamed to GetBuildArtifact. (RemovedAfter 2021-02-01)")]
        public IBuildArtifact GetLastBuildArtifact(Type type) => GetBuildArtifact(type);

        [Obsolete("GetLastBuildArtifact has been renamed to GetBuildArtifact. (RemovedAfter 2021-02-01)")]
        public T GetLastBuildArtifact<T>() where T : class, IBuildArtifact => (T)GetBuildArtifact(typeof(T));

        [Obsolete("GetLastBuildResult has been renamed to GetBuildResult. (RemovedAfter 2021-02-01)")]
        public BuildResult GetLastBuildResult() => GetBuildResult();
    }

    public static class BuildConfigurationReadOnlyExtensions
    {
        /// <summary>
        /// Retrieve the build pipeline of this build configuration.
        /// </summary>
        /// <returns>The build pipeline if found, otherwise <see langword="null"/>.</returns>
        static public BuildPipelineBase GetBuildPipeline(this BuildConfiguration.ReadOnly config) => config.TryGetComponent<IBuildPipelineComponent>(out var component) ? component.Pipeline : null;
    }
}
