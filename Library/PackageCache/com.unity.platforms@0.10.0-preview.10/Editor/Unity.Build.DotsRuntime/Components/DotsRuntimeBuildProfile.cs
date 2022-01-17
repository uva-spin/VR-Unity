using System;
using Unity.Properties;
using Unity.Serialization;
using UnityEngine;

namespace Unity.Build.DotsRuntime
{
    [FormerName("Unity.Entities.Runtime.Build.DotsRuntimeBuildProfile, Unity.Entities.Runtime.Build")]
    public sealed class DotsRuntimeBuildProfile : IBuildPipelineComponent
    {
        BuildTarget m_Target;
        DotsRuntimeBuildPipelineBase m_Pipeline;
        DotsRuntimeBuildPipelineConstructor m_PipelineConstructor;

        void ConstructPipeline()
        {
            if (m_Target == null)
            {
                m_Pipeline = null;
                return;
            }

            if (PipelineConstructor != null)
            {
                m_Pipeline = PipelineConstructor.GetPipeline(m_Target);
            }
            else if (TypeConstructionUtility.TryConstructTypeDerivedFrom<DotsRuntimeBuildPipelineSelectorBase>(out var selector))
            {
                m_Pipeline = selector.SelectFor(m_Pipeline, m_Target, m_UseNewPipeline);
            }
        }

        /// <summary>
        /// Gets or sets which <see cref="Platforms.BuildTarget"/> this profile is going to use for the build.
        /// Used for building Dots Runtime players.
        /// </summary>
        [CreateProperty]
        public BuildTarget Target
        {
            get => m_Target;
            set
            {
                m_Target = value;
                ConstructPipeline();
            }
        }

        public int SortingIndex => 0;
        public bool SetupEnvironment() => false;

        /// <summary>
        /// Gets or sets which <see cref="Configuration"/> this profile is going to use for the build.
        /// </summary>
        [CreateProperty]
        public BuildType Configuration { get; set; } = BuildType.Develop;

        bool m_UseNewPipeline = false;

        /// <summary>
        /// @TODO: remove once we are done merging the new pipeline
        /// Determines which pipeline to use. New pipeline is using the hybrid/dotsruntime unification code path
        /// </summary>
        [HideInInspector] // Hide this property until we deprecate the old pipeline
        [CreateProperty]
        public bool UseNewPipeline
        {
            get => m_UseNewPipeline;
            set
            {
                m_UseNewPipeline = value;
                ConstructPipeline();
            }
        }

        public BuildPipelineBase Pipeline
        {
            get
            {
                ConstructPipeline();
                return m_Pipeline;
            }
            set => throw new InvalidOperationException($"Cannot explicitly set {nameof(Pipeline)}, set {nameof(Target)} property instead.");
        }

        [HideInInspector]
        [CreateProperty]
        public DotsRuntimeBuildPipelineConstructor PipelineConstructor
        {
            get => m_PipelineConstructor;
            set
            {
                m_PipelineConstructor = value;
                ConstructPipeline();
            }
        }

        public DotsRuntimeBuildProfile()
        {
            Target = BuildTarget.DefaultBuildTarget;
        }
    }

    /// <summary>
    /// For internal use only. Creates the right pipeline based on useNewPipeline
    /// </summary>
    internal abstract class DotsRuntimeBuildPipelineSelectorBase
    {
        public abstract DotsRuntimeBuildPipelineBase SelectFor(DotsRuntimeBuildPipelineBase basePipeline, BuildTarget target, bool useNewPipeline);
    }
}
