using System;

namespace Bee.Core
{
    [Obsolete("Please switch to Unity.Build namespace (This class will be removed after 2021-03-01)", true)]
    public abstract class Platform
    {
        public string Name => throw new NotImplementedException();
        public string DisplayName => throw new NotImplementedException();
    }

    [Obsolete("Please include com.unity.platform.windows package and switch to Unity.Build.Windows namespace (This class will be removed after 2021-03-01)", true)]
    public class WindowsPlatform : Platform
    { }

    [Obsolete("Please include com.unity.platform.macos package and switch to Unity.Build.macOS namespace (This class will be removed after 2021-03-01)", true)]
    public class MacOSXPlatform : Platform
    { }

    [Obsolete("Please include com.unity.platform.ios package and switch to Unity.Build.iOS namespace (This class will be removed after 2021-03-01)", true)]
    public class IosPlatform : Platform
    { }

    [Obsolete("Please include com.unity.platform.android package and switch to Unity.Build.Android namespace (This class will be removed after 2021-03-01)", true)]
    public class AndroidPlatform : Platform
    { }
    
    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameLinux) (This class will be removed after 2021-03-01)", true)]
    public class LinuxPlatform : Platform
    { }


    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNametvOS) (This class will be removed after 2021-03-01)", true)]
    public class TvosPlatform : Platform
    { }


    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameWSA) (This class will be removed after 2021-03-01)", true)]
    public class UniversalWindowsPlatform : Platform
    { }

    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameXboxOne) (This class will be removed after 2021-03-01)", true)]
    public class XboxOnePlatform : Platform
    { }

    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameWebGL) (This class will be removed after 2021-03-01)", true)]
    public class WebGLPlatform : Platform
    { }

    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameSwitch) (This class will be removed after 2021-03-01)", true)]
    public class SwitchPlatform : Platform
    { }

    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNamePS4) (This class will be removed after 2021-03-01)", true)]
    public class PS4Platform : Platform
    { }

    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameStadia) (This class will be removed after 2021-03-01)", true)]
    public class StadiaPlatform : Platform
    { }


    [Obsolete("Please switch to Unity.Build namespace and use MissingPlatform(KnownPlatforms.kPlatformNameLumin) (This class will be removed after 2021-03-01)", true)]
    public class LuminPlatform : Platform
    { }
}

