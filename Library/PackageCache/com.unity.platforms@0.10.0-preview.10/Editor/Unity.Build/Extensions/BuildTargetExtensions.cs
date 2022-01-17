using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Unity.Build
{
    /// <summary>
    /// Extensions of <see cref="BuildTarget"/> enum type.
    /// </summary>
    public static class BuildTargetExtensions
    {
        static readonly (BuildTarget BuildTarget, string PlatformName)[] s_BuildTargetPlatformNameMap = new[]
        {
            (BuildTarget.StandaloneWindows64, KnownPlatforms.Windows.Name),
            (BuildTarget.StandaloneOSX, KnownPlatforms.macOS.Name),
            (BuildTarget.StandaloneLinux64, KnownPlatforms.Linux.Name),
            (BuildTarget.iOS, KnownPlatforms.iOS.Name),
            (BuildTarget.Android, KnownPlatforms.Android.Name),
            (BuildTarget.WebGL, KnownPlatforms.WebGL.Name),
            (BuildTarget.WSAPlayer, KnownPlatforms.UniversalWindowsPlatform.Name),
            (BuildTarget.PS4, KnownPlatforms.PlayStation4.Name),
            (BuildTarget.XboxOne, KnownPlatforms.XboxOne.Name),
            (BuildTarget.tvOS, KnownPlatforms.tvOS.Name),
            (BuildTarget.Switch, KnownPlatforms.Switch.Name),
            (BuildTarget.Stadia, KnownPlatforms.Stadia.Name),
            (BuildTarget.Lumin, KnownPlatforms.Lumin.Name),
        };

        static readonly Dictionary<BuildTarget, string> s_BuildTargetToPlatformName =
            s_BuildTargetPlatformNameMap.ToDictionary(x => x.BuildTarget, x => x.PlatformName);
        static readonly Dictionary<string, BuildTarget> s_PlatformNameToBuildTarget =
            s_BuildTargetPlatformNameMap.ToDictionary(x => x.PlatformName, x => x.BuildTarget);

        /// <summary>
        /// Retrieve the corresponding <see cref="Platform"/> of this <see cref="BuildTarget"/>.
        /// </summary>
        /// <param name="buildTarget">The build target.</param>
        /// <returns>The corresponding <see cref="Platform"/> if found, <see langword="null"/> otherwise.</returns>
        public static Platform GetPlatform(this BuildTarget buildTarget)
        {
            return Platform.GetPlatformByName(buildTarget.GetPlatformName());
        }

        /// <summary>
        /// Retrieve the corresponding <see cref="BuildTarget"/> of this <see cref="Platform"/>.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns>The corresponding <see cref="BuildTarget"/> if found, throws otherwise.</returns>
        public static BuildTarget GetBuildTarget(this Platform platform)
        {
            if (platform == null)
                throw new ArgumentNullException(nameof(platform));

            BuildTarget buildTarget;
            if (s_PlatformNameToBuildTarget.TryGetValue(platform.Name, out buildTarget))
                return buildTarget;

            if (Enum.TryParse(platform.Name, out buildTarget))
                return buildTarget;

            throw new NotImplementedException($"Could not map platform {platform.Name} to a {typeof(BuildTarget).FullName} value.");
        }

        internal static string GetPlatformName(this BuildTarget buildTarget)
        {
            if (s_BuildTargetToPlatformName.TryGetValue(buildTarget, out var platformName))
                return platformName;

            // If the platform was added to default list, this switch should reflect that as well.
            if (KnownPlatforms.IsKnownPlatform(buildTarget.ToString()))
                throw new NotImplementedException($"{nameof(BuildTarget)} {buildTarget} is a known platform, please provide mapping to platform name.");

            return buildTarget.ToString();
        }
    }
}
