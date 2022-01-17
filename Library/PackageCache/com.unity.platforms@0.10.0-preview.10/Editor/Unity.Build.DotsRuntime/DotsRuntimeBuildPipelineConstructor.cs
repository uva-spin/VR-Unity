using Unity.Serialization;
using BuildTarget = Unity.Build.DotsRuntime.BuildTarget;

namespace Unity.Build.DotsRuntime
{
    [FormerName("DotsRuntimeBuildPipelineConstructor, Unity.Entities.Runtime.Build")]
    public abstract class DotsRuntimeBuildPipelineConstructor
    {
        private BuildTarget m_Target;

        private DotsRuntimeBuildPipelineBase m_BuildPipeline;

        public DotsRuntimeBuildPipelineBase GetPipeline(BuildTarget target)
        {
            if (m_BuildPipeline == null || m_Target != target)
            {
                m_Target = target;
                m_BuildPipeline = Construct(target);
            }

            return m_BuildPipeline;
        }

        protected abstract DotsRuntimeBuildPipelineBase Construct(BuildTarget target);
    }
}
