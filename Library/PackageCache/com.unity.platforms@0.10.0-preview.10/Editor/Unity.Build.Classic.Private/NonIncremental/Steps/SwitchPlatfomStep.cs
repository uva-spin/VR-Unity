using UnityEditor;

namespace Unity.Build.Classic.Private
{
    sealed class SwitchPlatfomStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var target = context.GetValue<ClassicSharedData>().BuildTarget;
            if (target == BuildTarget.NoTarget)
            {
                return context.Failure($"Invalid build target '{target.ToString()}'.");
            }

            if (EditorUserBuildSettings.activeBuildTarget == target)
            {
                return context.Success();
            }

            var group = UnityEditor.BuildPipeline.GetBuildTargetGroup(target);
            if (EditorUserBuildSettings.SwitchActiveBuildTargetAsync(group, target))
            {
                return context.Failure($"Editor's active Build Target needed to be switched to {target} (BuildTargetGroup {group}). Please wait for switch to complete and then build again.");
            }
            else
            {
                return context.Failure($"Editor's active Build Target could not be switched to {target} (BuildTargetGroup {group}). Look in the console or the editor log for additional errors.");
            }
        }
    }
}
