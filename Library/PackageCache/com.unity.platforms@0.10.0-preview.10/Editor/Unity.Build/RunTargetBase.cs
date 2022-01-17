namespace Unity.Build
{
    /// <summary>
    /// Represents the deployement target where you can run your application.
    /// </summary>
    public abstract class RunTargetBase
    {
        /// <summary>
        /// Display Name suitable for use in UI.
        /// </summary>
        public abstract string DisplayName { get; protected set; }

        /// <summary>
        /// Stable Id between runs, can be used for storing in settings and similar.
        /// </summary>
        public abstract string UniqueId { get; }

        /// <summary>
        /// Deploy application to the run target.
        /// </summary>
        public abstract void Deploy(string applicationId, string path);

        /// <summary>
        /// Asynchronously start application on the run target.
        /// </summary>
        public abstract void Start(string applicationId);

        /// <summary>
        /// Force stop application on the run target.
        /// Application is guaranteed to be stopped after this method returns.
        /// </summary>
        public abstract void ForceStop(string applicationId);
    }
}
