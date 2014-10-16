// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Core;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestLists
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
            public List<int> IntList { get; private set; }

            [DataMember(2)]
            public List<SimpleClass> ClassList { get; private set; }

            [DataMember(3)]
            public List<SimpleStruct> SimpleStructList { get; private set; }

            [DataMember(4)]
            public List<NestedStruct> NestedStructList { get; private set; }

            [DataMember(5)]
            public List<List<SimpleStruct>> ListOfSimpleStructLists { get; private set; }

            [DataMember(6)]
            public List<List<NestedStruct>> ListOfNestedStructLists { get; private set; }
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
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            foreach (var guid in container.Guids)
            {
                var node = container.GetModelNode(guid);
                if (model != node)
                    Console.WriteLine(node.PrintHierarchy());
            }

            Assert.That(model.GetChild("IntList").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("IntList").Content.Value, Is.SameAs(obj.IntList));
            Assert.That(model.GetChild("IntList").Content.IsReference, Is.False);
            Assert.That(model.GetChild("ClassList").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("ClassList").Content.Value, Is.SameAs(obj.ClassList));
            Assert.That(model.GetChild("ClassList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.GetChild("SimpleStructList").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("SimpleStructList").Content.Value, Is.SameAs(obj.SimpleStructList));
            Assert.That(model.GetChild("SimpleStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.GetChild("NestedStructList").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("NestedStructList").Content.Value, Is.SameAs(obj.NestedStructList));
            Assert.That(model.GetChild("NestedStructList").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            Assert.That(model.GetChild("ListOfSimpleStructLists").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("ListOfSimpleStructLists").Content.Value, Is.SameAs(obj.ListOfSimpleStructLists));
            Assert.That(model.GetChild("ListOfSimpleStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            foreach (var reference in (ReferenceEnumerable)model.GetChild("ListOfSimpleStructLists").Content.Reference)
            {
                Assert.That(reference, Is.AssignableFrom(typeof(ObjectReference)));
            }
            Assert.That(model.GetChild("ListOfNestedStructLists").Children.Count, Is.EqualTo(0));
            Assert.That(model.GetChild("ListOfNestedStructLists").Content.Value, Is.SameAs(obj.ListOfNestedStructLists));
            Assert.That(model.GetChild("ListOfNestedStructLists").Content.Reference, Is.AssignableFrom(typeof(ReferenceEnumerable)));
            foreach (var reference in (ReferenceEnumerable)model.GetChild("ListOfNestedStructLists").Content.Reference)
            {
                Assert.That(reference, Is.AssignableFrom(typeof(ObjectReference)));
            }

            Assert.That(container.GetModelNode(obj.ClassList[0]), !Is.Null);
            Assert.That(container.Guids.Count(), Is.EqualTo(14));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestNullLists()
        {
            var obj = new ClassWithNullLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            foreach (var node in container.Guids.Select(container.GetModelNode).Where(x => model != x))
            {
                Console.WriteLine(node.PrintHierarchy());
            }
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithNullLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestPrimitiveItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            ((List<int>)model.GetChild("IntList").Content.Value)[1] = 42;
            ((List<int>)model.GetChild("IntList").Content.Value).Add(26);
            Assert.That(obj.IntList.Count, Is.EqualTo(4));
            Assert.That(obj.IntList[1], Is.EqualTo(42));
            Assert.That(obj.IntList[3], Is.EqualTo(26));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestObjectItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            var objRef = ((ReferenceEnumerable)model.GetChild("ClassList").Content.Reference).First();
            objRef.TargetNode.GetChild("SecondValue").Content.Value = 32;
            Assert.That(obj.ClassList[0].SecondValue, Is.EqualTo(32));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestStructItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            var objRef = ((ReferenceEnumerable)model.GetChild("SimpleStructList").Content.Reference).First();
            objRef.TargetNode.GetChild("SecondValue").Content.Value = 32;
            Assert.That(obj.SimpleStructList[0].SecondValue, Is.EqualTo(32));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestNestedStructItemUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            var objRef = ((ReferenceEnumerable)model.GetChild("NestedStructList").Content.Reference).First();
            objRef.TargetNode.GetChild("Struct").GetChild("SecondValue").Content.Value = 32;
            Assert.That(obj.NestedStructList[0].Struct.SecondValue, Is.EqualTo(32));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestListOfSimpleStructListsUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            var listRef = ((ReferenceEnumerable)model.GetChild("ListOfSimpleStructLists").Content.Reference).Last();
            var objRef = ((ReferenceEnumerable)listRef.TargetNode.Content.Reference).Last();
            objRef.TargetNode.GetChild("SecondValue").Content.Value = 32;
            Assert.That(obj.ListOfSimpleStructLists[1][0].SecondValue, Is.EqualTo(32));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }

        [Test]
        public void TestListOfNestedStructListsUpdate()
        {
            var obj = new ClassWithLists();
            var container = new ModelContainer();
            IModelNode model = container.GetOrCreateModelNode(obj, obj.GetType());
            Console.WriteLine(model.PrintHierarchy());
            var listRef = ((ReferenceEnumerable)model.GetChild("ListOfNestedStructLists").Content.Reference).Last();
            var objRef = ((ReferenceEnumerable)listRef.TargetNode.Content.Reference).Last();
            objRef.TargetNode.GetChild("Struct").GetChild("SecondValue").Content.Value = 32;
            Assert.That(obj.ListOfNestedStructLists[1][0].Struct.SecondValue, Is.EqualTo(32));
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            visitor.Check((ModelNode)model, obj, typeof(ClassWithLists), true);
            foreach (var node in container.Models)
            {
                visitor.Check((ModelNode)node, node.Content.Value, node.Content.Type, true);
            }
        }
    }
}
