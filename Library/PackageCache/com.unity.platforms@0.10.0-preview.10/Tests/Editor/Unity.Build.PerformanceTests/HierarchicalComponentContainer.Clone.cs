using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Unity.Build.PerformanceTests
{
    [TestFixture]
    partial class HierarchicalComponentContainerPerformanceTests
    {
        [Test, Performance]
        public void Clone()
        {
            var component = new TestComponent();
            Measure.Method(() =>
            {
                TestContainer.Clone(component);
            }).WarmupCount(1).MeasurementCount(1000).Run();
        }
    }
}
