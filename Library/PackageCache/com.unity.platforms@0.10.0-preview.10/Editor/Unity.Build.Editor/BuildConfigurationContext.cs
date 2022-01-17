using Unity.Properties.UI;

namespace Unity.Build.Editor
{
    class BuildConfigurationContext : InspectionContext
    {
        public BuildConfigurationScriptedImporterEditor ImporterEditor { get; }

        public BuildConfigurationContext(BuildConfigurationScriptedImporterEditor importerEditor)
        {
            ImporterEditor = importerEditor;
        }
    }
}
