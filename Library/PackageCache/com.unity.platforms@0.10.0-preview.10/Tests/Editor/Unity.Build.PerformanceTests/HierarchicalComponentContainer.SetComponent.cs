using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.PerformanceTesting;
using Unity.Properties.Editor;

namespace Unity.Build.PerformanceTests
{
    [TestFixture]
    partial class HierarchicalComponentContainerPerformanceTests
    {
        [Test, Performance]
        public void SetComponent()
        {
            var container = TestContainer.CreateInstance();
            Measure.Method(() =>
            {
                container.SetComponent<TestComponent>();
            }).WarmupCount(1).MeasurementCount(1000).Run();
        }

        [Test, Performance]
        public void SetComponent_WithValue()
        {
            var container = TestContainer.CreateInstance();
            var component = new TestComponent();
            Measure.Method(() =>
            {
                container.SetComponent(component);
            }).WarmupCount(1).MeasurementCount(1000).Run();
        }
    }
}
