using UnityEditor;

namespace Unity.Build.Classic.Private
{
    class CustomizerInfoImpl : ClassicBuildPipelineCustomizer.Info
    {
        public CustomizerInfoImpl(BuildContext context)
        {
            Context = context;
        }

        public override BuildContext Context { get; }
        public override string StreamingAssetsDirectory => Context.GetValue<ClassicSharedData>().StreamingAssetsDirectory;
        public override string OutputBuildDirectory => Context.GetValue<ClassicSharedData>().OutputBuildDirectory;
        public override string WorkingDirectory => Context.GetValue<ClassicSharedData>().WorkingDirectory.ToString();
        public override BuildTarget BuildTarget => Context.GetValue<ClassicSharedData>().BuildTarget;
        public override string[] EmbeddedScenes => Context.GetValue<EmbeddedScenesValue>().Scenes;
    }
}
