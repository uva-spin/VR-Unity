using System.Globalization;
using NUnit.Framework;
using Unity.Collections;

namespace Unity.Serialization.Json.Tests
{
    [TestFixture]
    sealed class JsonStringBufferTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(42)]
        public void Write_WhenValueIsInt_ValueIsCorrectlyWritten(int value)
        {
            using (var writer = new JsonStringBuffer(16, Allocator.Temp))
            {
                writer.Write(value);

                var str = writer.ToString();

                Assert.That(str, Is.EqualTo(value.ToString(CultureInfo.InvariantCulture)));
            }
        }
        
        [TestCase(4.2f)]
        [TestCase(3.402823E+38f)]
        [TestCase(-1.202823E+38f)]
        [TestCase(0)]
        [TestCase(-1)]
        public void Write_WhenValueIsFloat_ValueIsCorrectlyWritten(float value)
        {
            using (var writer = new JsonStringBuffer(16, Allocator.Temp))
            {
                writer.Write(value);

                var str = writer.ToString();

                Assert.That(str, Is.EqualTo(value.ToString(CultureInfo.InvariantCulture)));
            }
        }
    }
}