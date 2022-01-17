using Unity.Properties;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.Build
{
    /// <summary>
    /// A <see cref="RootAssemblyComponent"/> allows to specify an Assembly Definition Asset for a build.
    /// </summary>
    public class RootAssemblyComponent : IBuildComponent
    {
        /// <summary>
        /// Gets or sets the root assembly for a build. This root
        /// assembly determines what other assemblies will be pulled in for the build.
        /// </summary>
        [CreateProperty]
        public LazyLoadReference<AssemblyDefinitionAsset> RootAssembly { get; set; }
    }
}
