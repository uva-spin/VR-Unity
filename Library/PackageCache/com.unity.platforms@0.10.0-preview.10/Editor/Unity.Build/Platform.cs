using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// Platform base class.
    /// </summary>
    public abstract partial class Platform
    {
        static readonly Platform[] s_AvailablePlatforms;
        readonly PlatformInfo m_PlatformInfo;

        /// <summary>
        /// All available platforms.
        /// </summary>
        public static IEnumerable<Platform> AvailablePlatforms => s_AvailablePlatforms;

        /// <summary>
        /// Platform name, used for serialization.
        /// </summary>
        public string Name => m_PlatformInfo.Name;

        /// <summary>
        /// Platform display name, used in user interface.
        /// </summary>
        public string DisplayName => m_PlatformInfo.DisplayName;

        /// <summary>
        /// Platform icon name.
        /// </summary>
        public string IconName => m_PlatformInfo.IconName;

        /// <summary>
        /// Platform package name.
        /// </summary>
        public string PackageName => m_PlatformInfo.PackageName;

        /// <summary>
        /// Get platform by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>A <see cref="Platform"/> instance if found, <see langword="null"/> otherwise.</returns>
        public static Platform GetPlatformByName(string name)
        {
            var platform = s_AvailablePlatforms.FirstOrDefault(p => p.Name == name);

            // Check for platform former name.
            // Dot not change these values, they are only used to deserialize old build assets.
            // This list does not need to be updated when adding new platforms.
            if (platform == null)
            {
                if (name == "Windows")
                    name = KnownPlatforms.Windows.Name;
                else if (name == "OSX")
                    name = KnownPlatforms.macOS.Name;
                else if (name == "Linux")
                    name = KnownPlatforms.Linux.Name;
                else if (name == "IOS")
                    name = KnownPlatforms.iOS.Name;
                else if (name == "Android")
                    name = KnownPlatforms.Android.Name;
                else if (name == "WebGL")
                    name = KnownPlatforms.WebGL.Name;
                else if (name == "WSA")
                    name = KnownPlatforms.UniversalWindowsPlatform.Name;
                else if (name == "PS4")
                    name = KnownPlatforms.PlayStation4.Name;
                else if (name == "XboxOne")
                    name = KnownPlatforms.XboxOne.Name;
                else if (name == "tvOS")
                    name = KnownPlatforms.tvOS.Name;
                else if (name == "Switch")
                    name = KnownPlatforms.Switch.Name;
                else if (name == "Stadia")
                    name = KnownPlatforms.Stadia.Name;
                else if (name == "Lumin")
                    name = KnownPlatforms.Lumin.Name;

                platform = s_AvailablePlatforms.FirstOrDefault(p => p.Name == name);
            }
            return platform;
        }

        static Platform()
        {
            var platforms = TypeCache.GetTypesDerivedFrom<Platform>()
                .Where(type => type != typeof(MissingPlatform))
                .Where(type => !type.IsAbstract && !type.IsGenericType)
                .Where(type => !type.HasAttribute<ObsoleteAttribute>())
                .Where(TypeConstruction.CanBeConstructed)
                .Select(TypeConstruction.Construct<Platform>);

            var platformsByName = new Dictionary<string, Platform>();
            foreach (var platform in platforms)
            {
                if (platformsByName.TryGetValue(platform.Name, out var registeredPlatform))
                    throw new InvalidOperationException($"Duplicate platform name found. Platform named '{platform.Name}' is already registered by class '{registeredPlatform.GetType().FullName}'.");

                platformsByName.Add(platform.Name, platform);
            }

            // Fill up missing platforms
            foreach (var buildTarget in Enum.GetValues(typeof(BuildTarget)).Cast<BuildTarget>())
            {
                if (buildTarget == BuildTarget.NoTarget ||
                    buildTarget == BuildTarget.StandaloneWindows)
                    continue;

                if (buildTarget.HasAttribute<HideInInspector>() ||
                    buildTarget.HasAttribute<ObsoleteAttribute>())
                    continue;

                var name = buildTarget.GetPlatformName();
                if (platformsByName.ContainsKey(name))
                    continue;

                platformsByName.Add(name, new MissingPlatform(name));
            }

            s_AvailablePlatforms = platformsByName.Values.ToArray();
        }

        internal Platform(string name)
        {
            var info = KnownPlatforms.GetPlatformInfo(name);
            if (info == null)
                info = new PlatformInfo(name, name, null, null);

            m_PlatformInfo = info;
        }

        internal Platform(PlatformInfo info)
        {
            m_PlatformInfo = info;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Platform);
        }

        public bool Equals(Platform p)
        {
            if (ReferenceEquals(p, null))
                return false;

            if (ReferenceEquals(this, p))
                return true;

            // We don't change type equality on purpose, since for ex.
            // WindowsPlatform and MissingPlatform with internal name "Windows" means same platform
            return Equals(Name, p.Name);
        }

        public static bool operator ==(Platform lhs, Platform rhs)
        {
            if (ReferenceEquals(lhs, null))
            {
                if (ReferenceEquals(rhs, null))
                    return true;

                return false;
            }
            return Equals(lhs, rhs);
        }

        public static bool operator !=(Platform lhs, Platform rhs)
        {
            return !(lhs == rhs);
        }
    }
}
