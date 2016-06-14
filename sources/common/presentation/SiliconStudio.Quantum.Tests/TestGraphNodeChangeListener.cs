using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestGraphNodeChangeListener
    {
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

        public class TestVisitor : GraphVisitorBase
        {
            public readonly List<Tuple<IGraphNode, GraphNodePath>> Result = new List<Tuple<IGraphNode, GraphNodePath>>();

            public override void Visit(IGraphNode node, GraphNodePath initialPath = null)
            {
                Result.Clear();
                base.Visit(node, initialPath);
            }

            protected override void VisitNode(IGraphNode node, GraphNodePath currentPath)
            {
                Result.Add(Tuple.Create(node, currentPath));
                base.VisitNode(node, currentPath);
            }
        }


        [Test]
        public void TestChangePrimitiveMember()
        {
            var nodeContainer = new NodeContainer();
            var instance = new ComplexClass { Member1 = 3 };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member1));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member1));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, 3, 4, path, () => node.Content.Update(4));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, 4, 5, path, () => node.Content.Update(5));
        }

        [Test]
        public void TestChangeReferenceMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member2 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member2));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member2));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
        }

        [Test]
        public void TestChangeReferenceMemberToNull()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), null, new SimpleClass() };
            var instance = new ComplexClass { Member2 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member2));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member2));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
        }

        [Test]
        public void TestChangeBoxedPrimitiveMember()
        {
            var nodeContainer = new NodeContainer();
            var instance = new ComplexClass { Member3 = 3 };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member3));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member3));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, 3, 4, path, () => node.Content.Update(4));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, 4, 5, path, () => node.Content.Update(5));
        }

        [Test]
        public void TestChangeReferenceInObjectMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member3 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member3));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member3));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
        }

        [Test]
        public void TestChangeStruct()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member4 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member4));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member4));
            Assert.AreEqual("aa", node.GetChild(nameof(Struct.Member1)).Content.Retrieve());
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            Assert.AreEqual("bb", node.GetChild(nameof(Struct.Member1)).Content.Retrieve());
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
            Assert.AreEqual("cc", node.GetChild(nameof(Struct.Member1)).Content.Retrieve());
            TestContentChange(listener, node.GetChild(nameof(Struct.Member1)), ContentChangeType.ValueChange, Index.Empty, "cc", "dd", path.PushMember(nameof(Struct.Member1)), () => node.GetChild(nameof(Struct.Member1)).Content.Update("dd"));
            Assert.AreEqual("dd", node.GetChild(nameof(Struct.Member1)).Content.Retrieve());
        }

        [Test]
        public void TestChangeStructMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member4 = new Struct { Member1 = obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member4)).GetChild(nameof(Struct.Member1));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member4)).PushMember(nameof(Struct.Member1));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1], Index.Empty));
            Assert.AreEqual(node, rootNode.GetChild(nameof(ComplexClass.Member4)).GetChild(nameof(Struct.Member1)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2], Index.Empty));
            Assert.AreEqual(node, rootNode.GetChild(nameof(ComplexClass.Member4)).GetChild(nameof(Struct.Member1)));
        }

        [Test]
        public void TestChangePrimitiveList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<string> { "aa" }, new List<string> { "bb" }, new List<string> { "cc" } };
            var instance = new ComplexClass { Member5 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member5));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member5));
            Assert.AreEqual("aa", node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            Assert.AreEqual("bb", node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
            Assert.AreEqual("cc", node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), "cc", "dd", path, () => node.Content.Update("dd", new Index(0)));
            Assert.AreEqual("dd", node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangePrimitiveListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member5 = new List<string> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member5));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member5));
            Assert.AreEqual(obj[0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[0], obj[1], path, () => node.Content.Update(obj[1], new Index(0)));
            Assert.AreEqual(obj[1], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[1], obj[2], path, () => node.Content.Update(obj[2], new Index(0)));
            Assert.AreEqual(obj[2], node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangeReferenceList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<SimpleClass> { new SimpleClass() }, new List<SimpleClass> { new SimpleClass() }, new List<SimpleClass> { new SimpleClass() } };
            var instance = new ComplexClass { Member6 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member6));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member6));
            Assert.AreEqual(obj[0][0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            Assert.AreEqual(obj[1][0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
            Assert.AreEqual(obj[2][0], node.Content.Retrieve(new Index(0)));
            var newItem = new SimpleClass();
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[2][0], newItem, path, () => node.Content.Update(newItem, new Index(0)));
            Assert.AreEqual(newItem, node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangeReferenceListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new SimpleClass(), new SimpleClass(), new SimpleClass() };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member6));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member6));
            Assert.AreEqual(obj[0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[0], obj[1], path, () => node.Content.Update(obj[1], new Index(0)));
            Assert.AreEqual(obj[1], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[1], obj[2], path, () => node.Content.Update(obj[2], new Index(0)));
            Assert.AreEqual(obj[2], node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangeReferenceListItemMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { 3, 4, 5 };
            var instance = new ComplexClass { Member6 = new List<SimpleClass> { new SimpleClass(), new SimpleClass { Member1 = obj[0] } } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member6)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member6)).PushIndex(new Index(1)).PushMember(nameof(SimpleClass.Member1));
            Assert.AreEqual(obj[0], node.Content.Retrieve());
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1], Index.Empty));
            Assert.AreEqual(obj[1], node.Content.Retrieve());
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2], Index.Empty));
            Assert.AreEqual(obj[2], node.Content.Retrieve());
        }

        [Test]
        public void TestChangeStructList()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new List<Struct> { new Struct() }, new List<Struct> { new Struct() }, new List<Struct> { new Struct() } };
            var instance = new ComplexClass { Member7 = obj[0] };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member7));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member7));
            Assert.AreEqual(obj[0][0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1]));
            Assert.AreEqual(obj[1][0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2]));
            Assert.AreEqual(obj[2][0], node.Content.Retrieve(new Index(0)));
            var newItem = new Struct();
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[2][0], newItem, path, () => node.Content.Update(newItem, new Index(0)));
            Assert.AreEqual(newItem, node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangeStructListItem()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { new Struct { Member1 = "aa" }, new Struct { Member1 = "bb" }, new Struct { Member1 = "cc" } };
            var instance = new ComplexClass { Member7 = new List<Struct> { obj[0] } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member7));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member7));
            Assert.AreEqual(obj[0], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[0], obj[1], path, () => node.Content.Update(obj[1], new Index(0)));
            Assert.AreEqual(obj[1], node.Content.Retrieve(new Index(0)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, new Index(0), obj[1], obj[2], path, () => node.Content.Update(obj[2], new Index(0)));
            Assert.AreEqual(obj[2], node.Content.Retrieve(new Index(0)));
        }

        [Test]
        public void TestChangeStructListItemMember()
        {
            var nodeContainer = new NodeContainer();
            var obj = new[] { "aa", "bb", "cc" };
            var instance = new ComplexClass { Member7 = new List<Struct> { new Struct(), new Struct { Member1 = obj[0] } } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var listener = new GraphNodeChangeListener(rootNode);
            var node = rootNode.GetChild(nameof(ComplexClass.Member7)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1));
            var path = new GraphNodePath(rootNode).PushMember(nameof(ComplexClass.Member7)).PushIndex(new Index(1)).PushMember(nameof(SimpleClass.Member1));
            Assert.AreEqual(obj[0], node.Content.Retrieve());
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[0], obj[1], path, () => node.Content.Update(obj[1], Index.Empty));
            Assert.AreEqual(obj[1], node.Content.Retrieve());
            // TODO: would be nice to be able to keep the same boxed node!
            //Assert.AreEqual(node, rootNode.GetChild(nameof(ComplexClass.Member7)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1)));
            TestContentChange(listener, node, ContentChangeType.ValueChange, Index.Empty, obj[1], obj[2], path, () => node.Content.Update(obj[2], Index.Empty));
            Assert.AreEqual(obj[2], node.Content.Retrieve());
            //Assert.AreEqual(node, rootNode.GetChild(nameof(ComplexClass.Member7)).GetTarget(new Index(1)).GetChild(nameof(SimpleClass.Member1)));
        }

        private static void VerifyListenerEvent(GraphContentChangeEventArgs e, IGraphNode contentOwner, ContentChangeType type, Index index, object oldValue, object newValue, GraphNodePath path, bool changeApplied)
        {
            Assert.NotNull(e);
            Assert.NotNull(contentOwner);
            Assert.NotNull(path);
            Assert.AreEqual(type, e.ChangeType);
            Assert.AreEqual(contentOwner.Content, e.Content);
            Assert.AreEqual(index, e.Index);
            Assert.AreEqual(newValue, e.NewValue);
            Assert.AreEqual(oldValue, e.OldValue);
            Assert.AreEqual(path, e.Path);
            if (type == ContentChangeType.ValueChange)
            {
                Assert.AreEqual(changeApplied ? newValue : oldValue, contentOwner.Content.Retrieve(index));
            }
        }

        private static void TestContentChange(GraphNodeChangeListener listener, IGraphNode contentOwner, ContentChangeType type, Index index, object oldValue, object newValue, GraphNodePath path, Action change)
        {
            var i = 0;
            var prepareChange = new EventHandler<GraphContentChangeEventArgs>((sender, e) => { Assert.AreEqual(0, i); VerifyListenerEvent(e, contentOwner, type, index, oldValue, newValue, path, false); ++i; });
            var changing = new EventHandler<GraphContentChangeEventArgs>((sender, e) => { Assert.AreEqual(1, i); VerifyListenerEvent(e, contentOwner, type, index, oldValue, newValue, path, false); ++i; });
            var changed = new EventHandler<GraphContentChangeEventArgs>((sender, e) => { Assert.AreEqual(2, i); VerifyListenerEvent(e, contentOwner, type, index, oldValue, newValue, path, true); ++i; });
            var finalizeChange = new EventHandler<GraphContentChangeEventArgs>((sender, e) => { Assert.AreEqual(3, i); VerifyListenerEvent(e, contentOwner, type, index, oldValue, newValue, path, true); ++i; });
            listener.PrepareChange += prepareChange;
            listener.Changing += changing;
            listener.Changed += changed;
            listener.FinalizeChange += finalizeChange;
            change();
            Assert.AreEqual(4, i);
            listener.PrepareChange -= prepareChange;
            listener.Changing -= changing;
            listener.Changed -= changed;
            listener.FinalizeChange -= finalizeChange;
        }
    }
}
