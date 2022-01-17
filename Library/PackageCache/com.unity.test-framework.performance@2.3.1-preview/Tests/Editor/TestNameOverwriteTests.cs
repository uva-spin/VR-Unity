using System;
using System.Collections;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEngine.TestTools;

namespace TestNameOverwrite
{
    public class TestNameOverwriteInNamespaceTests
    {
        [Test, Performance]
        public void TestName_NameSpace_Default()
        {
            Assert.AreEqual(PerformanceTest.Active.Name,$"TestNameOverwrite.TestNameOverwriteInNamespaceTests.TestName_NameSpace_Default");
        }
        
        [Performance, TestCase(TestName = "TestName_OverwriteInTestCase")]
        public void TestName_Overwrite_TestCase()
        {
            Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwrite.TestNameOverwriteInNamespaceTests.TestName_Overwrite_TestCase.TestName_OverwriteInTestCase");
        }
    }
}

public class TestNameOverwriteTests
{
    private static int[] integer = new int[] {2};
    
    [Performance]
    [TestCase(1)]
    [TestCaseSource(nameof(integer))]
    public void TestName_TestCase_Default(int n)
    {
        Assert.AreEqual(PerformanceTest.Active.Name,$"TestNameOverwriteTests.TestName_TestCase_Default({n})");
    }
    
    [Performance, TestCase(TestName = "TestName_OverwriteInTestCase")]
    public void TestName_Overwrite_TestCase()
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_Overwrite_TestCase.TestName_OverwriteInTestCase");
    }
    
    [Performance, TestCaseSource(typeof(MyFactoryClass),"TestCases")]
    public void TestName_Overwrite_TestCaseSource()
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_Overwrite_TestCaseSource.TestName_OverwriteInTestCaseSource");
    }
    
    [Test, Performance]
    public void TestName_OverwriteMethodName( [ValueSource(nameof(MethodWrapper))] TestWrapper wrapper)
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_OverwriteMethodName(OverwriteTestMethodName)");
    }
    
    [UnityTest, Performance]
    [TestCase(1)]
    [TestCaseSource(nameof(integer))]
    public void TestName_TestCase_Default_UnityTest(int n)
    {
        Assert.AreEqual(PerformanceTest.Active.Name,$"TestNameOverwriteTests.TestName_TestCase_Default_UnityTest({n})");
    }
    
    [Performance, TestCase(TestName = "TestName_OverwriteInTestCase")]
    public void TestName_Overwrite_TestCase_UnityTest()
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_Overwrite_TestCase_UnityTest.TestName_OverwriteInTestCase");
    }
    
    [Performance, TestCaseSource(typeof(MyFactoryClass),"TestCases")]
    public void TestName_Overwrite_TestCaseSource_UnityTest()
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_Overwrite_TestCaseSource_UnityTest.TestName_OverwriteInTestCaseSource");
    }
    
    [Test, Performance]
    public void TestName_OverwriteMethodName_UnityTest( [ValueSource(nameof(MethodWrapper))] TestWrapper wrapper)
    {
        Assert.AreEqual(PerformanceTest.Active.Name, "TestNameOverwriteTests.TestName_OverwriteMethodName_UnityTest(OverwriteTestMethodName)");
    }
    
    public struct TestWrapper
    {
        private Func<String> func;

        public TestWrapper(Func<String> func)
        {
            this.func = func;
        }

        public override string ToString()
        {
            return "OverwriteTestMethodName";
        }
    }

    private static TestWrapper[] MethodWrapper =
    {
        new TestWrapper(TestMethod),
    };
    
    private static String TestMethod()
    {
        return "TestMethod";
    }
}

public class MyFactoryClass
{
    public static IEnumerable TestCases
    {
        get
        {
            yield return new TestCaseData()
                .SetName("TestName_OverwriteInTestCaseSource");
        }
    }
}