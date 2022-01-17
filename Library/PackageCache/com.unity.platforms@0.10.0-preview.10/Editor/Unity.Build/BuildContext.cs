using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;

namespace Unity.Build
{
    /// <summary>
    /// Holds contextual information when building a build pipeline.
    /// </summary>
    public sealed class BuildContext : ContextBase
    {
        readonly List<IBuildArtifact> m_Artifacts = new List<IBuildArtifact>();

        /// <summary>
        /// The build progress object used througout the build.
        /// </summary>
        public BuildProgress BuildProgress { get; }

        /// <summary>
        /// Quick access to build manifest value.
        /// </summary>
        public BuildManifest BuildManifest => GetOrCreateValue<BuildManifest>();

        /// <summary>
        /// Get a build result representing a success.
        /// </summary>
        /// <returns>A new build result instance.</returns>
        public BuildResult Success() => BuildResult.Success(BuildPipeline, BuildConfiguration);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="reason">The reason of the failure.</param>
        /// <returns>A new build result instance.</returns>
        public BuildResult Failure(string reason) => BuildResult.Failure(BuildPipeline, BuildConfiguration, reason);

        /// <summary>
        /// Get a build result representing a failure.
        /// </summary>
        /// <param name="exception">The exception that was thrown.</param>
        /// <returns>A new build result instance.</returns>
        public BuildResult Failure(Exception exception) => BuildResult.Failure(BuildPipeline, BuildConfiguration, exception);

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public override bool HasBuildArtifact(Type buildArtifactType)
        {
            BuildArtifacts.ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            return m_Artifacts.Any(a => buildArtifactType.IsAssignableFrom(a.GetType()));
        }

        /// <summary>
        /// Determine if a build artifact that is assignable to the specified type is present.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns><see langword="true"/> if a matching build artifact is found, <see langword="false"/> otherwise.</returns>
        public override bool HasBuildArtifact<T>() => m_Artifacts.Any(a => typeof(T).IsAssignableFrom(a.GetType()));

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public override IBuildArtifact GetBuildArtifact(Type buildArtifactType)
        {
            BuildArtifacts.ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            return m_Artifacts.FirstOrDefault(a => buildArtifactType.IsAssignableFrom(a.GetType()));
        }

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns>A build artifact value if found, <see langword="null"/> otherwise.</returns>
        public override T GetBuildArtifact<T>() => (T)m_Artifacts.FirstOrDefault(a => typeof(T).IsAssignableFrom(a.GetType()));

        /// <summary>
        /// Get all build artifact values.
        /// </summary>
        /// <returns>Enumeration of all build artifact values.</returns>
        public override IEnumerable<IBuildArtifact> GetAllBuildArtifacts() => m_Artifacts;

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type, or create and set it if not found.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns>The build artifact value.</returns>
        public IBuildArtifact GetOrCreateBuildArtifact(Type buildArtifactType)
        {
            BuildArtifacts.ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            var artifact = GetBuildArtifact(buildArtifactType);
            if (artifact == null)
            {
                artifact = TypeConstruction.Construct<IBuildArtifact>(buildArtifactType);
                SetBuildArtifact(artifact);
            }
            return artifact;
        }

        /// <summary>
        /// Get the first build artifact value that is assignable to specified type, or create and set it if not found.
        /// Multiple build artifact value can be stored per build configuration.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns>The build artifact value.</returns>
        public T GetOrCreateBuildArtifact<T>() where T : class, IBuildArtifact, new()
        {
            var artifact = GetBuildArtifact<T>();
            if (artifact == null)
            {
                artifact = TypeConstruction.Construct<T>();
                SetBuildArtifact(artifact);
            }
            return artifact;
        }

        /// <summary>
        /// Set the value of a build artifact.
        /// </summary>
        /// <param name="artifact"></param>
        public void SetBuildArtifact(IBuildArtifact artifact)
        {
            if (artifact == null)
                throw new ArgumentNullException(nameof(artifact));

            var artifactType = artifact.GetType();
            for (var i = 0; i < m_Artifacts.Count; ++i)
            {
                if (m_Artifacts[i].GetType() == artifactType)
                {
                    m_Artifacts[i] = artifact;
                    return;
                }
            }

            m_Artifacts.Add(artifact);
        }

        /// <summary>
        /// Remove all build artifact that is assignable to specified type.
        /// </summary>
        /// <param name="buildArtifactType">The build artifact type.</param>
        /// <returns><see langword="true"/> if one of more artifact was removed, <see langword="false"/> otherwise.</returns>
        public bool RemoveBuildArtifact(Type buildArtifactType)
        {
            BuildArtifacts.ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            return m_Artifacts.RemoveAll(a => buildArtifactType.IsAssignableFrom(a.GetType())) > 0;
        }

        /// <summary>
        /// Remove all build artifact that is assignable to specified type.
        /// </summary>
        /// <typeparam name="T">The build artifact type.</typeparam>
        /// <returns><see langword="true"/> if one of more artifact was removed, <see langword="false"/> otherwise.</returns>
        public bool RemoveBuildArtifact<T>() where T : class, IBuildArtifact => m_Artifacts.RemoveAll(a => typeof(T).IsAssignableFrom(a.GetType())) > 0;

        /// <summary>
        /// Remove all build artifacts.
        /// </summary>
        public void RemoveAllBuildArtifacts() => m_Artifacts.Clear();

        internal BuildContext(BuildPipelineBase pipeline, BuildConfiguration config, BuildProgress progress = null) :
            base(pipeline, config)
        {
            BuildProgress = progress;
        }
    }
}
