using NUnit.Framework;
using System;
using System.Linq;

namespace Unity.Build.Tests
{
    class ContextBaseTests : BuildTestsBase
    {
        class TestContextBase : ContextBase
        {
            public TestContextBase() : base() { }
            public TestContextBase(BuildPipelineBase pipeline, BuildConfiguration config) : base(pipeline, config) { }
        }

        class TestValueA { }
        class TestValueB { }

        [Test]
        public void HasValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.HasValue<TestValueB>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.HasValue<object>());
        }

        [Test]
        public void GetValue()
        {
            var context = new TestContextBase();
            var value = new TestValueA();
            context.SetValue(value);
            Assert.That(context.GetValue<TestValueA>(), Is.EqualTo(value));
            Assert.Throws<InvalidOperationException>(() => context.GetValue<object>());
        }

        [Test]
        public void GetValue_WhenValueDoesNotExist_IsNull()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValue<TestValueA>(), Is.Null);
        }

        [Test]
        public void GetOrCreateValue()
        {
            var context = new TestContextBase();
            Assert.That(context.GetOrCreateValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(1));
            Assert.Throws<InvalidOperationException>(() => context.GetOrCreateValue<object>());
        }

        [Test]
        public void GetOrCreateValue_WhenValueExist_DoesNotThrow()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.DoesNotThrow(() => context.GetOrCreateValue<TestValueA>());
        }

        [Test]
        public void GetValueOrDefault()
        {
            var context = new TestContextBase();
            Assert.That(context.GetValueOrDefault<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueA>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.GetValueOrDefault<object>());
        }

        [Test]
        public void SetValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            context.SetValue<TestValueB>();
            Assert.That(context.HasValue<TestValueA>(), Is.True);
            Assert.That(context.GetValue<TestValueA>(), Is.Not.Null);
            Assert.That(context.HasValue<TestValueB>(), Is.True);
            Assert.That(context.GetValue<TestValueB>(), Is.Not.Null);
            Assert.That(context.Values.Length, Is.EqualTo(2));
            Assert.Throws<ArgumentNullException>(() => context.SetValue<TestValueA>(null));
            Assert.Throws<InvalidOperationException>(() => context.SetValue(new object()));
        }

        [Test]
        public void SetValue_WhenValueExist_OverrideValue()
        {
            var context = new TestContextBase();
            var instance1 = new TestValueA();
            var instance2 = new TestValueA();

            context.SetValue(instance1);
            Assert.That(context.Values, Is.EqualTo(new[] { instance1 }));

            context.SetValue(instance2);
            Assert.That(context.Values, Is.EqualTo(new[] { instance2 }));
        }

        [Test]
        public void RemoveValue()
        {
            var context = new TestContextBase();
            context.SetValue(new TestValueA());
            Assert.That(context.Values.Length, Is.EqualTo(1));
            Assert.That(context.RemoveValue<TestValueA>(), Is.True);
            Assert.That(context.Values.Length, Is.Zero);
            Assert.Throws<InvalidOperationException>(() => context.RemoveValue<object>());
        }

        [Test]
        public void HasComponent()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.HasComponent<TestBuildComponentA>(), Is.True);
            Assert.That(context.HasComponent<TestBuildComponentB>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.HasComponent<TestBuildComponentC>());
            Assert.Throws<ArgumentNullException>(() => context.HasComponent(null));
            Assert.Throws<InvalidOperationException>(() => context.HasComponent(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.HasComponent(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void IsComponentInherited()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            Assert.That(context.IsComponentInherited<TestBuildComponentA>(), Is.False);
            Assert.That(context.IsComponentInherited<TestBuildComponentB>(), Is.True);
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited<TestBuildComponentC>());

            Assert.Throws<ArgumentNullException>(() => context.IsComponentInherited(null));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentInherited(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void IsComponentOverriding()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.SetComponent<TestBuildComponentB>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            Assert.That(context.IsComponentOverriding<TestBuildComponentA>(), Is.True);
            Assert.That(context.IsComponentOverriding<TestBuildComponentB>(), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverriding<TestBuildComponentC>());

            Assert.Throws<ArgumentNullException>(() => context.IsComponentOverriding(null));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverriding(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.IsComponentOverriding(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void TryGetComponent()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentA>());
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.TryGetComponent<TestBuildComponentA>(out _), Is.True);
            Assert.That(context.TryGetComponent<TestBuildComponentB>(out _), Is.False);
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent<TestBuildComponentC>(out _));
            Assert.Throws<ArgumentNullException>(() => context.TryGetComponent(null, out _));
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent(typeof(object), out _));
            Assert.Throws<InvalidOperationException>(() => context.TryGetComponent(typeof(TestBuildComponentInvalid), out _));
        }

        [Test]
        public void GetComponentOrDefault()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var config = BuildConfiguration.CreateInstance();
            var context = new TestContextBase(pipeline, config);
            Assert.That(context.HasComponent<TestBuildComponentA>(), Is.False);
            Assert.That(context.GetComponentOrDefault<TestBuildComponentA>(), Is.Not.Null);
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault<TestBuildComponentC>());
            Assert.Throws<ArgumentNullException>(() => context.GetComponentOrDefault(null));
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.GetComponentOrDefault(typeof(TestBuildComponentInvalid)));
        }

        [Test]
        public void GetComponents()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            var components = context.GetComponents();
            Assert.That(components.Count, Is.EqualTo(2));
            Assert.That(components.Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestBuildComponentA), typeof(TestBuildComponentB) }));

            configA.SetComponent<TestBuildComponentC>();
            Assert.Throws<InvalidOperationException>(() => context.GetComponents());
        }

        [Test]
        public void GetComponents_WithType()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);

            var components = context.GetComponents<TestBuildComponentA>();
            Assert.That(components.Count, Is.EqualTo(1));
            Assert.That(components.Select(c => c.GetType()), Is.EquivalentTo(new[] { typeof(TestBuildComponentA) }));
            Assert.Throws<InvalidOperationException>(() => context.GetComponents<TestBuildComponentC>());

            configA.SetComponent<TestBuildComponentC>();
            Assert.Throws<InvalidOperationException>(() => context.GetComponents<TestBuildComponentC>());
        }

        [Test]
        public void GetComponentTypes()
        {
            var pipeline = new TestBuildPipelineWithUsedComponents();
            var configB = BuildConfiguration.CreateInstance(c => c.SetComponent<TestBuildComponentB>());
            var configA = BuildConfiguration.CreateInstance(c =>
            {
                c.SetComponent<TestBuildComponentA>();
                c.AddDependency(configB);
            });
            var context = new TestContextBase(pipeline, configA);
            Assert.That(context.GetComponentTypes(), Is.EquivalentTo(new[] { typeof(TestBuildComponentA), typeof(TestBuildComponentB) }));
        }

        [Test]
        public void HasBuildArtifact()
        {
            var pipeline = new TestBuildPipelineWithBuildArtifact();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            var context = new TestContextBase(pipeline, config);

            Assert.That(context.HasBuildArtifact<TestBuildArtifactA>(), Is.False);
            Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.False);

            config.Build();
            Assert.That(context.HasBuildArtifact<TestBuildArtifactA>(), Is.True);
            Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.False);
            Assert.That(config.Run().Succeeded, Is.True);

            config.CleanBuildArtifact();
            Assert.That(context.HasBuildArtifact<TestBuildArtifactA>(), Is.False);
            Assert.That(context.HasBuildArtifact<TestBuildArtifactB>(), Is.False);

            Assert.Throws<ArgumentNullException>(() => context.HasBuildArtifact(null));
            Assert.Throws<InvalidOperationException>(() => context.HasBuildArtifact(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.HasBuildArtifact(typeof(TestBuildArtifactInvalidA)));
            Assert.Throws<InvalidOperationException>(() => context.HasBuildArtifact(typeof(TestBuildArtifactInvalidB)));
        }

        [Test]
        public void GetBuildArtifact()
        {
            var pipeline = new TestBuildPipelineWithBuildArtifact();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            var context = new TestContextBase(pipeline, config);

            Assert.That(context.GetBuildArtifact<TestBuildArtifactA>(), Is.Null);
            Assert.That(context.GetBuildArtifact<TestBuildArtifactB>(), Is.Null);

            config.Build();
            Assert.That(context.GetBuildArtifact<TestBuildArtifactA>(), Is.Not.Null);
            Assert.That(context.GetBuildArtifact<TestBuildArtifactB>(), Is.Null);
            Assert.That(config.Run().Succeeded, Is.True);

            config.CleanBuildArtifact();
            Assert.That(context.GetBuildArtifact<TestBuildArtifactA>(), Is.Null);
            Assert.That(context.GetBuildArtifact<TestBuildArtifactB>(), Is.Null);

            Assert.Throws<ArgumentNullException>(() => context.GetBuildArtifact(null));
            Assert.Throws<InvalidOperationException>(() => context.GetBuildArtifact(typeof(object)));
            Assert.Throws<InvalidOperationException>(() => context.GetBuildArtifact(typeof(TestBuildArtifactInvalidA)));
            Assert.Throws<InvalidOperationException>(() => context.GetBuildArtifact(typeof(TestBuildArtifactInvalidB)));
        }

        [Test]
        public void GetAllBuildArtifacts()
        {
            var pipeline = new TestBuildPipelineWithBuildArtifact();
            var config = BuildConfiguration.CreateInstance(c => c.SetComponent(new TestBuildPipelineComponent { Pipeline = pipeline }));
            var context = new TestContextBase(pipeline, config);

            Assert.That(context.GetAllBuildArtifacts(), Is.Empty);

            config.Build();
            Assert.That(context.GetAllBuildArtifacts().Select(a => a.GetType()), Is.EquivalentTo(new[] { typeof(TestBuildArtifactA) }));
            Assert.That(config.Run().Succeeded, Is.True);

            config.CleanBuildArtifact();
            Assert.That(context.GetAllBuildArtifacts(), Is.Empty);
        }
    }
}
