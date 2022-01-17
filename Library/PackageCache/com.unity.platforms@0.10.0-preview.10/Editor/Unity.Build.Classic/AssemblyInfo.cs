using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("Unity.Build.Classic.Private")]

// Per platform access
[assembly: InternalsVisibleTo("Unity.Build.Android.Classic")]
// TODO: com.unity.platforms.ios\Editor\Unity.Build.iOS.Classic\Unity.Build.iOS.Classic.asmdef set assembly name to Unity.Platforms.iOS.Build, since
//       it's accessing internals of build\iOSSupport\UnityEditor.iOS.Extensions.Xcode.dll and that has InternalsVisibleTo("Unity.Platforms.iOS.Build")
[assembly: InternalsVisibleTo("Unity.Platforms.iOS.Build")]
[assembly: InternalsVisibleTo("Unity.Build.iOS.Classic")]
[assembly: InternalsVisibleTo("Unity.Build.macOS.Classic")]
[assembly: InternalsVisibleTo("Unity.Build.Windows.Classic")]

// For GetBuildTarget extension
[assembly: InternalsVisibleTo("Unity.Build.Android")]
[assembly: InternalsVisibleTo("Unity.Build.iOS")]

// For ClassicBuildProfile.Platform property
[assembly: InternalsVisibleTo("Unity.Entities.Editor")]
[assembly: InternalsVisibleTo("Unity.Entities.Editor.Tests")]
[assembly: InternalsVisibleTo("Unity.Scenes.Editor")]

[assembly: InternalsVisibleTo("Unity.Build.Classic.Tests")]
[assembly: InternalsVisibleTo("Build.Tests.Common")]
