using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Build
{
    /// <summary>
    /// Contains constants for known platforms.
    /// </summary>
    public static class KnownPlatforms
    {
        /// <summary>
        /// Constant platform information for Windows platform.
        /// </summary>
        public static class Windows
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "win";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Windows";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = "com.unity.platforms.windows";

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Standalone";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for macOS platform.
        /// </summary>
        public static class macOS
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "macos";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "macOS";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = "com.unity.platforms.macos";

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Standalone";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Linux platform.
        /// </summary>
        public static class Linux
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "linux";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Linux";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = "com.unity.platforms.linux";

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Standalone";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for iOS platform.
        /// </summary>
        public static class iOS
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "ios";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "iOS";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "iPhone";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Android platform.
        /// </summary>
        public static class Android
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "android";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Android";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = "com.unity.platforms.android";

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Android";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for WebGL platform.
        /// </summary>
        public static class WebGL
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "webgl";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "WebGL";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "WebGL";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Universal Windows Platform (UWP) platform.
        /// </summary>
        public static class UniversalWindowsPlatform
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "uwp";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Universal Windows Platform";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Metro";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for PlayStation 4 platform.
        /// </summary>
        public static class PlayStation4
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "ps4";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "PlayStation 4";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "PS4";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Xbox One platform.
        /// </summary>
        public static class XboxOne
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "xb1";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Xbox One";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "XboxOne";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for tvOS platform.
        /// </summary>
        public static class tvOS
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "tvos";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "tvOS";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "tvOS";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Switch platform.
        /// </summary>
        public static class Switch
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "switch";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Switch";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Switch";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Stadia platform.
        /// </summary>
        public static class Stadia
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "stadia";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Stadia";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Stadia";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        /// <summary>
        /// Constant platform information for Lumin platform.
        /// </summary>
        public static class Lumin
        {
            /// <summary>
            /// Platform name, used for serialization.
            /// </summary>
            public const string Name = "lumin";

            /// <summary>
            /// Platform display name, used for display.
            /// </summary>
            public const string DisplayName = "Lumin";

            /// <summary>
            /// Platform package name.
            /// </summary>
            public const string PackageName = null;

            /// <summary>
            /// Platform icon name.
            /// </summary>
            public const string IconName = "Lumin";

            internal static PlatformInfo PlatformInfo { get; } = new PlatformInfo(Name, DisplayName, PackageName, IconName);
        }

        // Keep this list updated when adding a new platform
        static readonly PlatformInfo[] s_KnownPlatforms = new[]
        {
            Windows.PlatformInfo,
            macOS.PlatformInfo,
            Linux.PlatformInfo,
            iOS.PlatformInfo,
            Android.PlatformInfo,
            WebGL.PlatformInfo,
            UniversalWindowsPlatform.PlatformInfo,
            PlayStation4.PlatformInfo,
            XboxOne.PlatformInfo,
            tvOS.PlatformInfo,
            Switch.PlatformInfo,
            Stadia.PlatformInfo,
            Lumin.PlatformInfo
        };

        static KnownPlatforms()
        {
            // Verify known platforms have unique names
            var knownPlatformsByName = new Dictionary<string, PlatformInfo>();
            foreach (var platform in s_KnownPlatforms)
            {
                if (knownPlatformsByName.TryGetValue(platform.Name, out var registeredPlatform))
                    throw new InvalidOperationException($"Duplicate platform info found. Platform info with name '{platform.Name}' already registered.");

                knownPlatformsByName.Add(platform.Name, platform);
            }
        }

        internal static PlatformInfo GetPlatformInfo(string name)
        {
            return s_KnownPlatforms.FirstOrDefault(info => info.Name == name);
        }

        internal static bool IsKnownPlatform(string name)
        {
            return s_KnownPlatforms.Any(info => info.Name == name);
        }
    }
}
