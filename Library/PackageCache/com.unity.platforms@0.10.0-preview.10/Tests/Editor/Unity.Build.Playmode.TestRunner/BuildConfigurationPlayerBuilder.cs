#if ENABLE_PLAYMODE_EXTENSION
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.TestRunner.TestLaunchers;
using UnityEditor.TestTools.TestRunner;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor.TestTools.TestRunner.TestRun;
using UnityEditor.TestTools.TestRunner.TestRun.Tasks;
using UnityEngine;
using UnityEngine.TestTools.TestRunner;


namespace Unity.Build.Playmode.TestRunner
{
    class BuildConfigurationPlayerBuilder : IPlayerBuilder
    {
        private class BuildConfigurationPlayerRunTask : TestTaskBase
        {
            public override IEnumerator Execute(TestJobData testJobData)
            {
                var settings = PlaymodeTestsControllerSettings.CreateRunnerSettings(testJobData.executionSettings.filters.Select(filter => filter.ToTestRunnerFilter()).ToArray());
                var launcher = new BuildConfigurationPlayerLauncher(settings, testJobData);
                launcher.Run();
                yield return null;

                while (!RemoteTestRunController.instance.RunHasFinished)
                {
                    yield return null;
                }
            }
        }

        public string Name
        {
            get => "BuildConfiguration";
        }
        
        protected void RunTestsInPlayer()
        {
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.Execute(new ExecutionSettings()
            {
                filters = new[] { new Filter()
                {
                    testMode = TestMode.PlayMode,
                }},
                targetPlatform = EditorUserBuildSettings.activeBuildTarget,
                playmodeBuilderName = Name
            });
            GUIUtility.ExitGUI();
        }

        public void DoGUI()
        {
            if (GUILayout.Button($"Run All in player", EditorStyles.toolbarButton))
            {
                RunTestsInPlayer();
            }
        }

        public TestTaskBase ConstructRunTask()
        {
            return new BuildConfigurationPlayerRunTask();
        }
    }
}
#endif
