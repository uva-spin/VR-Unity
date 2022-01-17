using NUnit.Framework;
using System;
using System.IO;
using Unity.Properties;
using UnityEngine;

namespace Unity.Build.Tests
{
    class BuildTestsBase
    {
        [HideInInspector]
        protected class TestBuildPipelineComponent : IBuildPipelineComponent
        {
            [CreateProperty] public BuildPipelineBase Pipeline { get; set; }
            public int SortingIndex => 0;
            public bool SetupEnvironment() => false;
        }

        [HideInInspector]
        protected class TestBuildComponentA : IBuildComponent { }

        [HideInInspector]
        protected class TestBuildComponentB : IBuildComponent { }

        [HideInInspector]
        protected class TestBuildComponentC : IBuildComponent { }

        protected class TestBuildComponentInvalid { }

        [HideInInspector]
        protected class TestBuildPipeline : BuildPipelineBase
        {
            protected override CleanResult OnClean(CleanContext context) => context.Success();
            protected override BuildResult OnBuild(BuildContext context) => context.Success();
            protected override RunResult OnRun(RunContext context) => context.Success(new TestRunInstance());
            public override DirectoryInfo GetOutputBuildDirectory(BuildConfiguration config) => throw new NotImplementedException();
        }

        [HideInInspector]
        protected class TestBuildPipelineCantBuild : TestBuildPipeline
        {
            protected override BoolResult OnCanBuild(BuildContext context) => BoolResult.False(nameof(TestBuildPipelineCantBuild));
        }

        [HideInInspector]
        protected class TestBuildPipelineBuildFails : TestBuildPipeline
        {
            protected override BuildResult OnBuild(BuildContext context) => context.Failure(nameof(TestBuildPipelineBuildFails));
        }

        [HideInInspector]
        protected class TestBuildPipelineBuildThrows : TestBuildPipeline
        {
            protected override BuildResult OnBuild(BuildContext context) => throw new InvalidOperationException(nameof(TestBuildPipelineBuildThrows));
        }

        [HideInInspector]
        protected class TestBuildPipelineNullBuildResult : TestBuildPipeline
        {
            protected override BuildResult OnBuild(BuildContext context) => null;
        }

        [HideInInspector]
        protected class TestBuildPipelineCantRun : TestBuildPipeline
        {
            protected override BoolResult OnCanRun(RunContext context) => BoolResult.False(nameof(TestBuildPipelineCantRun));
        }

        [HideInInspector]
        protected class TestBuildPipelineRunFails : TestBuildPipeline
        {
            protected override RunResult OnRun(RunContext context) => context.Failure(nameof(TestBuildPipelineCantRun));
        }

        [HideInInspector]
        protected class TestBuildPipelineRunThrows : TestBuildPipeline
        {
            protected override RunResult OnRun(RunContext context) => throw new InvalidOperationException(nameof(TestBuildPipelineRunThrows));
        }

        [HideInInspector]
        protected class TestBuildPipelineNullRunResult : TestBuildPipeline
        {
            protected override RunResult OnRun(RunContext context) => null;
        }

        [HideInInspector]
        protected class TestBuildPipelineWithUsedComponents : TestBuildPipeline
        {
            public override Type[] UsedComponents { get; } =
            {
                typeof(TestBuildComponentA),
                typeof(TestBuildComponentB)
            };

            protected override BuildResult OnBuild(BuildContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success();
            }

            protected override RunResult OnRun(RunContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success(new TestRunInstance());
            }
        }

        [HideInInspector]
        protected class TestBuildPipelineWithMissingComponents : TestBuildPipeline
        {
            protected override BuildResult OnBuild(BuildContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success();
            }

            protected override RunResult OnRun(RunContext context)
            {
                context.GetComponentOrDefault<TestBuildComponentA>();
                context.GetComponentOrDefault<TestBuildComponentB>();
                return context.Success(new TestRunInstance());
            }
        }

        [HideInInspector]
        protected class TestBuildPipelineWithInvalidComponents : TestBuildPipeline
        {
            public override Type[] UsedComponents { get; } = { typeof(TestBuildComponentInvalid) };
        }

