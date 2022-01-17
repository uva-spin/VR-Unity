using UnityEditor;

namespace Unity.Build.Classic.Private
{
    static class BuildContextExtensions
    {
        public static bool UsesIL2CPP(this BuildContext context) => context.TryGetComponent(out ClassicScriptingSettings css) && css.ScriptingBackend == ScriptingImplementation.IL2CPP;
        public static bool IsDevelopmentBuild(this BuildContext context)
        {
            if (context.TryGetComponent(out ClassicBuildProfile classicBuildProfile))
                return classicBuildProfile.Configuration == BuildType.Develop || classicBuildProfile.Configuration == BuildType.Debug;
            return false;
        }
    }
}
