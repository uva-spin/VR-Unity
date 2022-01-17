using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditorInternal;
using CompilationPipeline = UnityEditor.Compilation.CompilationPipeline;

namespace Unity.Build
{
    /// <summary>
    /// A <see cref="BuildAssemblyCache"/> caches a set of assemblies built from a root assembly asset and a build target.
    /// It is rebuilt if the root assembly asset or the build target has changed.
    /// </summary>
    public sealed class BuildAssemblyCache
    {
        HashSet<Assembly> m_AssembliesCache = new HashSet<Assembly>();
        List<string> m_AssembliesPath = new List<String>();
        bool m_IsDirty = true;

        /// <summary>
        /// The list of assemblies that are being referenced from this cache
        /// </summary>
        public List<Assembly> Assemblies
        {
            get
            {
                RebuildCacheIfDirty();
                return m_AssembliesCache.ToList();
            }
        }

        public List<string> AssembliesPath
        {
            get
            {
                RebuildCacheIfDirty();
                return m_AssembliesPath;
            }
        }

        string m_PlatformName = null;
        /// <summary>
        /// The platform name to use to include the assemblies including this platform
        /// Dirty the cache if the name has changed
        /// </summary>
        public string PlatformName
        {
            get => m_PlatformName;
            set
            {
                if (m_PlatformName != value)
                {
                    m_PlatformName = value;
                    m_IsDirty = true;
                }
            }
        }

        AssemblyDefinitionAsset m_BaseAssemblyAsset = null;
        /// <summary>
        /// The assembly definition asset to use as the root assemblies for this cache.
        /// Dirty the cache if the asset has changed
        /// </summary>
        public AssemblyDefinitionAsset BaseAssemblies
        {
            get { return m_BaseAssemblyAsset; }
            set
            {
                if (m_BaseAssemblyAsset == null || m_BaseAssemblyAsset.name != value.name)
                {
                    m_BaseAssemblyAsset = value;
                    m_IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Return true if the type T is present in the assemblies of the cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool HasType<T>()
        {
            return HasType(typeof(T));
        }

        /// <summary>
        /// Returns true if the Type type is present in the assemblies of the cache
        /// and rebuilds the cache if needed
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool HasType(Type type)
        {
            RebuildCacheIfDirty();
            return m_AssembliesCache.Contains(type.Assembly);
        }

        /// <summary>
        /// Rebuilds the cache of assemblies only if the platform name or the root assembly asset has changed
        /// </summary>
        public void RebuildCacheIfDirty()
        {
            if (!m_IsDirty)
                return;

            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(m_BaseAssemblyAsset);
            if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(m_PlatformName))
                return;

            m_AssembliesCache.Clear();
            m_AssembliesPath.Clear();

            var assemblyNames = new HashSet<string>();
            var assemblyDefinitions = new Stack<AssemblyDefinitionDescription>();
            var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            //Add base assembly
            var assemblyDefinition = AssemblyDefinitionDescriptionUtility.Deserialize(assetPath);
            assemblyNames.Add(assemblyDefinition.name);
            m_AssembliesPath.Add(assetPath);
            assemblyDefinitions.Push(assemblyDefinition);

            // Walk the assembly tree from main assemblies
            var result = new HashSet<AssemblyDefinitionDescription>();
            while (assemblyDefinitions.Count > 0)
            {
                assemblyDefinition = assemblyDefinitions.Pop();

                //Add Assembly to the cache
                var asm = domainAssemblies.FirstOrDefault(s => s.GetName().Name == assemblyDefinition.name);
                if (asm != null && !m_AssembliesCache.Add(asm))
                    continue;

                if (assemblyDefinition.references == null)
                    continue;

                foreach (var assemblyReference in assemblyDefinition.references)
                {
                    if (assemblyNames.Contains(assemblyReference))
                        continue;

                    var assemblyDefinitionPath = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyReference(assemblyReference);
                    if (string.IsNullOrEmpty(assemblyDefinitionPath))
                        continue;

                    var referencedAssemblyDefinition = AssemblyDefinitionDescriptionUtility.Deserialize(assemblyDefinitionPath);
                    if (assemblyNames.Add(referencedAssemblyDefinition.name) && AssemblyDefinitionDescriptionUtility.MatchPlatform(referencedAssemblyDefinition, m_PlatformName))
                    {
                        assemblyDefinitions.Push(referencedAssemblyDefinition);
                        m_AssembliesPath.Add(assemblyDefinitionPath);
                    }
                }
            }
            m_IsDirty = false;
        }

        internal class AssemblyDefinitionDescription
        {
            public string name = null;
            public string[] references = null;
            public string[] includePlatforms = null;
            public string[] excludePlatforms = null;
            public string[] optionalUnityReferences = null;
        }

        internal static class AssemblyDefinitionDescriptionUtility
        {
            internal static AssemblyDefinitionDescription Deserialize(string assemblyDefinitionPath)
            {
                var assemblyDefinition = new AssemblyDefinitionDescription(); // autoReferenced is true by default even when it's not defined
                UnityEngine.JsonUtility.FromJsonOverwrite(File.ReadAllText(assemblyDefinitionPath), assemblyDefinition);

                if (assemblyDefinition == null)
                {
                    throw new Exception($"File '{assemblyDefinitionPath}' does not contain valid asmdef data.");
                }

                if (string.IsNullOrEmpty(assemblyDefinition.name))
                {
                    throw new Exception($"Required property name not set after deserializing");
                }

                if (assemblyDefinition.excludePlatforms?.Length > 0 && assemblyDefinition.includePlatforms?.Length > 0)
                {
                    throw new Exception($"Both excludePlatforms and includePlatforms are set.");
                }

                return assemblyDefinition;
            }

            internal static bool MatchPlatform(AssemblyDefinitionDescription asmdef, string platformName)
            {
                // Ignore test assemblies
                if (asmdef.optionalUnityReferences != null && asmdef.optionalUnityReferences.Any(r => r == "TestAssemblies"))
                {
                    return false;
                }

                var emptyIncludes = asmdef.includePlatforms == null || asmdef.includePlatforms.Length == 0;
                var emptyExcludes = asmdef.excludePlatforms == null || asmdef.excludePlatforms.Length == 0;

                // If no included or excluded platforms, means its available for any platforms
                if (emptyIncludes && emptyExcludes)
                    return true;

                // Is there a valid platform name?
                if (string.IsNullOrEmpty(platformName))
                    return false;

                // Not listed in included platforms
                if (!emptyIncludes && !asmdef.includePlatforms.Contains(platformName))
                    return false;

                // Not listed in excluded platforms
                return emptyExcludes || !asmdef.excludePlatforms.Contains(platformName);
            }
        }
    }
}
