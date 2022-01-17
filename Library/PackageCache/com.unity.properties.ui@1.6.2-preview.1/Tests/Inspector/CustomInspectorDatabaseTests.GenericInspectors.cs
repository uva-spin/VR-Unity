using System;
using NUnit.Framework;
using Unity.Properties.UI.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Properties.UI.Tests
{
    [UI]
    partial class CustomInspectorDatabaseTests
    {
        interface IGenericType<T> {}
        class GenericType<T> : IGenericType<T> { }
        class IGenericInspector<T> : Inspector<IGenericType<T>>, IExperimentalInspector { }
        class GenericInspector<T> : Inspector<GenericType<T>>, IExperimentalInspector { }
        class IGenericIntInspector : Inspector<IGenericType<int>>, IExperimentalInspector { }
        class GenericIntInspector : Inspector<GenericType<int>>, IExperimentalInspector { }
        class TooManyArgumentsInspector<T1, T2> : Inspector<GenericType<T1>>, IExperimentalInspector { }
        class TooManyArgumentsInspector<T> : Inspector<GenericType<int>>, IExperimentalInspector { }
        
        interface IGenericType<T1, T2> {}
        class GenericType<T1, T2> : IGenericType<T1, T2> { }
        class IGenericInspector<T1, T2> : Inspector<IGenericType<T1, T2>>, IExperimentalInspector { }
        class GenericInspector<T1, T2> : Inspector<GenericType<T1, T2>>, IExperimentalInspector { }
        class IMyIdentityGenericInspector<T> : Inspector<IGenericType<T, T>>, IExperimentalInspector { }
        class MyIdentityGenericInspector<T> : Inspector<GenericType<T, T>>, IExperimentalInspector { }
        class IGenericStringStringInspector : Inspector<IGenericType<string, string>>, IExperimentalInspector { }
        class GenericStringStringInspector : Inspector<GenericType<string, string>>, IExperimentalInspector { }
        class IPartialDerivedGenericInspector<T> : IGenericInspector<int, T>, IExperimentalInspector { }
        class PartialDerivedGenericInspector<T> : GenericInspector<int, T>, IExperimentalInspector { }
        class IResolvedDerivedGenericInspector : IPartialDerivedGenericInspector<Vector2>, IExperimentalInspector { }
        class ResolvedDerivedGenericInspector : PartialDerivedGenericInspector<Vector2>, IExperimentalInspector { }

        interface IDeviousGenericType<T1, T2> { }
        class DeviousGenericType<T1, T2> : IDeviousGenericType<T1, T2> { }
        class IDeviousGenericInspector<T1, T2> : Inspector<IDeviousGenericType<T2, T1>>, IExperimentalInspector { }
        class DeviousGenericInspector<T1, T2> : Inspector<DeviousGenericType<T2, T1>>, IExperimentalInspector { }
        
        class UnresolvableGeneric<T1, T2> {}
        class UnresolvableGenericInspector<T> : Inspector<UnresolvableGeneric<T, int>>, IExperimentalInspector { }
        class UnresolvableGenericInspector2<T> : Inspector<UnresolvableGeneric<int, T>>, IExperimentalInspector { }

        class ExperimentalGeneric<T1, T2> { }
        class ExperimentalGenericInspector<T1, T2> : Inspector<ExperimentalGeneric<T1, T2>> { }
        class ExperimentalGenericInspector<T> : Inspector<ExperimentalGeneric<T, T>>, IExperimentalInspector { }

        class DefaultGenericInspector<T> : Inspector<T>, IExperimentalInspector
        {
            public override VisualElement Build()
            {
                throw new InvalidOperationException();
            }
        }

        [Test]
        public void CustomInspector_ForGenericTypes_IsSupported()
        {
            // Fully resolved inspectors for generic types
            AssertInspectorMatchesForType<GenericType<int>, GenericIntInspector>();
            AssertInspectorMatchesForType<GenericType<string, string>, GenericStringStringInspector>();
            AssertInspectorMatchesForType<GenericType<int, Vector2>, ResolvedDerivedGenericInspector>();
            
            // Generic inspectors for generic types (arguments match in given order)
            AssertInspectorMatchesForType<GenericType<float>, GenericInspector<float>>();
            AssertInspectorMatchesForType<GenericType<string, float>, GenericInspector<string, float>>();
            AssertInspectorMatchesForType<GenericType<int, float>, GenericInspector<int, float>>();
            AssertInspectorMatchesForType<GenericType<float, int>, GenericInspector<float, int>>();
            
            // Specialized generic inspectors for generic types (reusing the same arguments)
            AssertInspectorMatchesForType<GenericType<Vector2, Vector2>, MyIdentityGenericInspector<Vector2>>();
            AssertInspectorMatchesForType<GenericType<float, float>, MyIdentityGenericInspector<float>>();
            
            // Strange generic inspectors for generic types (arguments were provided in whatever order)
            AssertInspectorMatchesForType<DeviousGenericType<string, float>, DeviousGenericInspector<float, string>>();
        }

        [Test]
        public void CustomInspector_ForGenericInterfaces_IsSupported()
        {
            // Fully resolved inspectors for generic types
            AssertInspectorMatchesForType<IGenericType<int>, IGenericIntInspector>();
            AssertInspectorMatchesForType<IGenericType<string, string>, IGenericStringStringInspector>();
            AssertInspectorMatchesForType<IGenericType<int, Vector2>, IResolvedDerivedGenericInspector>();
            
            // Generic inspectors for generic types (arguments match in given order)
            AssertInspectorMatchesForType<IGenericType<float>, IGenericInspector<float>>();
            AssertInspectorMatchesForType<IGenericType<string, float>, IGenericInspector<string, float>>();
            AssertInspectorMatchesForType<IGenericType<int, float>, IGenericInspector<int, float>>();
            AssertInspectorMatchesForType<IGenericType<float, int>, IGenericInspector<float, int>>();
            
            // Specialized generic inspectors for generic types (reusing the same arguments)
            AssertInspectorMatchesForType<IGenericType<Vector2, Vector2>, IMyIdentityGenericInspector<Vector2>>();
            AssertInspectorMatchesForType<IGenericType<float, float>, IMyIdentityGenericInspector<float>>();
            
            // Strange generic inspectors for generic types (arguments were provided in whatever order)
            AssertInspectorMatchesForType<IDeviousGenericType<string, float>, IDeviousGenericInspector<float, string>>();
        }

        [Test]
        public void PartiallyResolvedCustomInspector_ForGenericTypes_IsNotSupported()
        {
            AssertNoInspectorMatchesForType<UnresolvableGeneric<float, int>>();
            AssertNoInspectorMatchesForType<UnresolvableGeneric<int, float>>();
        }
        
        [Test]
        public void CustomInspector_ForGenericTypes_RequiresExperimentalInterface()
        {
            AssertNoInspectorMatchesForType<ExperimentalGeneric<float, int>>();
            AssertInspectorMatchesForType<ExperimentalGeneric<float, float>, ExperimentalGenericInspector<float>>();
        }

        [Test]
        public void UnsupportedCustomInspector_ForGenericTypes_AreDetected()
        {
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(TooManyArgumentsInspector<,>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.GenericArgumentsDoNotMatchInspectedType));
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(TooManyArgumentsInspector<>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(UnresolvableGenericInspector<>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(UnresolvableGenericInspector2<>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.UnsupportedPartiallyResolvedGenericInspector));
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(ExperimentalGenericInspector<,>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.UnsupportedUserDefinedGenericInspector));
            Assert.That(CustomInspectorDatabase.GetRegistrationStatusForInspectorType(typeof(DefaultGenericInspector<>)), Is.EqualTo(CustomInspectorDatabase.RegistrationStatus.UnsupportedGenericInspectorForNonGenericType));
        }
    }
}