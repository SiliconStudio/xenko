using System.Linq;
using NUnit.Framework;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterUpdates
    {
        [Test]
        public void TestPrimitiveMember()
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
        public void TestReferenceMember()
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


        private static IObjectNode BuildQuantumGraph(object instance)
        {
            var container = new NodeContainer();
            var node = container.GetOrCreateNode(instance);
            return node;
        }
    }
}
