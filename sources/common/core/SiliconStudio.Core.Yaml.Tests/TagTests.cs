using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;

namespace SiliconStudio.Core.Yaml.Tests
{
    // Note: do not move these types! If the namespace must be changed, be sure to update TagTests.Namespace.
    #region Types
    // ReSharper disable UnusedTypeParameter
    public class SimpleType { }

    public class SimpleType2 { }

    public class SimpleType3 { }

    public class SimpleType4 { }

    public class NestedTypeContainer { public class NestedType { public class NestedType2 { } } }

    public class GenericNestedTypeContainer<T> { public class NestedType { public class NestedType2 { } } }

    public class GenericNestedTypeContainer2<T1, T2> { public class NestedType<T3, T4> { public class NestedType2 { } } }

    public class GenericType<T> { }
    // ReSharper restore UnusedTypeParameter
    #endregion Types

    public class TagTests : YamlTest
    {
        private const string Namespace = "SiliconStudio.Core.Yaml.Tests";

        [Test]
        public void TestDefaultType()
        {
            TestType(typeof(int), "!!int");
        }

        [Test]
        public void TestCoreType()
        {
            TestType(typeof(Guid), "!System.Guid,mscorlib");
        }

        [Test]
        public void TestSimpleType()
        {
            TestType(typeof(SimpleType), $"!{Namespace}.SimpleType,{Namespace}");
        }

        [Test]
        public void TestNestedType()
        {
            TestType(typeof(NestedTypeContainer.NestedType), $"!{Namespace}.NestedTypeContainer+NestedType,{Namespace}");
        }

        [Test]
        public void TestDoubleNestedType()
        {
            TestType(typeof(NestedTypeContainer.NestedType.NestedType2), $"!{Namespace}.NestedTypeContainer+NestedType+NestedType2,{Namespace}");
        }

        [Test]
        public void TestGenericType()
        {
            TestType(typeof(GenericType<SimpleType>), $"!{Namespace}.GenericType%601[[{Namespace}.SimpleType,{Namespace}]],{Namespace}");
            TestType(typeof(GenericType<double>), $"!{Namespace}.GenericType%601[[System.Double,mscorlib]],{Namespace}");
        }

        [Test]
        public void TestGenericNestedType()
        {
            TestType(typeof(GenericNestedTypeContainer<SimpleType>.NestedType.NestedType2), $"!{Namespace}.GenericNestedTypeContainer%601+NestedType+NestedType2[[{Namespace}.SimpleType,{Namespace}]],{Namespace}");
            TestType(typeof(GenericNestedTypeContainer<int>.NestedType.NestedType2), $"!{Namespace}.GenericNestedTypeContainer%601+NestedType+NestedType2[[System.Int32,mscorlib]],{Namespace}");
        }

        [Test]
        public void TestGenericDoubleNestedType()
        {
            TestType(typeof(GenericNestedTypeContainer2<SimpleType, SimpleType2>.NestedType<SimpleType3, SimpleType4>.NestedType2), $"!{Namespace}.GenericNestedTypeContainer2%602+NestedType%602+NestedType2[[{Namespace}.SimpleType,{Namespace}],[{Namespace}.SimpleType2,{Namespace}],[{Namespace}.SimpleType3,{Namespace}],[{Namespace}.SimpleType4,{Namespace}]],{Namespace}");
            TestType(typeof(GenericNestedTypeContainer2<int, string>.NestedType<Guid, DateTime>.NestedType2), $"!{Namespace}.GenericNestedTypeContainer2%602+NestedType%602+NestedType2[[System.Int32,mscorlib],[System.String,mscorlib],[System.Guid,mscorlib],[System.DateTime,mscorlib]],{Namespace}");
        }

        [Test]
        public void TestNestedGenericType()
        {
            TestType(typeof(GenericType<NestedTypeContainer.NestedType.NestedType2>), $"!{Namespace}.GenericType%601[[{Namespace}.NestedTypeContainer+NestedType+NestedType2,{Namespace}]],{Namespace}");
        }

        [Test]
        public void TestGenericNestedGenericType()
        {
            TestType(typeof(GenericType<GenericNestedTypeContainer2<SimpleType, SimpleType2>.NestedType<SimpleType3, SimpleType4>.NestedType2>), $"!{Namespace}.GenericType%601[[{Namespace}.GenericNestedTypeContainer2%602+NestedType%602+NestedType2[[{Namespace}.SimpleType,{Namespace}],[{Namespace}.SimpleType2,{Namespace}],[{Namespace}.SimpleType3,{Namespace}],[{Namespace}.SimpleType4,{Namespace}]],{Namespace}]],{Namespace}");
            TestType(typeof(GenericType<GenericNestedTypeContainer2<int, string>.NestedType<Guid, DateTime>.NestedType2>), $"!{Namespace}.GenericType%601[[{Namespace}.GenericNestedTypeContainer2%602+NestedType%602+NestedType2[[System.Int32,mscorlib],[System.String,mscorlib],[System.Guid,mscorlib],[System.DateTime,mscorlib]],{Namespace}]],{Namespace}");
        }

        [NotNull]
        private static Serializer CreateSerializer()
        {
            var settings = new SerializerSettings();
            settings.RegisterAssembly(typeof(TagTests).Assembly);
            var serializer = new Serializer(settings);
            settings.AssemblyRegistry.UseShortTypeName = true;
            return serializer;
        }

        private static void TestType(Type type, string tag)
        {
            // NOTE: we're testing twice with different order because each method is caching result and discrepencies between the two methods could cause one of the order to fail
            var serializer = CreateSerializer();
            bool isAlias;
            var retrievedTag = serializer.Settings.TagTypeRegistry.TagFromType(type);
            Assert.AreEqual(tag, retrievedTag);
            var retrivedType = serializer.Settings.TagTypeRegistry.TypeFromTag(tag, out isAlias);
            Assert.AreEqual(type, retrivedType);

            serializer = CreateSerializer();
            retrivedType = serializer.Settings.TagTypeRegistry.TypeFromTag(tag, out isAlias);
            Assert.AreEqual(type, retrivedType);
            retrievedTag = serializer.Settings.TagTypeRegistry.TagFromType(type);
            Assert.AreEqual(tag, retrievedTag);
        }
    }
}
