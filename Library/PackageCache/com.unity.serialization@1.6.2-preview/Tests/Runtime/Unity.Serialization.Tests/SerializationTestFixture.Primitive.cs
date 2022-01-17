using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithInt32_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithPrimitives
            {
                Int32Value = 42
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Int32Value, Is.EqualTo(src.Int32Value));
        }
        
        [Test]
        public void ClassWithFloat32_WhenValueIsVeryLarge_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithPrimitives
            {
                Float32Value = 3.402823E+38f
            };

            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.Float32Value, Is.EqualTo(src.Float32Value));
        }
    }
}