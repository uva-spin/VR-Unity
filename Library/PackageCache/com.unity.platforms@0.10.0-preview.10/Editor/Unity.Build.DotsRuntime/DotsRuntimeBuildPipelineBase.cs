using System;
using System.Linq;
using Unity.Serialization;

namespace Unity.Build.DotsRuntime
{
    [FormerName("Unity.Entities.Runtime.Build.DotsRuntimeBuildPipelineBase, Unity.Entities.Runtime.Build")]
    public abstract class DotsRuntimeBuildPipelineBase : BuildPipelineBase
    {
        public override Type[] UsedComponents => base.UsedComponents.Concat(Target.UsedComponents).Distinct().ToArray();
        public BuildTarget Target { get; set; }
    }
}
