#if ENABLE_PLAYMODE_EXTENSION
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Build.Classic;
using Unity.Build.Common;
using UnityEditor;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestRunner.Utils;
using UnityEngine.TestTools.TestRunner;
using UnityEngine.TestTools.TestRunner.Callbacks;


namespace Unity.Build.Playmode.TestRunner
{
    [Serializable]
    internal class BuildConfigurationPlayerLauncher : RuntimeTestLauncherBase
    {
        private readonly PlaymodeTestsControllerSettings m_Settings;
        private readonly TestJobData m_JobData;
        private readonly BuildTarget m_TargetPlatform;
        private readonly Platform m_BuildConfigurationPlatform;
        private ExecutionSettings ExecutionSettings => m_JobData.executionSettings;

        public BuildConfigurationPlayerLauncher(PlaymodeTestsControllerSettings settings, TestJobData jobData)
        {
            m_Settings = settings;
            m_JobData = jobData;
            m_TargetPlatform = ExecutionSettings.targetPlatform ?? EditorUserBuildSettings.activeBuildTarget;
            m_BuildConfigurationPlatform = m_TargetPlatform.GetPlatform() ?? throw new Exception($"Cannot resolve platform for {m_TargetPlatform}");
        }

        private static SceneList.SceneInfo GetSceneInfo(string path)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            return new SceneList.SceneInfo() { AutoLoad = true, Scene = GlobalObjectId.GetGlobalObjectIdSlow(sceneAsset) };
        }

        private string GetBuildConfiguratioName()
        {
            var name = m_BuildConfigurationPlatform.Name;
            return name;
        }

        private BuildConfiguration CreateBuildConfiguration(string name, string firstScenePath)
        {
            var config = BuildConfiguration.CreateInstance();

            config.name = name;

            config.SetComponent<GeneralSettings>();

            var scenes = new List<string>() { firstScenePath };
            scenes.AddRange(EditorBuildSettings.scenes.Select(x => x.path));

            // It's important to keep these as it is, since UTR uses this info
            // To resolve path to Player.log
            // Otherwise there will be a warning in TestRunnerLog.txt "warning: C:\Users\bokken\appdata\locallow\DefaultCompany\UnityTestFramework\Player.log doesn't exist at expected location."
            config.SetComponent(new GeneralSettings()
            {
                CompanyName = "DefaultCompany",
                ProductName = "UnityTestFramework"
            });

            config.SetComponent(new SceneList
            {
                SceneInfos = new List<SceneList.SceneInfo>(scenes.Select(s => GetSceneInfo(s)))
            });


            var profile = new ClassicBuildProfile()
            {
                Configuration = BuildType.Develop,
                Platform = m_BuildConfigurationPlatform
            };
            
            config.SetComponent(profile);

            config.SetComponent(new PlayerConnectionSettings()
            {
                Mode = Unity.Build.Classic.PlayerConnectionInitiateMode.Connect
            });

            config.SetComponent(new PlaymodeTestRunnerComponent());
            config.SetComponent(new OutputBuildDirectory()
            {
                OutputDirectory = Path.Combine("Temp", name + DateTime.Now.Ticks.ToString())
            });

            return config;
        }

        private Scene PrepareScene(Scene scene, string scenePath)
        {
            var runner = GameObject.Find(PlaymodeTestsController.kPlaymodeTestControllerName).GetComponent<PlaymodeTestsController>();
            runner.AddEventHandlerMonoBehaviour<PlayModeRunnerCallback>();
            runner.settings = m_Settings;
            runner.AddEventHandlerMonoBehaviour<RemoteTestResultSender>();
            runner.includedObjects = new ScriptableObject[]
                {ScriptableObject.CreateInstance<RuntimeTestRunCallbackListener>()};
            SaveScene(scene, scenePath);
            return scene;
        }

        public virtual void Run()
        {
            var editorConnectionTestCollector = RemoteTestRunController.instance;
            editorConnectionTestCollector.hideFlags = HideFlags.HideAndDontSave;
            editorConnectionTestCollector.Init(m_TargetPlatform, ExecutionSettings.playerHeartbeatTimeout);

            var remotePlayerLogController = RemotePlayerLogController.instance;
            remotePlayerLogController.hideFlags = HideFlags.HideAndDontSave;

            using (var settings = new BuildConfigurationPlayerLauncherContextSettings(ExecutionSettings.overloadTestRunSettings))
            {
                PrepareScene(m_JobData.InitTestScene, m_JobData.InitTestScenePath);

                var filter = m_Settings.BuildNUnitFilter();
                var runner = LoadTests(filter);
                var exceptionThrown = ExecutePreBuildSetupMethods(runner.LoadedTest, filter);
                if (exceptionThrown)
                {
                    CallbacksDelegator.instance.RunFailed("Run Failed: One or more errors in a prebuild setup. See the editor log for details.");
                    return;
                }

                var name = GetBuildConfiguratioName();
                var path = $"Assets/{m_BuildConfigurationPlatform.Name}.buildConfiguration";
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, $"Creating build configuration at path {path}");
                var config = CreateBuildConfiguration(name, m_JobData.InitTestScenePath);

                // In basic scenarios you can build without saving build configuration to disk
                // But dots related systems, require build configuration to be present on disk
                config.SerializeToPath(path);
                AssetDatabase.Refresh();

                var buildResult = config.Build();
                AssetDatabase.DeleteAsset(path);
                buildResult.LogResult();
                
                editorConnectionTestCollector.PostBuildAction();
                ExecutePostBuildCleanupMethods(runner.LoadedTest, filter);


                if (buildResult.Failed)
                {
                    ScriptableObject.DestroyImmediate(editorConnectionTestCollector);
                    Debug.LogError("Player build failed");
                    throw new TestLaunchFailedException("Player build failed");
                }

                editorConnectionTestCollector.PostSuccessfulBuildAction();

                var runResult = config.Run();
                runResult.LogResult();
                if (runResult.Failed)
                    throw new TestLaunchFailedException("Player run failed");
                editorConnectionTestCollector.PostSuccessfulLaunchAction();
            }
        }
    }
}
#endif