        [HideInInspector]
        protected class TestBuildPipelineWithBuildArtifact : TestBuildPipeline
        {
            protected override BuildResult OnBuild(BuildContext context)
            {
                Assert.That(context.HasBuildArtifact<TestBuildArtifactA>(), Is.False);
                Assert.That(context.GetOrCreateBuildArtifact<TestBuildArtifactA>(), Is.Not.Null);
                Assert.That(context.HasBuildArtifact<TestBuildArtifactA>(), Is.True);
                Assert.That(context.RemoveValue<TestBuildArtifactA>(), Is.True);
                Assert.DoesNotThrow(() => context.SetValue<TestBuildArtifactA>());
                Assert.That(context.HasValue<TestBuildArtifactA>(), Is.True);

                Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.False);
                context.SetBuildArtifact(new TestBuildArtifactB());
                Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.True);
                Assert.That(context.RemoveBuildArtifact<TestBuildArtifactB>(), Is.True);
                Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.False);

                return context.Success();
            }

            protected override BoolResult OnCanRun(RunContext context)
            {
                var result = context.GetBuildResult();
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Succeeded, Is.True);

                var artifact = context.GetBuildArtifact<TestBuildArtifactA>();
                Assert.That(artifact, Is.Not.Null);
                Assert.Throws<NotSupportedException>(() => context.SetValue<TestBuildArtifactB>());
                Assert.Throws<NotSupportedException>(() => context.RemoveValue<TestBuildArtifactB>());

                return result.Succeeded && artifact != null ? BoolResult.True() : BoolResult.False(nameof(artifact));
            }

            protected override RunResult OnRun(RunContext context)
            {
                var result = context.GetBuildResult();
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Succeeded, Is.True);

                var artifact = context.GetBuildArtifact<TestBuildArtifactA>();
                Assert.That(artifact, Is.Not.Null);
                Assert.Throws<NotSupportedException>(() => context.SetValue<TestBuildArtifactB>());
                Assert.Throws<NotSupportedException>(() => context.RemoveValue<TestBuildArtifactB>());

                return result.Succeeded && artifact != null ? context.Success(new TestRunInstance()) : context.Failure(nameof(artifact));
            }

            protected override CleanResult OnClean(CleanContext context)
            {
                var result = context.GetBuildResult();
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Succeeded, Is.True);

                var artifact = context.GetBuildArtifact<TestBuildArtifactA>();
                Assert.That(artifact, Is.Not.Null);
                Assert.Throws<NotSupportedException>(() => context.SetValue<TestBuildArtifactB>());
                Assert.Throws<NotSupportedException>(() => context.RemoveValue<TestBuildArtifactB>());

                return result.Succeeded && artifact != null ? context.Success() : context.Failure(nameof(artifact));
            }
        }

        protected class TestBuildStepA : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Success();
        }

        protected class TestBuildStepB : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Success();
        }

        protected class TestBuildStepFails : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => context.Failure(nameof(TestBuildStepFails));
        }

        protected class TestBuildStepThrows : BuildStepBase
        {
            public override BuildResult Run(BuildContext context) => throw new InvalidOperationException(nameof(TestBuildStepThrows));
        }

        protected class TestBuildStepInvalid { }

        protected class TestRunInstance : IRunInstance
        {
            public TestRunInstance()
            {
                IsRunning = true;
            }

            public bool IsRunning { get; private set; }
            public void Dispose() { IsRunning = false; }
        }

        protected class TestBuildArtifactA : IBuildArtifact { }

        protected class TestBuildArtifactB : IBuildArtifact { }

        protected class TestBuildArtifactInvalidA { }

        protected struct TestBuildArtifactInvalidB : IBuildArtifact { }

        [SetUp]
        public void SetUp()
        {
            BuildArtifacts.CleanAllBuildArtifacts();
        }

        [TearDown]
        public void TearDown()
        {
            BuildArtifacts.CleanAllBuildArtifacts();
        }
    }
}
