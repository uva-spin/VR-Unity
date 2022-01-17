using System;
using System.Collections.Generic;
using Unity.Serialization.Json;
using UnityEditor;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace Unity.Build
{
    [ScriptedImporter(3, new[] { BuildConfiguration.AssetExtension })]
    sealed class BuildConfigurationScriptedImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext context)
        {
            var asset = BuildConfiguration.CreateInstance();
            if (BuildConfiguration.DeserializeFromPath(asset, context.assetPath))
            {
                context.AddObjectToAsset("asset", asset/*, icon*/);
                context.SetMainObject(asset);
            }
        }

        static string[] GatherDependenciesFromSourceFile(string assetPath)
        {
            var dependencies = new List<string>();
            try
            {
                using (var reader = new SerializedObjectReader(assetPath))
                {
                    var root = reader.ReadObject();
                    if (!root.TryGetMember(nameof(BuildConfiguration.Dependencies), out var member))
                    {
                        return Array.Empty<string>();
                    }

                    var valueView = member.Value();
                    if (valueView.Type != TokenType.Array)
                    {
                        return Array.Empty<string>();
                    }

                    var arrayView = valueView.AsArrayView();
                    foreach (var value in arrayView)
                    {
                        if (!GlobalObjectId.TryParse(value.AsStringView().ToString(), out var id))
                        {
                            continue;
                        }

                        var dependencyPath = AssetDatabase.GUIDToAssetPath(id.assetGUID.ToString());
                        if (string.IsNullOrEmpty(dependencyPath))
                        {
                            continue;
                        }

                        dependencies.Add(dependencyPath);
                    }
                }
            }
            catch
            {
                return Array.Empty<string>();
            }
            return dependencies.ToArray();
        }
    }
}
