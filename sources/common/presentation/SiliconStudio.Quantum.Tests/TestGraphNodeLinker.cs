using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

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
            public Dictionary<IContentNode, IContentNode> LinkedNodes = new Dictionary<IContentNode, IContentNode>();

            protected override void LinkNodes(IContentNode sourceNode, IContentNode targetNode)
            {
                LinkedNodes.Add(sourceNode, targetNode);
                base.LinkNodes(sourceNode, targetNode);
            }
        }

        public class CustomFindTargetLinker : TestLinker
        {
            private readonly IObjectNode root;

            public CustomFindTargetLinker(NodeContainer container, IObjectNode root)
            {
                this.root = root;
                CustomTarget = container.GetOrCreateNode(new SimpleClass());
            }

            public IObjectNode CustomTarget { get; }

            protected override IContentNode FindTarget(IContentNode sourceNode)
            {
                if (sourceNode is ObjectContent && sourceNode.Type == typeof(SimpleClass) && sourceNode != root)
                {
                    return CustomTarget;
                }
                return base.FindTarget(sourceNode);
            }
        }

        public class CustomFindTargetReferenceLinker : TestLinker
        {
            protected override ObjectReference FindTargetReference(IContentNode sourceNode, IContentNode targetNode, ObjectReference sourceReference)
            {
                if (sourceReference.Index.IsEmpty)
                    return base.FindTargetReference(sourceNode, targetNode, sourceReference);

                var matchValue = 0;
                if (sourceReference.TargetNode != null)
                    matchValue = (int)sourceReference.TargetNode[nameof(SimpleClass.Member1)].Value;

                var targetReference = targetNode.ItemReferences;
                return targetReference?.FirstOrDefault(x => (int)x.TargetNode[nameof(SimpleClass.Member1)].Value == matchValue);

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
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
                { source[nameof(SimpleClass.Member2)].Target, target[nameof(SimpleClass.Member2)].Target },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member2)] },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestObjectWithListOfReferences()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new ObjectListClass { Member1 = 3, Member2 = new List<SimpleClass> { new SimpleClass(), new SimpleClass() } };
            var instance2 = new ObjectListClass { Member1 = 3, Member2 = new List<SimpleClass> { new SimpleClass(), new SimpleClass() } };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            Index index = new Index(0);
            IContentNode tempQualifier = source[nameof(SimpleClass.Member2)];
            Index index1 = new Index(0);
            IContentNode tempQualifier1 = target[nameof(SimpleClass.Member2)];
            Index index2 = new Index(0);
            IContentNode tempQualifier2 = source[nameof(SimpleClass.Member2)];
            Index index3 = new Index(0);
            IContentNode tempQualifier3 = target[nameof(SimpleClass.Member2)];
            Index index4 = new Index(0);
            IContentNode tempQualifier4 = source[nameof(SimpleClass.Member2)];
            Index index5 = new Index(0);
            IContentNode tempQualifier5 = target[nameof(SimpleClass.Member2)];
            Index index6 = new Index(1);
            IContentNode tempQualifier6 = source[nameof(SimpleClass.Member2)];
            Index index7 = new Index(1);
            IContentNode tempQualifier7 = target[nameof(SimpleClass.Member2)];
            Index index8 = new Index(1);
            IContentNode tempQualifier8 = source[nameof(SimpleClass.Member2)];
            Index index9 = new Index(1);
            IContentNode tempQualifier9 = target[nameof(SimpleClass.Member2)];
            Index index10 = new Index(1);
            IContentNode tempQualifier10 = source[nameof(SimpleClass.Member2)];
            Index index11 = new Index(1);
            IContentNode tempQualifier11 = target[nameof(SimpleClass.Member2)];
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
                { tempQualifier.IndexedTarget(index), tempQualifier1.IndexedTarget(index1) },
                { tempQualifier2.IndexedTarget(index2)[nameof(SimpleClass.Member1)], tempQualifier3.IndexedTarget(index3)[nameof(SimpleClass.Member1)] },
                { tempQualifier4.IndexedTarget(index4)[nameof(SimpleClass.Member2)], tempQualifier5.IndexedTarget(index5)[nameof(SimpleClass.Member2)] },
                { tempQualifier6.IndexedTarget(index6), tempQualifier7.IndexedTarget(index7) },
                { tempQualifier8.IndexedTarget(index8)[nameof(SimpleClass.Member1)], tempQualifier9.IndexedTarget(index9)[nameof(SimpleClass.Member1)] },
                { tempQualifier10.IndexedTarget(index10)[nameof(SimpleClass.Member2)], tempQualifier11.IndexedTarget(index11)[nameof(SimpleClass.Member2)] },
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
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
                { source[nameof(SimpleClass.Member2)].Target, null },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member1)], null },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member2)], null },
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
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(StructClass.Member1)], target[nameof(StructClass.Member1)] },
                { source[nameof(StructClass.Member2)], target[nameof(StructClass.Member2)] },
                { source[nameof(StructClass.Member2)].Target, target[nameof(StructClass.Member2)].Target },
                { source[nameof(StructClass.Member2)].Target[nameof(Struct.Member1)], target[nameof(StructClass.Member2)].Target[nameof(Struct.Member1)] },
                { source[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)], target[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)] },
                { source[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target, target[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target },
                { source[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target[nameof(SimpleClass.Member1)], target[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target[nameof(SimpleClass.Member1)] },
                { source[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target[nameof(SimpleClass.Member2)], target[nameof(StructClass.Member2)].Target[nameof(Struct.Member2)].Target[nameof(SimpleClass.Member2)] },
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
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(InterfaceMember.Member1)], target[nameof(InterfaceMember.Member1)] },
                { source[nameof(InterfaceMember.Member2)], target[nameof(InterfaceMember.Member2)] },
                { source[nameof(InterfaceMember.Member2)].Target, target[nameof(InterfaceMember.Member2)].Target },
                { source[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member1Common)], target[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member1Common)] },
                { source[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member2Implem1)], null },
                { source[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member2Implem1)].Target, null },
                { source[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member2Implem1)].Target[nameof(SimpleClass.Member1)], null },
                { source[nameof(InterfaceMember.Member2)].Target[nameof(Implem1.Member2Implem1)].Target[nameof(SimpleClass.Member2)], null },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestCustomFindTarget()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new SimpleClass { Member1 = 3, Member2 = new SimpleClass() };
            var instance2 = new SimpleClass { Member1 = 4, Member2 = new SimpleClass() };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new CustomFindTargetLinker(nodeContainer, source);
            linker.LinkGraph(source, target);
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
                { source[nameof(SimpleClass.Member2)].Target, linker.CustomTarget },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member1)], linker.CustomTarget[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)].Target[nameof(SimpleClass.Member2)], linker.CustomTarget[nameof(SimpleClass.Member2)] },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestCustomFindTargetReference()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new ObjectListClass { Member1 = 3, Member2 = new List<SimpleClass> { new SimpleClass { Member1 = 1 }, new SimpleClass { Member1 = 2 }, new SimpleClass { Member1 = 3 } } };
            var instance2 = new ObjectListClass { Member1 = 3, Member2 = new List<SimpleClass> { new SimpleClass { Member1 = 2 }, new SimpleClass { Member1 = 4 }, new SimpleClass { Member1 = 1 } } };
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new CustomFindTargetReferenceLinker();
            linker.LinkGraph(source, target);
            // Expected links by index: 0 -> 2, 1 -> 0, 2 -> null
            Index index = new Index(0);
            IContentNode tempQualifier = source[nameof(SimpleClass.Member2)];
            Index index1 = new Index(2);
            IContentNode tempQualifier1 = target[nameof(SimpleClass.Member2)];
            Index index2 = new Index(0);
            IContentNode tempQualifier2 = source[nameof(SimpleClass.Member2)];
            Index index3 = new Index(2);
            IContentNode tempQualifier3 = target[nameof(SimpleClass.Member2)];
            Index index4 = new Index(0);
            IContentNode tempQualifier4 = source[nameof(SimpleClass.Member2)];
            Index index5 = new Index(2);
            IContentNode tempQualifier5 = target[nameof(SimpleClass.Member2)];
            Index index6 = new Index(1);
            IContentNode tempQualifier6 = source[nameof(SimpleClass.Member2)];
            Index index7 = new Index(0);
            IContentNode tempQualifier7 = target[nameof(SimpleClass.Member2)];
            Index index8 = new Index(1);
            IContentNode tempQualifier8 = source[nameof(SimpleClass.Member2)];
            Index index9 = new Index(0);
            IContentNode tempQualifier9 = target[nameof(SimpleClass.Member2)];
            Index index10 = new Index(1);
            IContentNode tempQualifier10 = source[nameof(SimpleClass.Member2)];
            Index index11 = new Index(0);
            IContentNode tempQualifier11 = target[nameof(SimpleClass.Member2)];
            Index index12 = new Index(2);
            IContentNode tempQualifier12 = source[nameof(SimpleClass.Member2)];
            Index index13 = new Index(2);
            IContentNode tempQualifier13 = source[nameof(SimpleClass.Member2)];
            Index index14 = new Index(2);
            IContentNode tempQualifier14 = source[nameof(SimpleClass.Member2)];
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
                { tempQualifier.IndexedTarget(index), tempQualifier1.IndexedTarget(index1) },
                { tempQualifier2.IndexedTarget(index2)[nameof(SimpleClass.Member1)], tempQualifier3.IndexedTarget(index3)[nameof(SimpleClass.Member1)] },
                { tempQualifier4.IndexedTarget(index4)[nameof(SimpleClass.Member2)], tempQualifier5.IndexedTarget(index5)[nameof(SimpleClass.Member2)] },
                { tempQualifier6.IndexedTarget(index6), tempQualifier7.IndexedTarget(index7) },
                { tempQualifier8.IndexedTarget(index8)[nameof(SimpleClass.Member1)], tempQualifier9.IndexedTarget(index9)[nameof(SimpleClass.Member1)] },
                { tempQualifier10.IndexedTarget(index10)[nameof(SimpleClass.Member2)], tempQualifier11.IndexedTarget(index11)[nameof(SimpleClass.Member2)] },
                { tempQualifier12.IndexedTarget(index12), null },
                { tempQualifier13.IndexedTarget(index13)[nameof(SimpleClass.Member1)], null },
                { tempQualifier14.IndexedTarget(index14)[nameof(SimpleClass.Member2)], null },
            };
            VerifyLinks(expectedLinks, linker);
        }

        [Test]
        public void TestReentrancy()
        {
            var nodeContainer = new NodeContainer();
            var instance1 = new SimpleClass { Member1 = 3 };
            var instance2 = new SimpleClass { Member1 = 4 };
            instance1.Member2 = instance1;
            instance2.Member2 = instance2;
            var source = nodeContainer.GetOrCreateNode(instance1);
            var target = nodeContainer.GetOrCreateNode(instance2);
            var linker = new TestLinker();
            linker.LinkGraph(source, target);
            var expectedLinks = new Dictionary<IContentNode, IContentNode>
            {
                { source, target },
                { source[nameof(SimpleClass.Member1)], target[nameof(SimpleClass.Member1)] },
                { source[nameof(SimpleClass.Member2)], target[nameof(SimpleClass.Member2)] },
            };
            VerifyLinks(expectedLinks, linker);
        }

        private static void VerifyLinks(Dictionary<IContentNode, IContentNode> expectedLinks, TestLinker linker)
        {
            Assert.AreEqual(expectedLinks.Count, linker.LinkedNodes.Count);
            foreach (var link in expectedLinks)
            {
                IContentNode actualTarget;
                Assert.True(linker.LinkedNodes.TryGetValue(link.Key, out actualTarget));
                Assert.AreEqual(link.Value, actualTarget);
            }
        }
    }
}
