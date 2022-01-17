using UnityEditor;
using UnityEditor.PlatformSupport;

namespace Unity.Build.Bridge
{
    internal class ProvisioningProfileBridge
    {
        ProvisioningProfile internalObject;

        ProvisioningProfileBridge(ProvisioningProfile profile)
        {
            internalObject = profile;
        }

        internal static ProvisioningProfileBridge ParseProvisioningProfileAtPath(string pathToFile)
        {
            var profile = ProvisioningProfile.ParseProvisioningProfileAtPath(pathToFile);
            return new ProvisioningProfileBridge(profile);
        }

        internal static ProvisioningProfileBridge FindLocalProfileByUUID(string UUID, string[] searchPaths = null)
        {
            var profile = ProvisioningProfile.FindLocalProfileByUUID(UUID, searchPaths);
            return profile != null ? new ProvisioningProfileBridge(profile) : null;
        }

        internal string UUID => internalObject.UUID;
        internal ProvisioningProfileType type => internalObject.type;
    }
}
