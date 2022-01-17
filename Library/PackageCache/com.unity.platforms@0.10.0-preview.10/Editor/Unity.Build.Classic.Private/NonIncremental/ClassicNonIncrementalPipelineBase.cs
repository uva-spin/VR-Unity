using System.IO;

namespace Unity.Build.Classic.Private
{
    public abstract class ClassicNonIncrementalPipelineBase : ClassicPipelineBase
    {
        protected override BuildResult OnBuild(BuildContext context)
        {
            PrepareContext(context);
            return BuildSteps.Run(context);
        }

        protected override void PrepareContext(BuildContext context)
        {
            base.PrepareContext(context);

            var nonIncrementalClassicData = context.GetOrCreateValue<NonIncrementalClassicSharedData>();
            // Note: Don't use absolute paths, since it's easy to hit path-too-long issue
            //       Also don't delete Temp directory, Unity deletes this directory on quit, also builds might contain cached data, thus making follow up builds faster
            nonIncrementalClassicData.TemporaryDirectory = Path.Combine("Temp", context.BuildConfigurationName);
            Directory.CreateDirectory(nonIncrementalClassicData.TemporaryDirectory);
        }
    }
}
