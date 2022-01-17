namespace Unity.Build
{
    /// <summary>
    /// Provides information about platform.
    /// </summary>
    public sealed class PlatformInfo
    {
        /// <summary>
        /// Provide a name for platform, used for serialization
        /// Note: Changing the name, will break serialization
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Text which is used to display a platform in UI?
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Provides a package name where platform's pipeline is implemented
        /// This value can be null, that missing platform doesn't have a pipeline implemented
        /// In case of Classic pipeline, MissingPipeline will be used
        /// </summary>
        public string PackageName { get; }

        /// <summary>
        /// Platform icon name
        /// </summary>
        public string IconName { get; }

        /// <summary>
        /// Construct a new <see cref="PlatformInfo"/> instance.
        /// </summary>
        /// <param name="name">The <see cref="Platform"/> short name, used by serialization.</param>
        /// <param name="displayName">The <see cref="Platform"/> display name, used by user interface.</param>
        /// <param name="packageName">The package name that contains the <see cref="Platform"/> type.</param>
        /// <param name="iconName">The <see cref="Platform"/> icon name, used by user interface.</param>
        internal PlatformInfo(string name, string displayName, string packageName, string iconName)
        {
            Name = name;
            DisplayName = displayName;
            PackageName = packageName;
            IconName = iconName;
        }
    }
}
