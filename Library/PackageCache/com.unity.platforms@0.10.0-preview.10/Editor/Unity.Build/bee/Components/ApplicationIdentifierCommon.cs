using Unity.Properties;
using Unity.Serialization;

namespace Unity.Build.Common
{
    public sealed partial class ApplicationIdentifier
    {
        string m_PackageName;

        [FormerName("BundleName")]
        [CreateProperty]
        public string PackageName
        {
            get => !string.IsNullOrEmpty(m_PackageName) ? m_PackageName : "com.unity.DefaultPackage";
            set => m_PackageName = value;
        }

    }
}
