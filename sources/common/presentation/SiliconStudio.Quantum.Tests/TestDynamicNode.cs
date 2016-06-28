using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestDynamicNode
    {
        // TODO: test enumeration with the three cases (reference, primitive collection, primitive dictionary)
        public class SimpleClass
        {
            public int Member1;
            public SimpleClass Member2;
        }

        public class ComplexClass
        {
            public int Member1;
            public SimpleClass Member2;
            public object Member3;
            public Struct Member4;
            public List<string> Member5;
            public List<SimpleClass> Member6;
            public List<Struct> Member7;
        }

        public struct Struct
        {
            public string Member1;
            public SimpleClass Member2;
        }

        [Test]
        public void TestChangePrimitiveMember()
        {
            var nodeContainer = new NodeContainer();
            var instance = new ComplexClass { Member1 = 3 };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member1, (int)dynNode.Member1);
            dynNode.Member1 = 4;
            Assert.AreEqual(4, instance.Member1);
            Assert.AreEqual(instance.Member1, (int)dynNode.Member1);
            rootNode.GetChild(nameof(ComplexClass.Member1)).Content.Update(5);
            Assert.AreEqual(5, instance.Member1);
            Assert.AreEqual(instance.Member1, (int)dynNode.Member1);
        }

        [Test]
        public void TestChangeReferenceMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member2 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
            dynNode.Member2 = obj[1];
            Assert.AreEqual(obj[1], instance.Member2);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
            rootNode.GetChild(nameof(ComplexClass.Member2)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], instance.Member2);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
        }

        [Test]
        public void TestChangeReferenceMemberToNull()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), null, new SimpleClass() };
            var instance = new ComplexClass { Member2 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
            dynNode.Member2 = obj[1];
            Assert.AreEqual(obj[1], instance.Member2);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
            rootNode.GetChild(nameof(ComplexClass.Member2)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], instance.Member2);
            Assert.AreEqual(instance.Member2, (SimpleClass)dynNode.Member2);
        }

        [Test]
        public void TestChangeBoxedPrimitiveMember()
        {
            var nodeContainer = new NodeContainer();
            var instance = new ComplexClass { Member3 = 3 };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member3, (int)dynNode.Member3);
            dynNode.Member3 = 4;
            Assert.AreEqual(4, instance.Member3);
            Assert.AreEqual(instance.Member3, (int)dynNode.Member3);
            rootNode.GetChild(nameof(ComplexClass.Member3)).Content.Update(5);
            Assert.AreEqual(5, instance.Member3);
            Assert.AreEqual(instance.Member3, (int)dynNode.Member3);
        }

        [Test]
        public void TestChangeReferenceInObjectMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member3 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member3, (SimpleClass)dynNode.Member3);
            dynNode.Member3 = obj[1];
            Assert.AreEqual(obj[1], instance.Member3);
            Assert.AreEqual(instance.Member3, (SimpleClass)dynNode.Member3);
            rootNode.GetChild(nameof(ComplexClass.Member3)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], instance.Member3);
            Assert.AreEqual(instance.Member3, (SimpleClass)dynNode.Member3);
        }

        [Test]
        public void TestChangeStruct()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member4 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
            Assert.AreEqual(obj[0].Member1, (string)dynNode.Member4.Member1);
            dynNode.Member4 = obj[1];
            Assert.AreEqual(obj[1], instance.Member4);
            Assert.AreEqual(obj[1].Member1, (string)dynNode.Member4.Member1);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
            rootNode.GetChild(nameof(ComplexClass.Member4)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], instance.Member4);
            Assert.AreEqual(obj[2].Member1, (string)dynNode.Member4.Member1);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
        }

        [Test]
        public void TestChangeStructMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member4 = new Struct { Member1 = obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
            Assert.AreEqual(obj[0], (string)dynNode.Member4.Member1);
            dynNode.Member4.Member1 = obj[1];
            Assert.AreEqual(obj[1], (string)dynNode.Member4.Member1);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
            rootNode.GetChild(nameof(ComplexClass.Member4)).GetChild(nameof(Struct.Member1)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], (string)dynNode.Member4.Member1);
            Assert.AreEqual(instance.Member4, (Struct)dynNode.Member4);
        }

        [Test]
        public void TestChangePrimitiveList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<string> { "aa" }, new List<string> { "bb" }, new List<string> { "cc" } };
            var instance = new ComplexClass { Member5 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member5, (List<string>)dynNode.Member5);
            Assert.AreEqual(obj[0], (List<string>)dynNode.Member5);
            dynNode.Member5 = obj[1];
            Assert.AreEqual(instance.Member5, (List<string>)dynNode.Member5);
            Assert.AreEqual(obj[1], (List<string>)dynNode.Member5);
            rootNode.GetChild(nameof(ComplexClass.Member5)).Content.Update(obj[2]);
            Assert.AreEqual(instance.Member5, (List<string>)dynNode.Member5);
            Assert.AreEqual(obj[2], (List<string>)dynNode.Member5);
        }

        [Test]
        public void TestChangePrimitiveListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member5 = new List<string> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
            dynNode.Member5[0] = obj[1];
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[1], (string)dynNode.Member5[0]);
            rootNode.GetChild(nameof(ComplexClass.Member5)).Content.Update(obj[2], new Index(0));
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[2], (string)dynNode.Member5[0]);
        }

        [Test]
        public void TestAddPrimitiveListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member5 = new List<string> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
            dynNode.Member5.Add(obj[1]);
            Assert.AreEqual(instance.Member5[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(obj[1], (string)dynNode.Member5[1]);
            rootNode.GetChild(nameof(ComplexClass.Member5)).Content.Add(obj[2], new Index(2));
            Assert.AreEqual(instance.Member5[2], (string)dynNode.Member5[2]);
            Assert.AreEqual(obj[2], (string)dynNode.Member5[2]);
        }

        [Test]
        public void TestInsertPrimitiveListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member5 = new List<string> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
            dynNode.Member5.Insert(obj[1], new Index(0));
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(instance.Member5[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(obj[1], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[1]);
            rootNode.GetChild(nameof(ComplexClass.Member5)).Content.Add(obj[2], new Index(1));
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(instance.Member5[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(instance.Member5[2], (string)dynNode.Member5[2]);
            Assert.AreEqual(obj[1], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[2], (string)dynNode.Member5[1]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[2]);
        }

        [Test]
        public void TestRemovePrimitiveListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member5 = new List<string> { obj[0], obj[1], obj[2] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(instance.Member5[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(instance.Member5[2], (string)dynNode.Member5[2]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(obj[2], (string)dynNode.Member5[2]);
            dynNode.Member5.Remove(obj[1], new Index(1));
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(instance.Member5[1], (string)dynNode.Member5[1]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[2], (string)dynNode.Member5[1]);
            rootNode.GetChild(nameof(ComplexClass.Member5)).Content.Remove(obj[2], new Index(1));
            Assert.AreEqual(instance.Member5[0], (string)dynNode.Member5[0]);
            Assert.AreEqual(obj[0], (string)dynNode.Member5[0]);
        }

        [Test]
        public void TestChangeReferenceList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<SimpleClass> { new SimpleClass() }, new List<SimpleClass> { new SimpleClass() }, new List<SimpleClass> { new SimpleClass() } };
            var instance = new ComplexClass { Member6 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6, (List<SimpleClass>)dynNode.Member6);
            Assert.AreEqual(obj[0], (List<SimpleClass>)dynNode.Member6);
            dynNode.Member6 = obj[1];
            Assert.AreEqual(instance.Member6, (List<SimpleClass>)dynNode.Member6);
            Assert.AreEqual(obj[1], (List<SimpleClass>)dynNode.Member6);
            rootNode.GetChild(nameof(ComplexClass.Member6)).Content.Update(obj[2]);
            Assert.AreEqual(instance.Member6, (List<SimpleClass>)dynNode.Member6);
            Assert.AreEqual(obj[2], (List<SimpleClass>)dynNode.Member6);
        }

        [Test]
        public void TestChangeReferenceListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
            dynNode.Member6[0] = obj[1];
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[1], (SimpleClass)dynNode.Member6[0]);
            rootNode.GetChild(nameof(ComplexClass.Member6)).Content.Update(obj[2], new Index(0));
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[2], (SimpleClass)dynNode.Member6[0]);
        }

        [Test]
        public void TestAddReferenceListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
            dynNode.Member6.Add(obj[1]);
            Assert.AreEqual(instance.Member6[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(obj[1], (SimpleClass)dynNode.Member6[1]);
            rootNode.GetChild(nameof(ComplexClass.Member6)).Content.Add(obj[2], new Index(2));
            Assert.AreEqual(instance.Member6[2], (SimpleClass)dynNode.Member6[2]);
            Assert.AreEqual(obj[2], (SimpleClass)dynNode.Member6[2]);
        }

        [Test]
        public void TestInsertReferenceListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
            dynNode.Member6.Insert(obj[1], new Index(0));
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(instance.Member6[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(obj[1], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[1]);
            rootNode.GetChild(nameof(ComplexClass.Member6)).Content.Add(obj[2], new Index(1));
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(instance.Member6[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(instance.Member6[2], (SimpleClass)dynNode.Member6[2]);
            Assert.AreEqual(obj[1], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[2], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[2]);
        }

        [Test]
        public void TestRemoveReferenceListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { obj[0], obj[1], obj[2] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(instance.Member6[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(instance.Member6[2], (SimpleClass)dynNode.Member6[2]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(obj[2], (SimpleClass)dynNode.Member6[2]);
            dynNode.Member6.Remove(obj[1], new Index(1));
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(instance.Member6[1], (SimpleClass)dynNode.Member6[1]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[2], (SimpleClass)dynNode.Member6[1]);
            rootNode.GetChild(nameof(ComplexClass.Member6)).Content.Remove(obj[2], new Index(1));
            Assert.AreEqual(instance.Member6[0], (SimpleClass)dynNode.Member6[0]);
            Assert.AreEqual(obj[0], (SimpleClass)dynNode.Member6[0]);
        }

        [Test]
        public void TestChangeReferenceListItemMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { 3, 4, 5 };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { new SimpleClass(), new SimpleClass { Member1 = obj[0] } } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member6[1].Member1, (int)dynNode.Member6[1].Member1);
            Assert.AreEqual(obj[0], (int)dynNode.Member6[1].Member1);
            dynNode.Member6[1].Member1 = obj[1];
            Assert.AreEqual(obj[1], (int)dynNode.Member6[1].Member1);
            Assert.AreEqual(instance.Member6[1].Member1, (int)dynNode.Member6[1].Member1);
            rootNode.GetChild(nameof(ComplexClass.Member6)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], (int)dynNode.Member6[1].Member1);
            Assert.AreEqual(instance.Member6[1].Member1, (int)dynNode.Member6[1].Member1);
        }

        [Test]
        public void TestChangeStructList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<Struct> { new Struct() }, new List<Struct> { new Struct() }, new List<Struct> { new Struct() } };
            var instance = new ComplexClass { Member7 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7, (List<Struct>)dynNode.Member7);
            Assert.AreEqual(obj[0], (List<Struct>)dynNode.Member7);
            dynNode.Member7 = obj[1];
            Assert.AreEqual(instance.Member7, (List<Struct>)dynNode.Member7);
            Assert.AreEqual(obj[1], (List<Struct>)dynNode.Member7);
            rootNode.GetChild(nameof(ComplexClass.Member7)).Content.Update(obj[2]);
            Assert.AreEqual(instance.Member7, (List<Struct>)dynNode.Member7);
            Assert.AreEqual(obj[2], (List<Struct>)dynNode.Member7);
        }

        [Test]
        public void TestChangeStructListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member7 = new List<Struct> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
            dynNode.Member7[0] = obj[1];
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[1], (Struct)dynNode.Member7[0]);
            rootNode.GetChild(nameof(ComplexClass.Member7)).Content.Update(obj[2], new Index(0));
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[2], (Struct)dynNode.Member7[0]);
        }

        [Test]
        public void TestAddStructListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member7 = new List<Struct> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
            dynNode.Member7.Add(obj[1]);
            Assert.AreEqual(instance.Member7[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(obj[1], (Struct)dynNode.Member7[1]);
            rootNode.GetChild(nameof(ComplexClass.Member7)).Content.Add(obj[2], new Index(2));
            Assert.AreEqual(instance.Member7[2], (Struct)dynNode.Member7[2]);
            Assert.AreEqual(obj[2], (Struct)dynNode.Member7[2]);
        }

        [Test]
        public void TestInsertStructListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member7 = new List<Struct> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
            dynNode.Member7.Insert(obj[1], new Index(0));
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(instance.Member7[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(obj[1], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[1]);
            rootNode.GetChild(nameof(ComplexClass.Member7)).Content.Add(obj[2], new Index(1));
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(instance.Member7[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(instance.Member7[2], (Struct)dynNode.Member7[2]);
            Assert.AreEqual(obj[1], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[2], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[2]);
        }

        [Test]
        public void TestRemoveStructListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member7 = new List<Struct> { obj[0], obj[1], obj[2] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(instance.Member7[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(instance.Member7[2], (Struct)dynNode.Member7[2]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(obj[2], (Struct)dynNode.Member7[2]);
            dynNode.Member7.Remove(obj[1], new Index(1));
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(instance.Member7[1], (Struct)dynNode.Member7[1]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[2], (Struct)dynNode.Member7[1]);
            rootNode.GetChild(nameof(ComplexClass.Member7)).Content.Remove(obj[2], new Index(1));
            Assert.AreEqual(instance.Member7[0], (Struct)dynNode.Member7[0]);
            Assert.AreEqual(obj[0], (Struct)dynNode.Member7[0]);
        }

        [Test]
        public void TestChangeStructListItemMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member7 = new List<Struct> { new Struct(), new Struct { Member1 = obj[0] } } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var dynNode = DynamicNode.FromNode(rootNode);
            Assert.AreEqual(instance.Member7[1].Member1, (string)dynNode.Member7[1].Member1);
            Assert.AreEqual(obj[0], (string)dynNode.Member7[1].Member1);
            dynNode.Member7[1].Member1 = obj[1];
            Assert.AreEqual(obj[1], (string)dynNode.Member7[1].Member1);
            Assert.AreEqual(instance.Member7[1].Member1, (string)dynNode.Member7[1].Member1);
            rootNode.GetChild(nameof(ComplexClass.Member7)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1)).Content.Update(obj[2]);
            Assert.AreEqual(obj[2], (string)dynNode.Member7[1].Member1);
            Assert.AreEqual(instance.Member7[1].Member1, (string)dynNode.Member7[1].Member1);
        }
    }
}
