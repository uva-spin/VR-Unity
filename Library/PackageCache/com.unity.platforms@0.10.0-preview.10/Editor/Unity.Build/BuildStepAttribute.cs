using System;

namespace Unity.Build
{
    /// <summary>
    /// Attribute to configure various properties of a <see cref="BuildStepBase"/> derived type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BuildStepAttribute : Attribute
    {
        /// <summary>
        /// Display description of the build step.
        /// </summary>
        public string Description { get; set; }
    }
}
