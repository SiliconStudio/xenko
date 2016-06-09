using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestGraphNodeLinker
    {
        public class SimpleClass
        {
            public int Member1;
            public SimpleClass Member2;
        }

        public class InterfaceMember
        {
            public int Member1;
            public IInterface Member2;
        }

        public interface IInterface
        {
            int Member1Common { get; set; }
        }

        public class Implem1 : IInterface
        {
            public int Member1Common { get; set; }
            public SimpleClass Member2Implem1 { get; set; }
        }

        public class Implem2 : IInterface
        {
            public int Member1Common { get; set; }
            public SimpleClass Member2Implem2 { get; set; }
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

        public class TestLinker : GraphNodeLinker
        {
            public Dictionary<IGraphNode, IGraphNode> LinkedNodes = new Dictionary<IGraphNode, IGraphNode>();

            protected override void LinkNodes(IGraphNode sourceNode, IGraphNode targetNode)
            {
                LinkedNodes.Add(sourceNode, targetNode);
                base.LinkNodes(sourceNode, targetNode);
            }
        }

        [Test]
        public void TestSimpleObject()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new SimpleClass { Member1 = 3, Member2 = new SimpleClass() };
            var instance2 = new SimpleClass { Member1 = 4, Member2 = new SimpleClass() };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            var expectedLinks =  new Dictionary < IGraphNode, IGraphNode>
            {
                { source, target },
                { source.GetChild(nameof(SimpleClass.Member1)), target.GetChild(nameof(SimpleClass.Member1)) },
                { source.GetChild(nameof(SimpleClass.Member2)), target.GetChild(nameof(SimpleClass.Member2)) },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget(), target.GetChild(nameof(SimpleClass.Member2)).GetTarget() },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)), target.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)) },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)), target.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)) },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestSimpleObjectWithNullInTarget()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new SimpleClass { Member1 = 3, Member2 = new SimpleClass() };
            var instance2 = new SimpleClass { Member1 = 4, Member2 = null };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            var expectedLinks = new Dictionary<IGraphNode, IGraphNode>
            {
                { source, target },
                { source.GetChild(nameof(SimpleClass.Member1)), target.GetChild(nameof(SimpleClass.Member1)) },
                { source.GetChild(nameof(SimpleClass.Member2)), target.GetChild(nameof(SimpleClass.Member2)) },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget(), null },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)), null },
                { source.GetChild(nameof(SimpleClass.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)), null },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestObjectWithStruct()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new StructClass { Member1 = 3, Member2 = new Struct { Member2 = new SimpleClass() } };
            var instance2 = new StructClass { Member1 = 3, Member2 = new Struct { Member2 = new SimpleClass() } };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            var expectedLinks = new Dictionary<IGraphNode, IGraphNode>
            {
                { source, target },
                { source.GetChild(nameof(StructClass.Member1)), target.GetChild(nameof(StructClass.Member1)) },
                { source.GetChild(nameof(StructClass.Member2)), target.GetChild(nameof(StructClass.Member2)) },
                { source.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member1)), target.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member1)) },
                { source.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)), target.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)) },
                { source.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget(), target.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget() },
                { source.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)), target.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member1)) },
                { source.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)), target.GetChild(nameof(StructClass.Member2)).GetChild(nameof(Struct.Member2)).GetTarget().GetChild(nameof(SimpleClass.Member2)) },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestInterfaceMemberDifferentImplementations()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new InterfaceMember { Member1 = 3, Member2 = new Implem1 { Member2Implem1 = new SimpleClass() } };
            var instance2 = new InterfaceMember { Member1 = 3, Member2 = new Implem2 { Member2Implem2 = new SimpleClass() } };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            var expectedLinks = new Dictionary<IGraphNode, IGraphNode>
            {
                { source, target },
                { source.GetChild(nameof(InterfaceMember.Member1)), target.GetChild(nameof(InterfaceMember.Member1)) },
                { source.GetChild(nameof(InterfaceMember.Member2)), target.GetChild(nameof(InterfaceMember.Member2)) },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget(), target.GetChild(nameof(InterfaceMember.Member2)).GetTarget() },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member1Common)), target.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member1Common)) },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member2Implem1)), null },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member2Implem1)).GetTarget(), null },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member2Implem1)).GetTarget().GetChild(nameof(SimpleClass.Member1)), null },
                { source.GetChild(nameof(InterfaceMember.Member2)).GetTarget().GetChild(nameof(Implem1.Member2Implem1)).GetTarget().GetChild(nameof(SimpleClass.Member2)), null },
            };
            VerifyLinks(expectedLinks, linker);
        }

        private static void VerifyLinks(Dictionary<IGraphNode, IGraphNode> expectedLinks, TestLinker linker)
        {
            Assert.AreEqual(expectedLinks.Count, linker.LinkedNodes.Count);
            foreach (var link in expectedLinks)
            {
                IGraphNode actualTarget;
                Assert.True(linker.LinkedNodes.TryGetValue(link.Key, out actualTarget));
                Assert.AreEqual(link.Value, actualTarget);
            }
        }
    }
}
