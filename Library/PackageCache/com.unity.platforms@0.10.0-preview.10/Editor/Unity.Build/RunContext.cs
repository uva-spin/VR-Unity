using System;

namespace Unity.Build
{
    /// <summary>
    /// Holds contextual information when running a build pipeline.
    /// </summary>
    public sealed class RunContext : ContextBase
    {
        /// <summary>
        /// Optional list of run targets to deploy and run on.
        /// </summary>
        public RunTargetBase[] RunTargets { get; } = Array.Empty<RunTargetBase>();

        /// <summary>
        /// Get a run result representing a success.
        /// </summary>
        /// <param name="instance">The run process instance.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Success(IRunInstance instance = null) => RunResult.Success(BuildPipeline, BuildConfiguration, instance);

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Failure(string reason) => RunResult.Failure(BuildPipeline, BuildConfiguration, reason);

        /// <summary>
        /// Get a run result representing a failure.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new run result instance.</returns>
        public RunResult Failure(Exception exception) => RunResult.Failure(BuildPipeline, BuildConfiguration, exception);

        /// <summary>
        /// Get the build result of the last <see cref="BuildConfiguration.Build"/> performed.
        /// </summary>
        /// <returns>The build result if found, <see langword="null"/> otherwise.</returns>
        public BuildResult GetBuildResult() => BuildConfiguration.GetBuildResult();

        internal RunContext(BuildPipelineBase pipeline, BuildConfiguration config, params RunTargetBase[] runTargets)
            : base(pipeline, config)
        {
            RunTargets = runTargets ?? Array.Empty<RunTargetBase>();
        }
    }
}
