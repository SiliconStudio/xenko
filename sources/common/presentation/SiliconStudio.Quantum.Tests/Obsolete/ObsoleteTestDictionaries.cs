// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests.Obsolete
{
    [TestFixture(Ignore = "Obsolete")]
    class ObsoleteTestDictionaries
    {
        #region Test class definitions
        public class SimpleClass
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public struct SimpleStruct
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public struct NestedStruct
        {
            [DataMember(1)]
            public SimpleStruct Struct { get; set; }
        }

        public class ClassWithDictionaries
        {
            public ClassWithDictionaries()
            {
                StringIntDic = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
                StringClassDic = new Dictionary<string, SimpleClass> { { "a", new SimpleClass() }, { "b", new SimpleClass() } };
                // TODO: test with primitive struct
                // TODO: test with nested struct
                //SimpleStructList = new List<SimpleStruct> { new SimpleStruct(), new SimpleStruct() };
                //NestedStructList = new List<NestedStruct> { new NestedStruct(), new NestedStruct() };
                //ListOfSimpleStructLists = new List<List<SimpleStruct>> { new List<SimpleStruct> { new SimpleStruct() }, new List<SimpleStruct> { new SimpleStruct() } };
                //ListOfNestedStructLists = new List<List<NestedStruct>> { new List<NestedStruct> { new NestedStruct() }, new List<NestedStruct> { new NestedStruct() } };
            }

            [DataMember(1)]
            public Dictionary<string, int> StringIntDic { get; private set; }

            [DataMember(2)]
            public Dictionary<string, SimpleClass> StringClassDic { get; private set; }

            //[DataMember(3)]
            //public List<SimpleStruct> SimpleStructList { get; private set; }

            //[DataMember(4)]
            //public List<NestedStruct> NestedStructList { get; private set; }

            //[DataMember(5)]
            //public List<List<SimpleStruct>> ListOfSimpleStructLists { get; private set; }

            //[DataMember(6)]
            //public List<List<NestedStruct>> ListOfNestedStructLists { get; private set; }
        }
        #endregion Test class definitions

        [Test]
        public void TestConstruction()
        {
            var obj = new ClassWithDictionaries();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);

            Assert.That(model.GetChild("StringIntDic").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("StringIntDic").Content.Value, Is.SameAs(obj.StringIntDic));
            Assert.That(model.GetChild("StringIntDic").Content.IsReference, Is.False);
            Assert.That(model.GetChild("StringClassDic").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("StringClassDic").Content.Value, Is.SameAs(obj.StringClassDic));
            Assert.That(model.GetChild("StringClassDic").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            var enumerator = obj.StringClassDic.GetEnumerator();
            foreach (var reference in model.GetChild("StringClassDic").Content.Reference.AsEnumerable)
            {
                enumerator.MoveNext();
                var keyValuePair = enumerator.Current;
                Assert.That(reference.Index, Is.EqualTo(keyValuePair.Key));
                Assert.That(reference.ObjectValue, Is.EqualTo(keyValuePair.Value));
            }
            //Assert.That(model.GetChild("SimpleStructList").Children.Count, Is.EqualTo(0));
            //Assert.That(model.GetChild("SimpleStructList").Content.Value, Is.SameAs(obj.SimpleStructList));
            //Assert.That(model.GetChild("SimpleStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //Assert.That(model.GetChild("NestedStructList").Children.Count, Is.EqualTo(0));
            //Assert.That(model.GetChild("NestedStructList").Content.Value, Is.SameAs(obj.NestedStructList));
            //Assert.That(model.GetChild("NestedStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //Assert.That(model.GetChild("ListOfSimpleStructLists").Children.Count, Is.EqualTo(0));
            //Assert.That(model.GetChild("ListOfSimpleStructLists").Content.Value, Is.SameAs(obj.ListOfSimpleStructLists));
            //Assert.That(model.GetChild("ListOfSimpleStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //foreach (var reference in (ReferenceEnumerable)model.GetChild("ListOfSimpleStructLists").Content.Reference)
            //{
            //    Assert.That(reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //}
            //Assert.That(model.GetChild("ListOfNestedStructLists").Children.Count, Is.EqualTo(0));
            //Assert.That(model.GetChild("ListOfNestedStructLists").Content.Value, Is.SameAs(obj.ListOfNestedStructLists));
            //Assert.That(model.GetChild("ListOfNestedStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //foreach (var reference in (ReferenceEnumerable)model.GetChild("ListOfNestedStructLists").Content.Reference)
            //{
            //    Assert.That(reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            //}

            //Assert.That(container.GetNode(obj.ClassList[0]), !Is.Null);
            //Assert.That(container.Guids.Count(), Is.EqualTo(10));
        }

        [Test]
        public void TestPrimitiveItemUpdate()
        {
            var obj = new ClassWithDictionaries();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            ((Dictionary<string, int>)model.GetChild("StringIntDic").Content.Value)["b"] = 42;
            ((Dictionary<string, int>)model.GetChild("StringIntDic").Content.Value).Add("d", 26);
            Assert.That(obj.StringIntDic.Count, Is.EqualTo(4));
            Assert.That(obj.StringIntDic["b"], Is.EqualTo(42));
            Assert.That(obj.StringIntDic["d"], Is.EqualTo(26));
            Helper.PrintModelContainerContent(container, model);
            Helper.ConsistencyCheck(container, obj);
        }

    }
}
