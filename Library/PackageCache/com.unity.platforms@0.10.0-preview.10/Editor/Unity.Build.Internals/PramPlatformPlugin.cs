using System.Collections.Generic;

namespace Unity.Build.Internals
{
    internal abstract class PramPlatformPlugin
    {
        public abstract string[] Providers { get; }
        public abstract string PlatformAssemblyLoadPath { get; }
        public abstract IReadOnlyDictionary<string, string> Environment { get; }
    }
}
