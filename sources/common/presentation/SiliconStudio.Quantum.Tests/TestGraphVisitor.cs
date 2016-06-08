using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestGraphVisitor
    {
        public class SimpleClass
        {
            public int Member1;
            public SimpleClass Member2;
        }

        public class StructClass
        {
            public int Member1;
            public Struct Member2;
        }

        public struct Struct
        {
            public string Member1;
            public SimpleClass Member2;
        }

        public class TestVisitor : GraphVisitorBase
        {
            public event Action<IGraphNode, GraphNodePath> VisitingNode;

            public readonly List<Tuple<IGraphNode, GraphNodePath>> Result = new List<Tuple<IGraphNode, GraphNodePath>>();

            public override void VisitNode(IGraphNode node, GraphNodePath currentPath)
            {
                Result.Add(Tuple.Create(node, currentPath));
                VisitingNode?.Invoke(node, currentPath);
                base.VisitNode(node, currentPath);
            }
        }

        [Test]
        public void TestSimpleObject()
        {
            var nodeContainer = new NodeContainer();
            var instance = new SimpleClass { Member1 = 3, Member2 = new SimpleClass() };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.GetChild(nameof(SimpleClass.Member1)),
                rootNode.GetChild(nameof(SimpleClass.Member2)),
                rootNode.GetChild(nameof(SimpleClass.Member2)).GetTarget(),
                rootNode.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)),
                rootNode.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
            };
            Assert.AreEqual(expectedNodes.Length, visitor.Result.Count);
            for (var i = 0; i < expectedNodes.Length; i++)
            {
                Assert.AreEqual(expectedNodes[i], visitor.Result[i].Item1);
                Assert.AreEqual(expectedPaths[i], visitor.Result[i].Item2);
            }
        }

        [Test]
        public void TestObjectWithStruct()
        {
            var nodeContainer = new NodeContainer();
            var instance = new StructClass { Member1 = 3, Member2 = new Struct() };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.GetChild(nameof(StructClass.Member1)),
                rootNode.GetChild(nameof(StructClass.Member2)),
                rootNode.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member1)),
                rootNode.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)).PushMember(nameof(Struct.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)).PushMember(nameof(Struct.Member2)),
            };
            Assert.AreEqual(expectedNodes.Length, visitor.Result.Count);
            for (var i = 0; i < expectedNodes.Length; i++)
            {
                Assert.AreEqual(expectedNodes[i], visitor.Result[i].Item1);
                Assert.AreEqual(expectedPaths[i], visitor.Result[i].Item2);
            }
        }
    }
}