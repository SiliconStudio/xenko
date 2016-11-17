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

        public class SimpleClass2
        {
            public int Member1;
            public SimpleClass Member2;
            public SimpleClass Member3;
        }

        public class StructClass
        {
            public int Member1;
            public Struct Member2;
        }

        public class PrimitiveListClass
        {
            public int Member1;
            public List<string> Member2;
        }

        public class ObjectListClass
        {
            public int Member1;
            public List<SimpleClass> Member2;
        }

        public class StructListClass
        {
            public int Member1;
            public List<Struct> Member2;
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
                rootNode.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target,
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)),
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
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestSimpleObjectInitialPath()
        {
            var nodeContainer = new NodeContainer();
            var instance = new SimpleClass { Member1 = 3, Member2 = new SimpleClass() };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var container = new SimpleClass { Member2 = instance };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var initialPath = new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget();
            var visitor = new TestVisitor();
            visitor.Visit(rootNode, initialPath);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target,
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(containerNode).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestSimpleObjectWithNull()
        {
            var nodeContainer = new NodeContainer();
            var instance = new SimpleClass { Member1 = 3, Member2 = null };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
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
                rootNode.TryGetChild(nameof(StructClass.Member1)),
                rootNode.TryGetChild(nameof(StructClass.Member2)),
                rootNode.TryGetChild(nameof(StructClass.Member2)).TryGetChild(nameof(Struct.Member1)),
                rootNode.TryGetChild(nameof(StructClass.Member2)).TryGetChild(nameof(Struct.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)).PushMember(nameof(Struct.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructClass.Member2)).PushMember(nameof(Struct.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestObjectWithPrimitiveList()
        {
            var nodeContainer = new NodeContainer();
            var instance = new PrimitiveListClass { Member1 = 3, Member2 = new List<string> { "aaa", "bbb", "ccc" } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(PrimitiveListClass.Member1)),
                rootNode.TryGetChild(nameof(PrimitiveListClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(PrimitiveListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(PrimitiveListClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestObjectWithObjectList()
        {
            var nodeContainer = new NodeContainer();
            // We also test a null item in the list
            var instance = new ObjectListClass { Member1 = 3, Member2 = new List<SimpleClass> { new SimpleClass(), null, new SimpleClass() } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            Index index = new Index(2);
            IGraphNode tempQualifier = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            Index index1 = new Index(2);
            IGraphNode tempQualifier1 = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            Index index2 = new Index(2);
            IGraphNode tempQualifier2 = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            Index index3 = new Index(0);
            IGraphNode tempQualifier3 = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            Index index4 = new Index(0);
            IGraphNode tempQualifier4 = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            Index index5 = new Index(0);
            IGraphNode tempQualifier5 = rootNode.TryGetChild(nameof(ObjectListClass.Member2));
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(ObjectListClass.Member1)),
                rootNode.TryGetChild(nameof(ObjectListClass.Member2)),
                tempQualifier4.IndexedTarget(index4),
                tempQualifier5.IndexedTarget(index5).TryGetChild(nameof(ObjectListClass.Member1)),
                tempQualifier3.IndexedTarget(index3).TryGetChild(nameof(ObjectListClass.Member2)),
                tempQualifier1.IndexedTarget(index1),
                tempQualifier2.IndexedTarget(index2).TryGetChild(nameof(ObjectListClass.Member1)),
                tempQualifier.IndexedTarget(index).TryGetChild(nameof(ObjectListClass.Member2)),
              };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(0)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(0)).PushMember(nameof(ObjectListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(0)).PushMember(nameof(ObjectListClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(2)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(2)).PushMember(nameof(ObjectListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(ObjectListClass.Member2)).PushIndex(new Index(2)).PushMember(nameof(ObjectListClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestObjectWithStructList()
        {
            var nodeContainer = new NodeContainer();
            // We also test a null item in the list
            var instance = new StructListClass { Member1 = 3, Member2 = new List<Struct> { new Struct(), new Struct() } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            Index index = new Index(0);
            IGraphNode tempQualifier = rootNode.TryGetChild(nameof(StructListClass.Member2));
            Index index1 = new Index(0);
            IGraphNode tempQualifier1 = rootNode.TryGetChild(nameof(StructListClass.Member2));
            Index index2 = new Index(0);
            IGraphNode tempQualifier2 = rootNode.TryGetChild(nameof(StructListClass.Member2));
            Index index3 = new Index(1);
            IGraphNode tempQualifier3 = rootNode.TryGetChild(nameof(StructListClass.Member2));
            Index index4 = new Index(1);
            IGraphNode tempQualifier4 = rootNode.TryGetChild(nameof(StructListClass.Member2));
            Index index5 = new Index(1);
            IGraphNode tempQualifier5 = rootNode.TryGetChild(nameof(StructListClass.Member2));
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(StructListClass.Member1)),
                rootNode.TryGetChild(nameof(StructListClass.Member2)),
                tempQualifier.IndexedTarget(index),
                tempQualifier1.IndexedTarget(index1).TryGetChild(nameof(StructListClass.Member1)),
                tempQualifier2.IndexedTarget(index2).TryGetChild(nameof(StructListClass.Member2)),
                tempQualifier3.IndexedTarget(index3),
                tempQualifier4.IndexedTarget(index4).TryGetChild(nameof(StructListClass.Member1)),
                tempQualifier5.IndexedTarget(index5).TryGetChild(nameof(StructListClass.Member2)),
              };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(0)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(0)).PushMember(nameof(StructListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(0)).PushMember(nameof(StructListClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(1)).PushMember(nameof(StructListClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(StructListClass.Member2)).PushIndex(new Index(1)).PushMember(nameof(StructListClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestCircularReference()
        {
            var nodeContainer = new NodeContainer();
            var obj1 = new SimpleClass { Member1 = 3 };
            var obj2 = new SimpleClass { Member1 = 3 };
            obj1.Member2 = obj2;
            obj2.Member2 = obj1;
            var rootNode1 = nodeContainer.GetOrCreateNode(obj1);
            var rootNode2 = nodeContainer.GetOrCreateNode(obj2);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode1);
            var expectedNodes = new[]
            {
                rootNode1,
                rootNode1.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode1.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode2,
                rootNode2.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode2.TryGetChild(nameof(SimpleClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode1),
                new GraphNodePath(rootNode1).PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode1).PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode1).PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(rootNode1).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode1).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);

            visitor.Visit(rootNode2);
            expectedNodes = new[]
            {
                rootNode2,
                rootNode2.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode2.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode1,
                rootNode1.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode1.TryGetChild(nameof(SimpleClass.Member2)),
            };
            expectedPaths = new[]
            {
                new GraphNodePath(rootNode2),
                new GraphNodePath(rootNode2).PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode2).PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode2).PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(rootNode2).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode2).PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        [Test]
        public void TestMultipleReferences()
        {
            var nodeContainer = new NodeContainer();
            var commonObj = new SimpleClass();
            var instance = new SimpleClass2 { Member1 = 3, Member2 = new SimpleClass { Member1 = 4, Member2 = commonObj }, Member3 = new SimpleClass { Member1 = 5, Member2 = commonObj } };
            var rootNode = nodeContainer.GetOrCreateNode(instance);
            var visitor = new TestVisitor();
            visitor.Visit(rootNode);
            var expectedNodes = new[]
            {
                rootNode,
                rootNode.TryGetChild(nameof(SimpleClass2.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target,
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target,
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target,
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target.TryGetChild(nameof(SimpleClass.Member2)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target,
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member1)),
                rootNode.TryGetChild(nameof(SimpleClass2.Member3)).Target.TryGetChild(nameof(SimpleClass.Member2)).Target.TryGetChild(nameof(SimpleClass.Member2)),
            };
            var expectedPaths = new[]
            {
                new GraphNodePath(rootNode),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget(),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget(),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget(),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member1)),
                new GraphNodePath(rootNode).PushMember(nameof(SimpleClass2.Member3)).PushTarget().PushMember(nameof(SimpleClass.Member2)).PushTarget().PushMember(nameof(SimpleClass.Member2)),
            };
            VerifyNodesAndPath(expectedNodes, expectedPaths, visitor);
        }

        private static void VerifyNodesAndPath(IReadOnlyList<IGraphNode> expectedNodes, IReadOnlyList<GraphNodePath> expectedPaths, TestVisitor visitor)
        {
            Assert.AreEqual(expectedNodes.Count, visitor.Result.Count);
            Assert.AreEqual(expectedPaths.Count, visitor.Result.Count);
            for (var i = 0; i < expectedNodes.Count; i++)
            {
                Assert.AreEqual(expectedNodes[i], visitor.Result[i].Item1);
                Assert.AreEqual(expectedPaths[i], visitor.Result[i].Item2);
            }
        }
    }
}
