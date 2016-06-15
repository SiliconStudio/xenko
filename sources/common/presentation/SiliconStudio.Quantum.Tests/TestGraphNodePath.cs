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
            AssertAreEqual(rootNode, path.RootNode);
        }

        [Test]
        public void TestEquals()
        {
            var obj = new Class { StructMember = { StringMember = "aa" }, ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.IntMember));
            var path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.IntMember));
            AssertAreEqual(path1, path2);
            AssertAreEqual(path1.GetHashCode(), path2.GetHashCode());
            path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember));
            AssertAreNotEqual(path1, path2);
            AssertAreNotEqual(path1.GetHashCode(), path2.GetHashCode());
            path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember));
            AssertAreEqual(path1, path2);
            AssertAreEqual(path1.GetHashCode(), path2.GetHashCode());
            path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember)).PushTarget();
            AssertAreNotEqual(path1, path2);
            AssertAreNotEqual(path1.GetHashCode(), path2.GetHashCode());
            path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember)).PushTarget();
            AssertAreEqual(path1, path2);
            AssertAreEqual(path1.GetHashCode(), path2.GetHashCode());
            path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            AssertAreNotEqual(path1, path2);
            AssertAreNotEqual(path1.GetHashCode(), path2.GetHashCode());
            path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            AssertAreEqual(path1, path2);
            AssertAreEqual(path1.GetHashCode(), path2.GetHashCode());
            path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ListMember)).PushIndex(new Index(0));
            AssertAreNotEqual(path1, path2);
            AssertAreNotEqual(path1.GetHashCode(), path2.GetHashCode());
            path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ListMember)).PushIndex(new Index(0));
            AssertAreEqual(path1, path2);
            AssertAreEqual(path1.GetHashCode(), path2.GetHashCode());
            path2 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj)).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1));
            AssertAreNotEqual(path1, path2);
            AssertAreNotEqual(path1.GetHashCode(), path2.GetHashCode());
        }

        [Test]
        public void TestClone()
        {
            var obj = new Class { ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var path1 = new GraphNodePath(nodeContainer.GetOrCreateNode(obj));
            var clone = path1.Clone();
            AssertAreEqual(path1, clone);
            AssertAreEqual(path1.GetHashCode(), clone.GetHashCode());
            AssertAreEqual(path1.RootNode, clone.RootNode);
            AssertAreEqual(path1.IsValid, clone.IsValid);
            AssertAreEqual(path1.IsEmpty, clone.IsEmpty);
            AssertAreEqual(path1.GetNode(), clone.GetNode());
            var path2 = path1.PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            clone = path2.Clone();
            AssertAreEqual(path2, clone);
            AssertAreEqual(path2.RootNode, clone.RootNode);
            AssertAreEqual(path2.IsValid, clone.IsValid);
            AssertAreEqual(path2.IsEmpty, clone.IsEmpty);
            AssertAreEqual(path2.GetNode(), clone.GetNode());
            var path3 = path1.PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            clone = path3.Clone();
            AssertAreEqual(path3, clone);
            AssertAreEqual(path3.RootNode, clone.RootNode);
            AssertAreEqual(path3.IsValid, clone.IsValid);
            AssertAreEqual(path3.IsEmpty, clone.IsEmpty);
            AssertAreEqual(path3.GetNode(), clone.GetNode());
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
            AssertAreNotEqual(path1, clone);
            AssertAreNotEqual(path1.GetHashCode(), clone.GetHashCode());
            AssertAreNotEqual(newRoot, path1.RootNode);
            AssertAreEqual(newRoot, clone.RootNode);
            AssertAreEqual(path1.IsValid, clone.IsValid);
            AssertAreEqual(path1.IsEmpty, clone.IsEmpty);
            var path2 = path1.PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            clone = path2.Clone(newRoot);
            AssertAreNotEqual(path2, clone);
            AssertAreNotEqual(path2.GetHashCode(), clone.GetHashCode());
            AssertAreNotEqual(newRoot, path2.RootNode);
            AssertAreEqual(newRoot, clone.RootNode);
            AssertAreEqual(path2.IsValid, clone.IsValid);
            AssertAreEqual(path2.IsEmpty, clone.IsEmpty);
            var path3 = path1.PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            clone = path3.Clone(newRoot);
            AssertAreNotEqual(path3, clone);
            AssertAreNotEqual(path3.GetHashCode(), clone.GetHashCode());
            AssertAreNotEqual(newRoot, path3.RootNode);
            AssertAreEqual(newRoot, clone.RootNode);
            AssertAreEqual(path3.IsValid, clone.IsValid);
            AssertAreEqual(path3.IsEmpty, clone.IsEmpty);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(memberNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(targetNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(targetNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
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
            AssertAreEqual(rootNode, path.RootNode);
            AssertAreEqual(intNode, path.GetNode());
            var i = 0;
            foreach (var node in path)
            {
                AssertAreEqual(nodes[i++], node);
            }
            AssertAreEqual(nodes.Length, i);
        }

        [Test]
        public void TestGetParent()
        {
            var obj = new Class { StructMember = { StringMember = "aa" }, ClassMember = new Class(), ListMember = { new Class(), new Class(), new Class() } };
            var nodeContainer = new NodeContainer();
            var rootNode = nodeContainer.GetOrCreateNode(obj);

            var path = new GraphNodePath(rootNode).PushMember(nameof(Class.IntMember));
            var parentPath = new GraphNodePath(rootNode);
            AssertAreEqual(parentPath, path.GetParent());

            path = new GraphNodePath(rootNode).PushMember(nameof(Class.StructMember)).PushMember(nameof(Struct.StringMember));
            parentPath = new GraphNodePath(rootNode).PushMember(nameof(Class.StructMember));
            AssertAreEqual(parentPath, path.GetParent());

            path = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember)).PushTarget();
            parentPath = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember));
            AssertAreEqual(parentPath, path.GetParent());

            path = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember)).PushTarget().PushMember(nameof(Class.IntMember));
            parentPath = new GraphNodePath(rootNode).PushMember(nameof(Class.ClassMember)).PushTarget();
            AssertAreEqual(parentPath, path.GetParent());

            path = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1));
            parentPath = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember));
            AssertAreEqual(parentPath, path.GetParent());

            path = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1)).PushMember(nameof(Class.IntMember));
            parentPath = new GraphNodePath(rootNode).PushMember(nameof(Class.ListMember)).PushIndex(new Index(1));
            AssertAreEqual(parentPath, path.GetParent());
        }

        // NUnit does not use the Equals method for objects that implement IEnumerable, but that's what we want to use for GraphNodePath
        // ReSharper disable UnusedParameter.Local
        private static void AssertAreEqual(object expected, object actual)
        {
            Assert.True(expected.Equals(actual));
        }

        private static void AssertAreNotEqual(object expected, object actual)
        {
            Assert.False(expected.Equals(actual));
        }
        // ReSharper restore UnusedParameter.Local
    }
}
