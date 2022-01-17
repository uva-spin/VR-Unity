using System;
using System.IO;
using Unity.Properties;
using Unity.Serialization;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.DotsRuntime
{
    [FormerName("Unity.Entities.Runtime.Build.DotsRuntimeRootAssembly, Unity.Entities.Runtime.Build")]
    public sealed class DotsRuntimeRootAssembly : RootAssemblyComponent
    {
        public string ProjectName
        {
            get
            {
#if UNITY_2020_1_OR_NEWER
                var rootAssembly = RootAssembly.asset;
#else
                var rootAssembly = RootAssembly;
#endif
                if (rootAssembly == null || !rootAssembly)
                    return null;

                // FIXME should maybe be RootAssembly.name, but this is super confusing
                var asmdefPath = AssetDatabase.GetAssetPath(rootAssembly);
                var asmdefFilename = Path.GetFileNameWithoutExtension(asmdefPath);

                // just require that they're identical for this root assembly
                if (!asmdefFilename.Equals(rootAssembly.name))
                    throw new InvalidOperationException($"Root asmdef {asmdefPath} must have its assembly name (currently '{rootAssembly.name}') set to the same as the filename (currently '{asmdefFilename}')");

                return asmdefFilename;
            }
        }

        public static DirectoryInfo BeeRootDirectory => new DirectoryInfo("Library/DotsRuntimeBuild");
        public DirectoryInfo StagingDirectory => new DirectoryInfo($"Library/DotsRuntimeBuild/{ProjectName}");

        [CreateProperty, HideInInspector]
        public string BeeTargetOverride { get; set; }

        public string MakeBeeTargetName(string buildConfigurationName)
        {
#if UNITY_2020_1_OR_NEWER
            var rootAssembly = RootAssembly.asset;
#else
            var rootAssembly = RootAssembly;
#endif
            if(rootAssembly == null)
                throw new ArgumentException("No DotsRuntimeRootAssembly component specified. Please make sure your build configuration specifies one.");

            return $"{rootAssembly.name}-{buildConfigurationName}".ToLower();
        }
    }
}
