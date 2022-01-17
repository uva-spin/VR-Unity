using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Unity.Build.PerformanceTests
{
    [TestFixture]
    partial class HierarchicalComponentContainerPerformanceTests
    {
        [Test, Performance]
        public void Construct()
        {
            Measure.Method(() =>
            {
                TestContainer.Construct(typeof(TestComponent));
            }).WarmupCount(1).MeasurementCount(1000).Run();
        }
    }
}
