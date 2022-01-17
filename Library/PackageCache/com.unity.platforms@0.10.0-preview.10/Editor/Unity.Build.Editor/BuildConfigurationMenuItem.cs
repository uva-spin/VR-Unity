using UnityEditor;

#if UNITY_INTERNAL
using System.IO;
using System.Linq;
#endif

namespace Unity.Build.Editor
{
    public static class BuildConfigurationMenuItem
    {
        public const string k_BuildConfigurationMenu = "Assets/Create/Build/";
        const string k_CreateBuildConfigurationAssetEmpty = k_BuildConfigurationMenu + "Empty Build Configuration";

#if UNITY_INTERNAL
        [MenuItem("INTERNAL/Upgrade All Build Assets")]
        static void UpgradeAllBuildAssets()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(BuildConfiguration)}");
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            try
            {
                for (var i = 0; i < paths.Length; ++i)
                {
                    var assetPath = paths[i];
                    EditorUtility.DisplayProgressBar($"Upgrading Asset ({i + 1} of {paths.Length})", Path.GetFileName(assetPath), (float)i / paths.Length);
                    var asset = AssetDatabase.LoadAssetAtPath<BuildConfiguration>(assetPath);
                    asset.SerializeToPath(assetPath);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }
#endif

        [MenuItem(k_CreateBuildConfigurationAssetEmpty)]
        static void CreateBuildConfigurationAsset()
        {
            var newAsset = CreateAssetInActiveDirectory("Empty");
            if (newAsset != null && newAsset)
                ProjectWindowUtil.ShowCreatedAsset(newAsset);
        }

        public static BuildConfiguration CreateAssetInActiveDirectory(string prefix, params IBuildComponent[] components)
        {
            var dependency = Selection.activeObject as BuildConfiguration;
            return BuildConfiguration.CreateAssetInActiveDirectory(prefix + $"{nameof(BuildConfiguration)}{BuildConfiguration.AssetExtension}", (config) =>
            {
                if (dependency != null && dependency)
                {
                    config.AddDependency(dependency);
                }

                foreach (var component in components)
                {
                    config.SetComponent(component);
                }
            });
        }
    }
}
