using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Unity.Build.PerformanceTests
{
    [TestFixture]
    partial class HierarchicalComponentContainerPerformanceTests
    {
        interface ITestComponent { }

        class TestContainer : HierarchicalComponentContainer<TestContainer, ITestComponent> { }

        class TestComponent : ITestComponent
        {
#pragma warning disable 649
            public int Integer;
            public float Float;
#pragma warning restore 649
            public List<int> List = new List<int>();
        }

        TestContainer CreateContainerHierarchy(int depth, Action<TestContainer> mutator)
        {
            var root = TestContainer.CreateInstance(mutator);
            root.name = "Depth 0";

            var parent = root;
            for (var i = 0; i < depth; ++i)
            {
                var child = TestContainer.CreateInstance(mutator);
                parent.AddDependency(child);
                parent = child;
            }

            return root;
        }
    }
}
