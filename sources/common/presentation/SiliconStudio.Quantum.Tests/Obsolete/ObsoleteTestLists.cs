// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests.Obsolete
{
    [TestFixture(Ignore = "Obsolete")]
    public class ObsoleteTestLists
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

        public class ClassWithLists
        {
            public ClassWithLists()
            {
                IntList = new List<int> { 1, 2, 3 };
                ClassList = new List<SimpleClass> { new SimpleClass() };
                SimpleStructList = new List<SimpleStruct> { new SimpleStruct(), new SimpleStruct() };
                NestedStructList = new List<NestedStruct> { new NestedStruct(), new NestedStruct() };
                ListOfSimpleStructLists = new List<List<SimpleStruct>> { new List<SimpleStruct> { new SimpleStruct() }, new List<SimpleStruct> { new SimpleStruct() } };
                ListOfNestedStructLists = new List<List<NestedStruct>> { new List<NestedStruct> { new NestedStruct() }, new List<NestedStruct> { new NestedStruct() } };
            }

            [DataMember(1)]
            public List<int> IntList { get; }

            [DataMember(2)]
            public List<SimpleClass> ClassList { get; }

            [DataMember(3)]
            public List<SimpleStruct> SimpleStructList { get; }

            [DataMember(4)]
            public List<NestedStruct> NestedStructList { get; }

            [DataMember(5)]
            public List<List<SimpleStruct>> ListOfSimpleStructLists { get; }

            [DataMember(6)]
            public List<List<NestedStruct>> ListOfNestedStructLists { get; }
        }

        public class ClassWithNullLists
        {
            [DataMember(1)]
            public List<int> IntList { get; set; }

            [DataMember(2)]
            public List<SimpleClass> ClassList { get; set; }

            [DataMember(3)]
            public List<SimpleStruct> SimpleStructList { get; set; }

            [DataMember(4)]
            public List<NestedStruct> NestedStructList { get; set; }

            [DataMember(5)]
            public List<List<SimpleStruct>> ListOfSimpleStructLists { get; set; }

            [DataMember(6)]
            public List<List<NestedStruct>> ListOfNestedStructLists { get; set; }
        }

        #endregion Test class definitions

        [Test]
        public void TestConstruction()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);

            Assert.That(model.TryGetChild("IntList").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("IntList").Content.Value, Is.SameAs(obj.IntList));
            Assert.That(model.TryGetChild("IntList").Content.IsReference, Is.False);
            Assert.That(model.TryGetChild("ClassList").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("ClassList").Content.Value, Is.SameAs(obj.ClassList));
            Assert.That(model.TryGetChild("ClassList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.TryGetChild("SimpleStructList").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("SimpleStructList").Content.Value, Is.SameAs(obj.SimpleStructList));
            Assert.That(model.TryGetChild("SimpleStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.TryGetChild("NestedStructList").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("NestedStructList").Content.Value, Is.SameAs(obj.NestedStructList));
            Assert.That(model.TryGetChild("NestedStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.TryGetChild("ListOfSimpleStructLists").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("ListOfSimpleStructLists").Content.Value, Is.SameAs(obj.ListOfSimpleStructLists));
            Assert.That(model.TryGetChild("ListOfSimpleStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            foreach (var reference in model.TryGetChild("ListOfSimpleStructLists").Content.Reference.AsEnumerable)
            {
                Assert.That(reference, Is.AssignableFrom(typeof(ObjectReference)));
            }
            Assert.That(model.TryGetChild("ListOfNestedStructLists").Children.Count, Is.EqualTo(0));
            Assert.That(model.TryGetChild("ListOfNestedStructLists").Content.Value, Is.SameAs(obj.ListOfNestedStructLists));
            Assert.That(model.TryGetChild("ListOfNestedStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            foreach (var reference in model.TryGetChild("ListOfNestedStructLists").Content.Reference.AsEnumerable)
            {
                Assert.That(reference, Is.AssignableFrom(typeof(ObjectReference)));
            }

            Assert.That(container.GetNode(obj.ClassList[0]), !Is.Null);
            Assert.That(container.Guids.Count(), Is.EqualTo(18));
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestNullLists()
        {
            var obj = new ClassWithNullLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestPrimitiveItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Console.WriteLine(model.PrintHierarchy());
            ((List<int>)model.TryGetChild("IntList").Content.Value)[1] = 42;
            ((List<int>)model.TryGetChild("IntList").Content.Value).Add(26);
            Assert.That(obj.IntList.Count, Is.EqualTo(4));
            Assert.That(obj.IntList[1], Is.EqualTo(42));
            Assert.That(obj.IntList[3], Is.EqualTo(26));
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestObjectItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            var objRef = ((ReferenceEnumerable)model.TryGetChild("ClassList").Content.Reference).First();
            objRef.TargetNode.TryGetChild("SecondValue").Content.Update(32);
            Helper.PrintModelContainerContent(container, model);
            Assert.That(obj.ClassList[0].SecondValue, Is.EqualTo(32));
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestStructItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            var objRef = ((ReferenceEnumerable)model.TryGetChild("SimpleStructList").Content.Reference).First();
            objRef.TargetNode.TryGetChild("SecondValue").Content.Update(32);
            Helper.PrintModelContainerContent(container, model);
            Assert.That(obj.SimpleStructList[0].SecondValue, Is.EqualTo(32));
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestNestedStructItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            var objRef = ((ReferenceEnumerable)model.TryGetChild("NestedStructList").Content.Reference).First();
            var structNode = container.GetNode(((ObjectReference)objRef.TargetNode.TryGetChild("Struct").Content.Reference).TargetGuid);
            structNode.TryGetChild("SecondValue").Content.Update(32);
            Helper.PrintModelContainerContent(container, model);
            Assert.That(obj.NestedStructList[0].Struct.SecondValue, Is.EqualTo(32));
            //var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            //visitor.Check((GraphNode)model, obj, typeof(ClassWithLists), true);
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestListOfSimpleStructListsUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            var listRef = ((ReferenceEnumerable)model.TryGetChild("ListOfSimpleStructLists").Content.Reference).Last();
            var objRef = ((ReferenceEnumerable)listRef.TargetNode.Content.Reference).Last();
            objRef.TargetNode.TryGetChild("SecondValue").Content.Update(32);
            Helper.PrintModelContainerContent(container, model);
            Assert.That(obj.ListOfSimpleStructLists[1][0].SecondValue, Is.EqualTo(32));
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestListOfNestedStructListsUpdate()
        {
            var obj = new ClassWithLists();
            var container = new NodeContainer();
            IGraphNode model = container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, model);
            var listRef = ((ReferenceEnumerable)model.TryGetChild("ListOfNestedStructLists").Content.Reference).Last();
            var objRef = ((ReferenceEnumerable)listRef.TargetNode.Content.Reference).Last();
            var structNode = container.GetNode(((ObjectReference)objRef.TargetNode.TryGetChild("Struct").Content.Reference).TargetGuid);
            structNode.TryGetChild("SecondValue").Content.Update(32);
            Helper.PrintModelContainerContent(container, model);
            Assert.That(obj.ListOfNestedStructLists[1][0].Struct.SecondValue, Is.EqualTo(32));
            Helper.ConsistencyCheck(container, obj);
        }
    }
}
