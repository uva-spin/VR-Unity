using System;

namespace Unity.Build
{
    [Obsolete("ICustomBuildComponentConstructor has been replaced by IBuildComponentInitialize. (RemovedAfter 2021-01-01)", true)]
    internal interface ICustomBuildComponentConstructor
    {
        void Construct(BuildConfiguration.ReadOnly config);
    }
}
