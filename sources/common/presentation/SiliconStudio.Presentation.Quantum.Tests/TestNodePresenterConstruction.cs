// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Presentation.Quantum.Tests.Helpers;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterConstruction
    {
        [Test]
        public void TestSimpleType()
        {
            var instance = new Types.SimpleType { String = "aaa" };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(false, presenter.IsPrimitive);
            Assert.Null(presenter.Parent);
            Assert.AreEqual(1, presenter.Children.Count);
            Assert.AreEqual(typeof(Types.SimpleType), presenter.Type);
            Assert.AreEqual(instance, presenter.Value);
            var child = presenter.Children.Single();
            Assert.AreEqual("String", child.Name);
            Assert.AreEqual(true, child.IsPrimitive);
            Assert.AreEqual(presenter, child.Parent);
            Assert.AreEqual(0, child.Children.Count);
            Assert.AreEqual(typeof(string), child.Type);
            Assert.AreEqual("aaa", child.Value);
        }

        [Test]
        public void TestClassWithRef()
        {
            var instance = new Types.ClassWithRef { String = "aaa", Ref = new Types.ClassWithRef { String = "bbb", Ref = new Types.ClassWithRef { String = "ccc" } } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(false, presenter.IsPrimitive);
            Assert.Null(presenter.Parent);
            Assert.AreEqual(2, presenter.Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Type);
            Assert.AreEqual(instance, presenter.Value);
            Assert.AreEqual("String", presenter.Children[0].Name);
            Assert.AreEqual("aaa", presenter.Children[0].Value);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);

            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);

            Assert.AreEqual(2, presenter.Children[1].Children[1].Children.Count);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[1].Children[0].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(Types.ClassWithRef), presenter.Children[1].Children[1].Children[1].Type);
        }

        [Test]
        public void TestClassWithPrimitiveCollection()
        {
            var instance = new Types.ClassWithCollection { String = "aaa", List = { "bbb", "ccc" } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeHierarchy(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(false, presenter.IsPrimitive);
            Assert.Null(presenter.Parent);
            Assert.AreEqual(2, presenter.Children.Count);
            Assert.AreEqual(typeof(Types.ClassWithCollection), presenter.Type);
            Assert.AreEqual(instance, presenter.Value);
            Assert.AreEqual("String", presenter.Children[0].Name);
            Assert.AreEqual("aaa", presenter.Children[0].Value);
            Assert.AreEqual(typeof(List<string>), presenter.Children[1].Type);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);

            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(instance.List, presenter.Children[1].Value);
            // TODO: Name is undefined yet
            //Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            //Assert.AreEqual("String", presenter.Children[1].Children[1].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Value);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[0].Type);
            Assert.AreEqual(typeof(string), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.List[0], presenter.Children[1].Children[0].Value);
            Assert.AreEqual(instance.List[1], presenter.Children[1].Children[1].Value);
        }

        private static IObjectNode BuildQuantumGraph(object instance)
        {
            var container = new NodeContainer();
            var node = container.GetOrCreateNode(instance);
            return node;
        }
    }
}
