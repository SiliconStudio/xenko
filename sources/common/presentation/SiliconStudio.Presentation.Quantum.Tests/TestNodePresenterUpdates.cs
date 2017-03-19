using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Presentation.Quantum.Presenters;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterUpdates
    {
        [Test]
        public void TestPrimitiveMemberUpdate()
        {
            var instance = new Types.SimpleType { String = "aaa" };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children.Single().UpdateValue("bbb");
            Assert.AreEqual(1, presenter.Children.Count);
            var child = presenter.Children.Single();
            Assert.AreEqual("String", child.Name);
            Assert.AreEqual(true, child.IsPrimitive);
            Assert.AreEqual(presenter, child.Parent);
            Assert.AreEqual(0, child.Children.Count);
            Assert.AreEqual(typeof(string), child.Type);
            Assert.AreEqual("bbb", child.Value);

            presenter.Children.Single().UpdateValue("ccc");
            Assert.AreEqual(1, presenter.Children.Count);
            child = presenter.Children.Single();
            Assert.AreEqual("String", child.Name);
            Assert.AreEqual(true, child.IsPrimitive);
            Assert.AreEqual(presenter, child.Parent);
            Assert.AreEqual(0, child.Children.Count);
            Assert.AreEqual(typeof(string), child.Type);
            Assert.AreEqual("ccc", child.Value);
        }

        [Test]
        public void TestReferenceMemberUpdate()
        {
            var instance = new Types.ClassWithRef { String = "aaa", Ref = new Types.ClassWithRef { String = "bbb" } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].UpdateValue(new Types.ClassWithRef { String = "ccc" });
            Assert.AreEqual(2, presenter.Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);

            presenter.Children[1].UpdateValue(new Types.ClassWithRef { String = "ddd" });
            Assert.AreEqual(2, presenter.Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            Assert.AreEqual("ddd", presenter.Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);
        }

        [Test]
        public void TestPrimitiveListUpdate()
        {
            var instance = new Types.ClassWithCollection { String = "aaa", List = { "bbb", "ccc" } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].UpdateItem("ddd", new Index(1));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[1].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].UpdateItem("eee", new Index(1));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].UpdateItem("fff", new Index(0));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("fff", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
        }

        [Test]
        public void TestPrimitiveListAdd()
        {
            var instance = new Types.ClassWithCollection { String = "aaa", List = { "bbb", "ccc" } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].AddItem("ddd", new Index(2));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(3, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[2].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);

            presenter.Children[1].AddItem("eee", new Index(1));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(4, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[2].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[3].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);

            presenter.Children[1].AddItem("fff", new Index(0));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(5, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("fff", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("bbb", presenter.Children[1].Children[1].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[2].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[3].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[4].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[4].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);
            Assert.AreEqual(instance.List[4], presenter.Children[1].Children[4].Value);
        }

        [Test]
        public void TestPrimitiveListRemove()
        {
            var instance = new Types.ClassWithCollection { String = "aaa", List = { "bbb", "ccc", "ddd", "eee", "fff" } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].RemoveItem("fff", new Index(4));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(4, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[2].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[3].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);

            presenter.Children[1].RemoveItem("bbb", new Index(0));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(3, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[1].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[2].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);

            presenter.Children[1].RemoveItem("ddd", new Index(1));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].RemoveItem("eee", new Index(1));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(1, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);

            presenter.Children[1].RemoveItem("ccc", new Index(0));
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(0, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
        }

        [Test]
        public void TestReferenceListUpdate()
        {
            var instance = new Types.ClassWithRefCollection { String = "aaa", List = { new Types.SimpleType { String = "bbb" }, new Types.SimpleType { String = "ccc" } } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].UpdateItem(new Types.SimpleType { String = "ddd" }, new Index(1));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].UpdateItem(new Types.SimpleType { String = "eee" }, new Index(1));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].UpdateItem(new Types.SimpleType { String = "fff" }, new Index(0));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("fff", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
        }

        [Test]
        public void TestReferenceListAdd()
        {
            var instance = new Types.ClassWithRefCollection { String = "aaa", List = { new Types.SimpleType { String = "bbb" }, new Types.SimpleType { String = "ccc" } } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].AddItem(new Types.SimpleType { String = "ddd" }, new Index(2));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(3, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[2].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);

            presenter.Children[1].AddItem(new Types.SimpleType { String = "eee" }, new Index(1));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(4, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[2].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[3].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);

            presenter.Children[1].AddItem(new Types.SimpleType { String = "fff" }, new Index(0));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            Assert.AreEqual(5, presenter.Children[1].Children.Count);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("fff", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("bbb", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[2].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[3].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[4].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[4].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[4].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);
            Assert.AreEqual(instance.List[4], presenter.Children[1].Children[4].Value);
        }

        [Test]
        public void TestReferenceListRemove()
        {
            var instance = new Types.ClassWithRefCollection { String = "aaa", List = { new Types.SimpleType { String = "bbb" }, new Types.SimpleType { String = "ccc" }, new Types.SimpleType { String = "ddd" }, new Types.SimpleType { String = "eee" }, new Types.SimpleType { String = "fff" }, } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));

            presenter.Children[1].RemoveItem(instance.List[4], new Index(4));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(4, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[2].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[3].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[3].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[3].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);
            Assert.AreEqual(instance.List[3], presenter.Children[1].Children[3].Value);

            presenter.Children[1].RemoveItem("bbb", new Index(0));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(3, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("ddd", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[2].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[2].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[2].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
            Assert.AreEqual(instance.List[2], presenter.Children[1].Children[2].Value);

            presenter.Children[1].RemoveItem("ddd", new Index(1));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual("eee", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);

            presenter.Children[1].RemoveItem("eee", new Index(1));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(1, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[0].Children[0].Value);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Children[0].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);

            presenter.Children[1].RemoveItem("ccc", new Index(0));
            Assert.AreEqual(typeof(List<Types.SimpleType>), presenter.Children[1].Type);
            Assert.AreEqual(0, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
        }

        private static IObjectNode BuildQuantumGraph(object instance)
        {
            var container = new NodeContainer();
            var node = container.GetOrCreateNode(instance);
            return node;
        }
    }
}
