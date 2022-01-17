using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Unity.Build
{
    /// <summary>
    /// Holds information about exported assets throughout a build pipeline execution.
    /// All exported assets listed in the build manifest will also be installed in the build data directory.
    /// </summary>
    public sealed class BuildManifest
    {
        readonly Dictionary<Guid, string> m_Assets = new Dictionary<Guid, string>();
        readonly HashSet<FileInfo> m_ExportedFiles = new HashSet<FileInfo>();

        /// <summary>
        /// A dictionary of all assets exported during the build pipeline execution.
        /// </summary>
        public IReadOnlyDictionary<Guid, string> Assets => m_Assets;

        /// <summary>
        /// The list of exported files during the build pipeline execution.
        /// </summary>
        public IEnumerable<FileInfo> ExportedFiles => m_ExportedFiles;

        /// <summary>
        /// Add an asset and its exported files to the build manifest.
        /// </summary>
        /// <param name="assetGuid">The asset <see cref="Guid"/>.</param>
        /// <param name="assetPath">The asset path.</param>
        /// <param name="exportedFiles">The files that were exported by the asset exporter for this asset.</param>
        public void Add(Guid assetGuid, string assetPath, IEnumerable<FileInfo> exportedFiles)
        {
            if (exportedFiles == null || exportedFiles.Count() == 0)
                return;

            if (!m_Assets.ContainsKey(assetGuid))
            {
                m_Assets.Add(assetGuid, assetPath);
            }

            foreach (var file in exportedFiles)
                m_ExportedFiles.Add(file);
        }

        /// <summary>
        /// Add files to be deployed to the build manifest which are not backed by an asset.
        /// </summary>
        /// <param name="exportedFiles">The files to be exported with the build.</param>
        public void AddAdditionalFilesToDeploy(IEnumerable<FileInfo> exportedFiles)
        {
            if (exportedFiles == null || exportedFiles.Count() == 0)
                return;

            foreach (var file in exportedFiles)
                m_ExportedFiles.Add(file);
        }

        /// <summary>
        /// Add a file to be deployed to the build manifest which are not backed by an asset.
        /// </summary>
        /// <param name="exportedFile">The file to be exported with the build.</param>
        public void AddAdditionalFilesToDeploy(FileInfo exportedFile)
        {
            if (exportedFile == null)
                return;

            m_ExportedFiles.Add(exportedFile);
        }
    }
}
