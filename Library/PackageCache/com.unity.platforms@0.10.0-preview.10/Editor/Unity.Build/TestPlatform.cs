using UnityEngine;

namespace Unity.Build.Tests
{
    /// <summary>
    /// Mock test platform.
    /// </summary>
    [HideInInspector]
    public sealed class TestPlatform : Platform
    {
        public TestPlatform() : base(new PlatformInfo("test", "Test Platform", "com.unity.platforms", null)) { }
    }
}
