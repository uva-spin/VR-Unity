using System;

namespace Unity.Build
{
    /// <summary>
    /// Describes a platform which doesn't have its pipeline implemented.
    /// </summary>
    public sealed class MissingPlatform : Platform, ICloneable
    {
        public MissingPlatform(string name) : base(name) { }
        internal MissingPlatform(PlatformInfo info) : base(info) { }
        public object Clone() => new MissingPlatform(new PlatformInfo(Name, DisplayName, PackageName, IconName));
    }
}
