using Bee.NativeProgramSupport;

namespace DotsBuildTargets
{
    abstract class DotsBuildSystemTarget
    {
        protected virtual bool CanRunMultiThreadedJobs => false; // Disabling by default; Eventually: ScriptingBackend == ScriptingBackend.Dotnet;
        /*
         * disabled by default because it takes work to enable each platform for burst
         */
        public virtual bool CanUseBurst => false;

        public abstract string Identifier { get; }
        public abstract ToolChain ToolChain { get; }

        public virtual ScriptingBackend ScriptingBackend { get; set; } = ScriptingBackend.TinyIl2cpp;
        public virtual TargetFramework TargetFramework => TargetFramework.Tiny;

        protected virtual NativeProgramFormat GetExecutableFormatForConfig(DotsConfiguration config, bool enableManagedDebugger) => null;

        public virtual DotsRuntimeCSharpProgramConfiguration CustomizeConfigForSettings(DotsRuntimeCSharpProgramConfiguration config, FriendlyJObject settings) => config;

        // required if more than one set of binaries need to be generated (for example Android armv7 + arm64)
        // see comment in https://github.com/Unity-Technologies/dots/blob/master/TinySamples/Packages/com.unity.dots.runtime/bee%7E/BuildProgramSources/DotsConfigs.cs
        // DotsConfigs.MakeConfigs() method for details.
        public virtual DotsBuildSystemTarget ComplementaryTarget => null;

        public virtual bool ValidateManagedDebugging(ref bool mdb) { return true; }
    }
}
