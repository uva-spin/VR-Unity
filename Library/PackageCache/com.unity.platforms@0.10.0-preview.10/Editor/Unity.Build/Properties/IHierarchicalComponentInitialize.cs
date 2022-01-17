using Unity.Properties.Editor;

namespace Unity.Build
{
    /// <summary>
    /// Base interface for components that wants to provide an initialization method.
    /// </summary>
    /// <typeparam name="TContainer">The container type.</typeparam>
    /// <typeparam name="TComponent">The component type.</typeparam>
    public interface IHierarchicalComponentInitialize<TContainer, TComponent>
        where TContainer : HierarchicalComponentContainer<TContainer, TComponent>
    {
        /// <summary>
        /// Initialize a component.
        /// Called after the component has been constructed with the <see cref="TypeConstruction"/> utility.
        /// </summary>
        /// <param name="container">The component container as read-only.</param>
        void Initialize(HierarchicalComponentContainer<TContainer, TComponent>.ReadOnly container);
    }
}
