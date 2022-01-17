using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Unity.Build.PerformanceTests
{
    [TestFixture]
    partial class HierarchicalComponentContainerPerformanceTests
    {
        [Test, Performance]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void GetComponent(int depth)
        {
            var container = CreateContainerHierarchy(depth, c => c.SetComponent<TestComponent>());
            Measure.Method(() =>
            {
                container.GetComponent<TestComponent>();
            }).WarmupCount(1).MeasurementCount(1000).Run();
        }
    }
}
