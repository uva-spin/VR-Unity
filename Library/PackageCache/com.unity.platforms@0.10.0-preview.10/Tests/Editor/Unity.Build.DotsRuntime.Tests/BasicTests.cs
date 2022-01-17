using NUnit.Framework;

namespace Unity.Build.DotsRuntime.Tests
{
    class BasicTests
    {
        [Test]
        public void VerifyCanReferenceEditorBuildTarget()
        {
            Assert.IsNotNull(typeof(EditorBuildTarget));
        }
    }
}
