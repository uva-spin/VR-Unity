using System.Linq;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [GeneratePropertyBag]
    public class ClassWithAnonymousType
    {
        public (int IntValue, float FloatValue) AnonymousValue;
    }

    [TestFixture]
    sealed partial class PropertyBagTests
    {
        [Test]
        public void ClassWithAnonymousType_HasPropertyBagGenerated()
        {
            // Check properties are generated for anonymous field types.
            {
                var propertyBag = PropertyBagStore.GetPropertyBag(typeof(ClassWithAnonymousType));
            
                Assert.That(propertyBag, Is.InstanceOf(typeof(ContainerPropertyBag<ClassWithAnonymousType>)));

                var typed = propertyBag as ContainerPropertyBag<ClassWithAnonymousType>;
                var container = default(ClassWithAnonymousType);
                var properties = typed.GetProperties(ref container);
            
                Assert.That(properties.Count(), Is.EqualTo(1));
                Assert.That(properties.ElementAt(0), Is.InstanceOf(typeof(Property<ClassWithAnonymousType, (int IntValue, float FloatValue)>)));
            }

            // Check that the anonymous type has a property bag generated
            {
                var propertyBag = PropertyBagStore.GetPropertyBag(typeof((int IntValue, float FloatValue)));
                Assert.That(propertyBag, Is.InstanceOf(typeof(ContainerPropertyBag<(int IntValue, float FloatValue)>)));
                
                var typed = propertyBag as ContainerPropertyBag<(int IntValue, float FloatValue)>;
                var container = default((int IntValue, float FloatValue));
                var properties = typed.GetProperties(ref container);
                
                Assert.That(properties.Count(), Is.EqualTo(2));
                Assert.That(properties.ElementAt(0), Is.InstanceOf(typeof(Property<(int IntValue, float FloatValue), int>)));
                Assert.That(properties.ElementAt(1), Is.InstanceOf(typeof(Property<(int IntValue, float FloatValue), float>)));
            }
        }
    }
}