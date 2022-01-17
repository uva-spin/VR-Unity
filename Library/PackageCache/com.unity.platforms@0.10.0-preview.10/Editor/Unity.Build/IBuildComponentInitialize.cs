namespace Unity.Build
{
    /// <summary>
    /// Base interface for build components that wants to provide an initialization method.
    /// </summary>
    public interface IBuildComponentInitialize : IHierarchicalComponentInitialize<BuildConfiguration, IBuildComponent> { }
}
