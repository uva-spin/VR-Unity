using Unity.Serialization;

namespace Unity.Build.Common
{
    /// <summary>
    /// Overrides the default output directory of Builds/BuildConfiguration.name to an arbitrary location.
    /// </summary>
    [FormerName("Unity.Build.Common.OutputBuildDirectory, Unity.Build.Common")]
    public class OutputBuildDirectory : IBuildComponent
    {
        public string OutputDirectory;
    }
}
