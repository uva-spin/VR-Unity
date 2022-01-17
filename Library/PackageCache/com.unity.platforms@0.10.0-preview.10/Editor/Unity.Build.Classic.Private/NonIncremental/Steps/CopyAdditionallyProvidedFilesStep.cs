using System.IO;

namespace Unity.Build.Classic.Private
{
    sealed class CopyAdditionallyProvidedFilesStep : BuildStepBase
    {
        public override BuildResult Run(BuildContext context)
        {
            var classicSharedData = context.GetValue<ClassicSharedData>();

            foreach (var customizer in classicSharedData.Customizers)
                customizer.OnBeforeRegisterAdditionalFilesToDeploy();

            foreach (var customizer in classicSharedData.Customizers)
            {
                customizer.RegisterAdditionalFilesToDeploy((from, to) =>
                {
                    var parent = Path.GetDirectoryName(to);
                    Directory.CreateDirectory(parent);
                    File.Copy(from, to, true);
                });
            }
            return context.Success();
        }
    }
}
