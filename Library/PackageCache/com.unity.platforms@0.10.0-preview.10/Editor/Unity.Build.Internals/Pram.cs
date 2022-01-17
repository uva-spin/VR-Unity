using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Unity.Build.Internals
{
    internal class Pram : RunTargetProviderBase
    {
        // Local pram development setup - set to machine local directory to quickly iterate on pram features.
        private static readonly string LocalPramDevelopmentRepository = null;
        // Enable tracing
        public static bool Trace { get; set; } = false;

        private IReadOnlyDictionary<string, string> PlatformAssemblyLoadPath { get; }
        private IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Environment { get; }

        private string PlatformsPackagePath { get; } = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.platforms").resolvedPath;

        public Pram()
        {
            var platformAssemblyLoadPath = new Dictionary<string, string>();
            var environment = new Dictionary<string, IReadOnlyDictionary<string, string>>();
            var plugins = TypeConstructionUtility.ConstructTypesDerivedFrom<PramPlatformPlugin>();
            foreach (var plugin in plugins)
            {
                foreach (var provider in plugin.Providers)
                {
                    platformAssemblyLoadPath[provider] = plugin.PlatformAssemblyLoadPath;
                    environment[provider] = plugin.Environment;
                }
            }

            PlatformAssemblyLoadPath = platformAssemblyLoadPath;
            Environment = environment;
        }

        public override IEnumerable<RunTargetBase> Discover() => Discover(Array.Empty<string>());

        public RunTargetBase[] GetDefault(params string[] providers) => QueryTargets("env-default", providers);

        public RunTargetBase[] Discover(params string[] providers) => QueryTargets("env-detect", providers);

        public void Deploy(string provider, string environmentId, string applicationId, string path) =>
            ExecuteEnvironmentCommand(provider, "app-deploy", environmentId, applicationId, $"\"{path}\"");

        public void Start(string provider, string environmentId, string applicationId) =>
            ExecuteEnvironmentCommand(provider, "app-start-detached", environmentId, applicationId);

        public void ForceStop(string provider, string environmentId, string applicationId) =>
            ExecuteEnvironmentCommand(provider, "app-kill", environmentId, applicationId);

        public string GetName(string provider, string environmentId) =>
            ParseYamlProperties(ExecuteEnvironmentCommand(provider, "env-props", environmentId))
                .Where(p => p.Key == "env.name")
                .Select(p => p.Value)
                .FirstOrDefault();


        // Note: Pram use a very simplified yaml output which essentially means, ':' for key value separation
        // and single-quotation of any escaped string
        private readonly Regex _yamlParserExpression = new Regex(@"(?<key>'.*?'|.*?):\s+(?<value>.*)");
        private static string Unquote(string str) => str.StartsWith("'") ? str.TrimStart('\'').TrimEnd('\'').Replace("''", "'") : str;
        private IEnumerable<KeyValuePair<string, string>> ParseYamlProperties(string yamlString) =>
            _yamlParserExpression.Matches(yamlString)
                .Cast<Match>()
                .Select(match => new KeyValuePair<string, string>(
                    Unquote(match.Groups["key"].Value.Trim()),
                    Unquote(match.Groups["value"].Value.Trim())))
                .ToArray<KeyValuePair<string, string>>();

        private RunTargetBase[] QueryTargets(string pramCommand, params string[] providers) =>
            ParseYamlProperties(Execute(providers, pramCommand))
                .Select(p => new PramRunTarget(this, p.Key, p.Value))
                .ToArray<RunTargetBase>();

        private string ExecuteEnvironmentCommand(string provider, string command, string environmentId, params string[] args) =>
            Execute(new[] { provider }, new[] { command, $"-e {environmentId}", provider }.Concat(args).ToArray());

        private string Execute(string[] providers, params string[] args)
        {
            var providersSet = (providers != null && providers.Any()) ? new HashSet<string>(providers) : null;

            var assemblyLoadPaths = PlatformAssemblyLoadPath
                .Where(x => providersSet?.Contains(x.Key) ?? true)
                .Select(x => x.Value)
                .ToArray();
            var environment = Environment
                .Where(x => providersSet?.Contains(x.Key) ?? true)
                .SelectMany(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            var pramExecutable = Path.Combine(PlatformsPackagePath, "Editor/Unity.Build.Internals/pram~/pram.exe");
            var platformAssembliesPaths = string.Join(" ", assemblyLoadPaths.Select(x => $"--assembly-load-path \"{x}\""));

            // Override executable and assembly load paths if local repository is used
            if (LocalPramDevelopmentRepository != null)
            {
                pramExecutable = $"\"{Path.Combine(LocalPramDevelopmentRepository, "artifacts/PramDistribution/pram.exe")}\"";
                platformAssembliesPaths = "";
            }

            var trace = Trace ? "--trace --very-verbose" : "";
#if UNITY_EDITOR_WIN
            var exe = ".exe";
#elif UNITY_EDITOR_OSX
            var exe = "";
#else
            var exe = "";
#endif            
            var monoExe = $"{EditorApplication.applicationContentsPath}/MonoBleedingEdge/bin/mono{exe}";
            var arguments = new List<string> { $"\"{pramExecutable}\"", trace, platformAssembliesPaths };
            arguments.AddRange(args.Select(arg => $"\"{arg}\""));
            var result = ShellProcess.Run(new ShellProcessArguments()
            {
                ThrowOnError = false,
                AnyOutputToErrorIsAnError = false,
                Executable = monoExe,
                Arguments = arguments.ToArray(),
                EnvironmentVariables = environment
            });

            if (Trace)
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", result.ErrorOutput);

            if (!result.Succeeded)
                throw new Exception($"Failed {result.Command}\n{result.ErrorOutput}");
            return result.FullOutput;
        }
    }
}
