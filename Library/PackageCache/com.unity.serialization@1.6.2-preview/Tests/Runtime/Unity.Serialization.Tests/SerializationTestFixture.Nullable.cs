using NUnit.Framework;

namespace Unity.Serialization.Tests
{
    [TestFixture]
    partial class SerializationTestFixture
    {
        [Test]
        public void ClassWithNullableInt32_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithNullable
            {
                NullableInt32 = null
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.NullableInt32, Is.EqualTo(src.NullableInt32));
        }
        
        [Test]
        public void ClassWithNullableInt32_WhenValueIsNotNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithNullable
            {
                NullableInt32 = 10
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.NullableInt32, Is.EqualTo(src.NullableInt32));
        }
        
        [Test]
        public void ClassWithNullableEnum_WhenValueIsNotNull_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithNullable
            {
                NullableEnumUInt8 = EnumUInt8.Value1
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.NullableEnumUInt8, Is.EqualTo(src.NullableEnumUInt8));
        }
        
        [Test]
        public void ClassWithNullableEnum_CanBeSerializedAndDeserialized_CanBeSerializedAndDeserialized()
        {
            var src = new ClassWithNullable
            {
                NullableEnumUInt8 = null
            };
            
            var dst = SerializeAndDeserialize(src);
            
            Assert.That(dst, Is.Not.SameAs(src));
            Assert.That(dst.NullableEnumUInt8, Is.EqualTo(src.NullableEnumUInt8));
        }
    }
}