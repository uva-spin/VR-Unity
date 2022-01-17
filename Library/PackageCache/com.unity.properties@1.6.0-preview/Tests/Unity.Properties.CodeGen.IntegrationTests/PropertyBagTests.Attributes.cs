using System;
using NUnit.Framework;
using Unity.Properties.Internal;

namespace Unity.Properties.CodeGen.IntegrationTests
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class CustomAttribute : Attribute
    {
            
    }
        
    class ClassWithAttributeOnPrivateField
    {
        [CustomAttribute, CreateProperty] int Value;
    }

    [GeneratePropertyBag]
    class SuperClassWithAttributeOnPrivateField : ClassWithAttributeOnPrivateField
    {
            
    }

    [TestFixture]
    partial class PropertyBagTests
    {
        [Test]
        public void ClassThatInheritsClassWithAttributes_HasPropertyBagGenerated()
        {
            var container = default(SuperClassWithAttributeOnPrivateField);
            var properties = (PropertyBagStore.GetPropertyBag(typeof(SuperClassWithAttributeOnPrivateField)) as IPropertyList<SuperClassWithAttributeOnPrivateField>).GetProperties(ref container);

            Assert.That(properties.Count, Is.EqualTo(1));
            Assert.That(properties[0].HasAttribute<CustomAttribute>());
        }
    }
}