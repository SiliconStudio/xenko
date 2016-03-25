using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestReferences
    {
        public class TestObject
        {
            [DataMember]
            public string Name;
        }

        public class ObjectContainer
        {
            [DataMember]
            public object Instance { get; set; }
        }

        public class MultipleObjectContainer
        {
            [DataMember]
            public List<TestObject> Instances { get; set; } = new List<TestObject>();
        }

        /// <summary>
        /// This test creates two objects, one referencing the other. It verifies that when constructing the node of the referenced object first,
        /// the referencer object will reuse the same node as target of the reference
        /// </summary>
        [Test]
        public void TestUseExistingNodeAsReference()
        {
            var nodeContainer = new NodeContainer();
            var instance = new TestObject { Name = "Test" };
            var container = new ObjectContainer { Instance = instance };
            var instanceNode = nodeContainer.GetOrCreateNode(instance);
            var containerNode = nodeContainer.GetOrCreateNode(container);
            Assert.AreEqual(1, containerNode.Children.Count);
            var memberNode = containerNode.Children.First();
            Assert.AreEqual(instance, memberNode.Content.Retrieve());
            Assert.AreEqual(true, memberNode.Content.IsReference);
            Assert.AreEqual(instanceNode, memberNode.Content.Reference.AsObject.TargetNode);
        }

        /// <summary>
        /// This test creates two objects, one referencing the other. It verifies that when constructing the node of the referencer object first,
        /// the referenced object will reuse the same node as target of the reference
        /// </summary>
        [Test]
        public void TestUseExistingReferenceAsNode()
        {
            var nodeContainer = new NodeContainer();
            var instance = new TestObject { Name = "Test" };
            var container = new ObjectContainer { Instance = instance };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var instanceNode = nodeContainer.GetOrCreateNode(instance);
            Assert.AreEqual(1, containerNode.Children.Count);
            var memberNode = containerNode.Children.First();
            Assert.AreEqual(instance, memberNode.Content.Retrieve());
            Assert.AreEqual(true, memberNode.Content.IsReference);
            Assert.AreEqual(instanceNode, memberNode.Content.Reference.AsObject.TargetNode);
        }

        /// <summary>
        /// This test a container object that reference multiples other object. It verifies that the same nodes are reused between the instances
        /// of objects and the references
        /// </summary>
        [Test]
        public void TestUseExistingNodesAsReference()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new TestObject { Name = "Test1" };
            var instance2 = new TestObject { Name = "Test2" };
            var container = new MultipleObjectContainer { Instances = { instance1, instance2 } };
            var instance1Node = nodeContainer.GetOrCreateNode(instance1);
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var instance2Node = nodeContainer.GetOrCreateNode(instance2);
            Assert.AreEqual(1, containerNode.Children.Count);
            var memberNode = containerNode.Children.First();
            Assert.AreEqual(true, memberNode.Content.IsReference);
            var reference = memberNode.Content.Reference.AsEnumerable;
            Assert.AreEqual(2, reference.Indices.Count());
            var reference1 = reference.First();
            var reference2 = reference.Last();
            Assert.AreEqual(instance1Node, reference1.TargetNode);
            Assert.AreEqual(instance2Node, reference2.TargetNode);
        }

        [Test]
        public void TestCircularReferences()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new ObjectContainer();
            var instance2 = new ObjectContainer();
            instance1.Instance = instance2;
            instance2.Instance = instance1;
            var instance1Node = nodeContainer.GetOrCreateNode(instance1);
            var instance2Node = nodeContainer.GetOrCreateNode(instance2);
            Assert.AreEqual(1, instance1Node.Children.Count);
            Assert.AreEqual(1, instance2Node.Children.Count);
            Assert.AreEqual(instance1Node.Children.First().Content.Reference.AsObject.TargetNode, instance2Node);
            Assert.AreEqual(instance2Node.Children.First().Content.Reference.AsObject.TargetNode, instance1Node);
        }
    }
}
