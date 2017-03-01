// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Presentation.Quantum.Tests
{
    [TestFixture]
    public class TestNodePresenterConstruction
    {
        public class SimpleType
        {
            public string String { get; set; }
        }

        public class ClassWithRef
        {
            [Display(1)]
            public string String { get; set; }
            [Display(2)]
            public ClassWithRef Ref { get; set; }
        }

        [Test]
        public void TestSimpleType()
        {
            var instance = new SimpleType { String = "aaa" };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeTree(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(false, presenter.IsPrimitive);
            Assert.Null(presenter.Parent);
            Assert.AreEqual(1, presenter.Children.Count);
            Assert.AreEqual(typeof(SimpleType), presenter.Type);
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
            var instance = new ClassWithRef { String = "aaa", Ref = new ClassWithRef { String = "bbb", Ref = new ClassWithRef { String = "ccc" } } };
            var rootNode = BuildQuantumGraph(instance);
            var factory = new NodePresenterFactory();
            var presenter = factory.CreateNodeTree(rootNode, new GraphNodePath(rootNode));
            Assert.AreEqual(false, presenter.IsPrimitive);
            Assert.Null(presenter.Parent);
            Assert.AreEqual(2, presenter.Children.Count);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Type);
            Assert.AreEqual(instance, presenter.Value);
            Assert.AreEqual("String", presenter.Children[0].Name);
            Assert.AreEqual("aaa", presenter.Children[0].Value);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);

            Assert.AreEqual(2, presenter.Children[1].Children.Count);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Children[1].Type);
            Assert.AreEqual(instance.Ref, presenter.Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[0].Name);
            Assert.AreEqual("bbb", presenter.Children[1].Children[0].Value);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);

            Assert.AreEqual(2, presenter.Children[1].Children[1].Children.Count);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Children[1].Children[1].Type);
            Assert.AreEqual(instance.Ref.Ref, presenter.Children[1].Children[1].Value);
            Assert.AreEqual("String", presenter.Children[1].Children[1].Children[0].Name);
            Assert.AreEqual("ccc", presenter.Children[1].Children[1].Children[0].Value);
            Assert.AreEqual(typeof(ClassWithRef), presenter.Children[1].Children[1].Children[1].Type);
        }

        private static IObjectNode BuildQuantumGraph(object instance)
        {
            var container = new NodeContainer();
            var node = container.GetOrCreateNode(instance);
            return node;
        }
    }
}
