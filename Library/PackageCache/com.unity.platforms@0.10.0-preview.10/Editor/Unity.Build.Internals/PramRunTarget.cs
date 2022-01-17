using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Build.Internals
{
    internal class PramRunTarget : RunTargetBase
    {
        public override string DisplayName { get; protected set; }
        public override string UniqueId => $"{ProviderName}-{EnvironmentId}";

        private Pram Pram { get; }
        private string ProviderName { get; }
        private string EnvironmentId { get; }

        public PramRunTarget(Pram pram, string providerName, string environmentId)
        {
            Pram = pram;
            ProviderName = providerName;
            EnvironmentId = environmentId;
            DisplayName = $"{ProviderName} - {EnvironmentId}";

            // Asynchronously update display name. Often requires device communication.
            Task.Run(() =>
            {
                try
                {
                    DisplayName = Pram.GetName(ProviderName, EnvironmentId) ?? DisplayName;
                }
                catch
                {
                    Debug.LogWarning($"{ProviderName} {EnvironmentId} not reachable.");
                }
            });
        }

        public override void Deploy(string applicationId, string path)
        {
            Pram.Deploy(ProviderName, EnvironmentId, applicationId, path);
        }

        public override void Start(string applicationId)
        {
            Pram.Start(ProviderName, EnvironmentId, applicationId);
        }

        public override void ForceStop(string applicationId)
        {
            Pram.ForceStop(ProviderName, EnvironmentId, applicationId);
        }
    }
}
