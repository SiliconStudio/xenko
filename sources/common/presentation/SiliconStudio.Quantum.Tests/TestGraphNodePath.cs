using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestGraphNodePath
    {
        public struct Struct
        {
            public string StringMember;
            public Class ClassMember;
        }
        public class Class
        {
            public int IntMember;
            public Struct StructMember;
            public Class ClassMember;
            public List<Class> ListMember = new List<Class>();
        }

        [Test]
        public void TestConstructor()
        {
            var obj = new Class();
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode);
            Assert.True(path.IsValid);
            Assert.True(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
        }

        [Test]
        public void TestClone()
        {
            var obj = new Class { ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj));
            var clone = path1.Clone();
            Assert.AreEqual(path1.RootNode, clone.RootNode);
            Assert.AreEqual(path1.IsValid, clone.IsValid);
            Assert.AreEqual(path1.IsEmpty, clone.IsEmpty);
            Assert.AreEqual(path1.GetNode(), clone.GetNode());
            var path2 = path1.PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            clone = path2.Clone();
            Assert.AreEqual(path2.RootNode, clone.RootNode);
            Assert.AreEqual(path2.IsValid, clone.IsValid);
            Assert.AreEqual(path2.IsEmpty, clone.IsEmpty);
            Assert.AreEqual(path2.GetNode(), clone.GetNode());
            var path3 = path1.PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            clone = path3.Clone();
            Assert.AreEqual(path3.RootNode, clone.RootNode);
            Assert.AreEqual(path3.IsValid, clone.IsValid);
            Assert.AreEqual(path3.IsEmpty, clone.IsEmpty);
            Assert.AreEqual(path3.GetNode(), clone.GetNode());
        }

        [Test]
        public void TestCloneNewRoot()
        {
            var obj1 = new Class { ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var obj2 = new Class { ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var newRoot = nodeContainer.GetOrCreateNode(obj2);
            var path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj1));
            var clone = path1.Clone(newRoot);
            Assert.AreNotEqual(newRoot, path1.RootNode);
            Assert.AreEqual(newRoot, clone.RootNode);
            Assert.AreEqual(path1.IsValid, clone.IsValid);
            Assert.AreEqual(path1.IsEmpty, clone.IsEmpty);
            var path2 = path1.PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            clone = path2.Clone(newRoot);
            Assert.AreNotEqual(newRoot, path2.RootNode);
            Assert.AreEqual(newRoot, clone.RootNode);
            Assert.AreEqual(path2.IsValid, clone.IsValid);
            Assert.AreEqual(path2.IsEmpty, clone.IsEmpty);
            var path3 = path1.PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            clone = path3.Clone(newRoot);
            Assert.AreNotEqual(newRoot, path3.RootNode);
            Assert.AreEqual(newRoot, clone.RootNode);
            Assert.AreEqual(path3.IsValid, clone.IsValid);
            Assert.AreEqual(path3.IsEmpty, clone.IsEmpty);
        }

        [Test]
        public void TestPushMember()
        {
            var obj = new Class();
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.IntMember));
            var intNode = rootNode.GetChild(nameof(Class.IntMember));
            var nodes = new[] { rootNode, intNode };
            Assert.NotNull(intNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }

        [Test]
        public void TestPushStructMember()
        {
            var obj = new Class { StructMember = { StringMember = "aa" } };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.StructMember)).PushMember(nameof(Struct.StringMember));
            var structNode = rootNode.GetChild(nameof(Class.StructMember));
            var memberNode = rootNode.GetChild(nameof(Class.StructMember)).GetChild(nameof(Struct.StringMember));
            var nodes = new[] { rootNode, structNode, memberNode };
            Assert.NotNull(memberNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(memberNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }

        [Test]
        public void TestPushTarget()
        {
            var obj = new Class { ClassMember = new Class() };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember)).PushTarget();
            var targetNode = nodeContainer.GetNode(obj.ClassMember);
            var nodes = new[] { rootNode, rootNode.GetChild(nameof(Class.ClassMember)), targetNode };
            Assert.NotNull(targetNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(targetNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }

        [Test]
        public void TestPushTargetAndMember()
        {
            var obj = new Class { ClassMember = new Class() };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            var targetNode = nodeContainer.GetNode(obj.ClassMember);
            var intNode = targetNode.GetChild(nameof(Class.IntMember));
            var nodes = new[] { rootNode, rootNode.GetChild(nameof(Class.ClassMember)), targetNode, intNode };
            Assert.NotNull(targetNode);
            Assert.NotNull(intNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }

        [Test]
        public void TestPushIndex()
        {
            var obj = new Class { ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1));
            var targetNode = nodeContainer.GetNode(obj.ListMember[1]);
            var nodes = new[] { rootNode, rootNode.GetChild(nameof(Class.ListMember)), targetNode };
            Assert.NotNull(targetNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(targetNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }

        [Test]
        public void TestPushIndexAndMember()
        {
            var obj = new Class { ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);
            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            var targetNode = nodeContainer.GetNode(obj.ListMember[1]);
            var intNode = targetNode.GetChild(nameof(Class.IntMember));
            var nodes = new[] { rootNode, rootNode.GetChild(nameof(Class.ListMember)), targetNode, intNode };
            Assert.NotNull(targetNode);
            Assert.NotNull(intNode);
            Assert.True(path.IsValid);
            Assert.False(path.IsEmpty);
            Assert.AreEqual(rootNode, path.RootNode);
            Assert.AreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                Assert.AreEqual(nodes[i++], node);
            }
            Assert.AreEqual(nodes.Length, i);
        }
    }
}
