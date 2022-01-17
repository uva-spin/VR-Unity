using System.Collections.Generic;
using UnityEngine;

namespace Unity.Build.Editor
{
    static class Resources
    {
        // UI Templates
        public static UITemplate BuildConfiguration = new UITemplate("build-configuration");
        public static UITemplate BuildConfigurationDependency = new UITemplate("build-configuration-dependency");
        public static UITemplate BuildComponent = new UITemplate("build-component");
        public static UITemplate ClassicBuildProfile = new UITemplate("classic-build-profile");
        public static UITemplate TypeInspector = new UITemplate("type-inspector");

        // UI Icons
        public static Texture2D BuildComponentIcon = UIIcon.LoadPackageIcon("Component");

        private static Dictionary<string, Texture2D> m_PlatformIcons = new Dictionary<string, Texture2D>();
        public static Texture2D GetPlatformIcon(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            if (m_PlatformIcons.TryGetValue(name, out var value))
                return value;
            value = UIIcon.LoadIcon("Icons", "BuildSettings." + name);
            m_PlatformIcons[name] = value;
            return value;
        }
    }
}
