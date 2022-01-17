using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Unity.Properties.Editor;
using Unity.Serialization.Json;

namespace Unity.Build
{
    static class BuildArtifacts
    {
        internal const string k_BaseDirectory = "Library/BuildArtifacts";

        class ArtifactData
        {
            public BuildResult Result;
            public IBuildArtifact[] Artifacts;
        }

        public static bool HasBuildArtifact(BuildConfiguration config, Type buildArtifactType)
        {
            ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            return GetAllBuildArtifacts(config).Any(a => buildArtifactType.IsAssignableFrom(a.GetType()));
        }

        public static bool HasBuildArtifact<T>(BuildConfiguration config) where T : class, IBuildArtifact, new()
        {
            return GetAllBuildArtifacts(config).Any(a => typeof(T).IsAssignableFrom(a.GetType()));
        }

        public static IBuildArtifact GetBuildArtifact(BuildConfiguration config, Type buildArtifactType)
        {
            ValidateBuildArtifactTypeAndThrow(buildArtifactType);
            return GetAllBuildArtifacts(config)?.FirstOrDefault(a => buildArtifactType.IsAssignableFrom(a.GetType()));
        }

        public static T GetBuildArtifact<T>(BuildConfiguration config) where T : class, IBuildArtifact, new()
        {
            return (T)GetAllBuildArtifacts(config)?.FirstOrDefault(a => typeof(T).IsAssignableFrom(a.GetType()));
        }

        public static IEnumerable<IBuildArtifact> GetAllBuildArtifacts(BuildConfiguration config)
        {
            return GetBuildArtifactData(config)?.Artifacts ?? Enumerable.Empty<IBuildArtifact>();
        }

        public static BuildResult GetBuildResult(BuildConfiguration config)
        {
            return GetBuildArtifactData(config)?.Result;
        }

        public static void CleanBuildArtifact(BuildConfiguration config)
        {
            var path = GetBuildArtifactPath(config);
            if (string.IsNullOrEmpty(path))
                return;

            if (File.Exists(path))
                File.Delete(path);
        }

        public static void CleanAllBuildArtifacts()
        {
            if (Directory.Exists(k_BaseDirectory))
                Directory.Delete(k_BaseDirectory, true);
        }

        internal static void ValidateBuildArtifactTypeAndThrow(Type buildArtifactType)
        {
            if (buildArtifactType == null)
                throw new ArgumentNullException(nameof(buildArtifactType));

            if (buildArtifactType == typeof(object))
                throw new InvalidOperationException("Build artifact type cannot be object.");

            if (!buildArtifactType.IsClass)
                throw new InvalidOperationException($"Build artifact type {buildArtifactType.FullName} is not a class.");

            if (!typeof(IBuildArtifact).IsAssignableFrom(buildArtifactType))
                throw new InvalidOperationException($"Build artifact type {buildArtifactType.FullName} does not derive from {typeof(IBuildArtifact).FullName}.");

            if (!TypeConstruction.CanBeConstructed(buildArtifactType))
                throw new InvalidOperationException($"Build artifact type {buildArtifactType.FullName} cannot be constructed because it does not have a default, implicit or registered constructor.");
        }

        static string GetBuildArtifactPath(BuildConfiguration config)
        {
            if (config == null || !config)
                throw new ArgumentNullException(nameof(config));

            var json = config.SerializeToJson();
            if (string.IsNullOrEmpty(json))
                return null;

            var name = config.name ?? string.Empty;
            var hash = ComputeHashString(name + json);
            return Path.Combine(k_BaseDirectory, hash).ToForwardSlash();
        }

        static string ComputeHashString(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var bytes = Encoding.ASCII.GetBytes(input);
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);
            var output = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; ++i)
                output.Append(hash[i].ToString("x2"));

            return output.ToString();
        }

        static ArtifactData GetBuildArtifactData(BuildConfiguration config)
        {
            if (config == null || !config)
                throw new ArgumentNullException(nameof(config));

            return Deserialize(config);
        }

        internal static void Serialize(BuildResult result, IBuildArtifact[] artifacts)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            if (artifacts == null)
                throw new ArgumentNullException(nameof(artifacts));

            var path = GetBuildArtifactPath(result.BuildConfiguration);
            if (string.IsNullOrEmpty(path))
                return;

            var data = new ArtifactData
            {
                Result = result,
                Artifacts = artifacts
            };

            var file = new FileInfo(path);
            file.WriteAllText(JsonSerialization.ToJson(data, new JsonSerializationParameters
            {
                DisableRootAdapters = true,
                SerializedType = typeof(ArtifactData)
            }));
        }

        static ArtifactData Deserialize(BuildConfiguration config)
        {
            if (config == null || !config)
                return null;

            var path = GetBuildArtifactPath(config);
            if (string.IsNullOrEmpty(path))
                return null;

            var file = new FileInfo(path);
            if (!file.Exists)
                return null;

            return JsonSerialization.TryFromJson<ArtifactData>(file, out var artifactData, out _, new JsonSerializationParameters
            {
                DisableRootAdapters = true,
                SerializedType = typeof(ArtifactData)
            }) ? artifactData : null;
        }
    }
}
