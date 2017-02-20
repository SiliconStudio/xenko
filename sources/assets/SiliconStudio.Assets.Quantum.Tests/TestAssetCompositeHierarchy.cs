using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchy
    {
        [DataContract("MyPart")]
        public class MyPart : IIdentifiable
        {
            [NonOverridable]
            public Guid Id { get; set; }
            public string Name { get; set; }
            public MyPart Parent { get; set; }
            public MyPart MyReference { get; set; }
            public List<MyPart> MyReferences { get; set; }
            public List<MyPart> Children { get; } = new List<MyPart>();
            public void AddChild([NotNull] MyPart child) { Children.Add(child); child.Parent = this; }
            public override string ToString() => $"{Name} [{Id}]";
        }

        [DataContract("MyPartDesign")]
        public class MyPartDesign : IAssetPartDesign<MyPart>
        {
            public BasePart Base { get; set; }
            public MyPart Part { get; set; }
            public override string ToString() => $"Design: {Part.Name} [{Part.Id}]";
        }

        public class MyAsset : AssetCompositeHierarchy<MyPartDesign, MyPart>
        {
            public override MyPart GetParent(MyPart part) => part.Parent;
            public override int IndexOf(MyPart part) => GetParent(part)?.Children.IndexOf(part) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
            public override MyPart GetChild(MyPart part, int index) => part.Children[index];
            public override int GetChildCount(MyPart part) => part.Children.Count;
            public override IEnumerable<MyPart> EnumerateChildParts(MyPart part, bool isRecursive) => isRecursive ? part.Children.DepthFirst(t => t.Children) : part.Children;
        }

        [AssetPropertyGraph(typeof(MyAsset))]
        // ReSharper disable once ClassNeverInstantiated.Local
        private class MyAssetPropertyGraph : AssetCompositeHierarchyPropertyGraph<MyPartDesign, MyPart>
        {
            public MyAssetPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger) { }
            protected override void AddChildPartToParentPart(MyPart parentPart, MyPart childPart, int index) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Add(childPart, new Index(index));
            protected override void RemoveChildPartFromParentPart(MyPart parentPart, MyPart childPart) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Remove(childPart, new Index(parentPart.Children.IndexOf(childPart)));
        }

        [Test]
        public void TestSimpleCloneSubHierarchy()
        {
            var graph = BuildAssetAndGraph(2, 2, 2);
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
        }

        [Test]
        public void TestCloneSubHierarchyWithInternalReference()
        {
            var graph = BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReference = x.Parts[GuidGenerator.Get(6)].Part);
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part.Children[1], cloneRoot.Part.Children[0].MyReference);
        }

        [Test]
        public void TestCloneSubHierarchyWithExternalReferences()
        {
            var graph = BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReferences = new List<MyPart> { x.Parts[GuidGenerator.Get(2)].Part });
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(graph.AssetHierarchy.Hierarchy.Parts[GuidGenerator.Get(2)].Part, cloneRoot.Part.Children[0].MyReferences[0]);
        }

        [Test]
        public void TestSimpleCloneSubHierarchyWithCleanExternalReferences()
        {
            var graph = BuildAssetAndGraph(2, 2, 2);
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
        }

        [Test]
        public void TestCloneSubHierarchyWithInternalReferenceWithCleanExternalReferences()
        {
            var graph = BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReference = x.Parts[GuidGenerator.Get(6)].Part);
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part.Children[1], cloneRoot.Part.Children[0].MyReference);
        }

        [Test]
        public void TestCloneSubHierarchyWithExternalReferencesWithCleanExternalReferences()
        {
            var graph = BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReferences = new List<MyPart> { x.Parts[GuidGenerator.Get(2)].Part });
            Debug.Write(PrintHierarchy(graph.AssetHierarchy));
            var originalRoot = graph.AssetHierarchy.Hierarchy.Parts[graph.AssetHierarchy.Hierarchy.RootPartIds[1]];
            Dictionary<Guid, Guid> remapping;
            var clone = graph.CloneSubHierarchy(originalRoot.Part.Id, SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootPartIds.Single()];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootPartIds.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.AssetHierarchy.Hierarchy.Parts[part.Part.Id];
                Assert.AreNotEqual(matchingPart, part);
                Assert.AreNotEqual(matchingPart.Part, part.Part);
                Assert.AreEqual(matchingPart.Part.Id, part.Part.Id);
                Assert.AreEqual(matchingPart.Part.Name, part.Part.Name);
            }
            Assert.AreEqual(originalRoot.Part.Id, cloneRoot.Part.Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0], cloneRoot.Part.Children[0]);
            Assert.AreNotEqual(originalRoot.Part.Children[1], cloneRoot.Part.Children[1]);
            Assert.AreEqual(originalRoot.Part.Children[0].Id, cloneRoot.Part.Children[0].Id);
            Assert.AreEqual(originalRoot.Part.Children[1].Id, cloneRoot.Part.Children[1].Id);
            Assert.AreNotEqual(originalRoot.Part.Children[0].Parent, cloneRoot.Part.Children[0].Parent);
            Assert.AreNotEqual(originalRoot.Part.Children[1].Parent, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[0].Parent);
            Assert.AreEqual(cloneRoot.Part, cloneRoot.Part.Children[1].Parent);
            Assert.AreEqual(null, cloneRoot.Part.Children[0].MyReferences[0]);
        }

        private static string PrintHierarchy(AssetCompositeHierarchy<MyPartDesign, MyPart> asset)
        {
            var stack = new Stack<Tuple<MyPartDesign, int>>();
            asset.Hierarchy.RootPartIds.Select(x => asset.Hierarchy.Parts[x]).Reverse().ForEach(x => stack.Push(Tuple.Create(x, 0)));
            var sb = new StringBuilder();
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                sb.Append("".PadLeft(current.Item2 * 2));
                sb.AppendLine($"- {current.Item1.Part.Name} [{current.Item1.Part.Id}]");
                foreach (var child in asset.EnumerateChildPartDesigns(current.Item1, asset.Hierarchy, false).Reverse())
                {
                    stack.Push(Tuple.Create(child, current.Item2 + 1));
                }
            }
            var str = sb.ToString();
            return str;
        }

        private static MyAssetPropertyGraph BuildAssetAndGraph(int rootCount, int depth, int childPerPart, Action<AssetCompositeHierarchyData<MyPartDesign, MyPart>> initializeProperties = null)
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = BuildHierarchy(rootCount,  depth,  childPerPart);
            var assetItem = new AssetItem("MyAsset", asset);
            initializeProperties?.Invoke(asset.Hierarchy);
            var graph = (MyAssetPropertyGraph)AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            return graph;
        }

        private static MyAsset BuildHierarchy(int rootCount, int depth, int childPerPart)
        {
            var asset = new MyAsset();
            var guid = 0;
            for (var i = 0; i < rootCount; ++i)
            {
                var rootPart = BuildPart(asset, $"Part{i+1}", depth - 1, childPerPart, ref guid);
                asset.Hierarchy.RootPartIds.Add(rootPart.Part.Id);
            }
            return asset;
        }

        private static MyPartDesign BuildPart(MyAsset asset, string name, int depth, int childPerPart, ref int guidCount)
        {
            var part = new MyPartDesign { Part = new MyPart { Id = GuidGenerator.Get(++guidCount), Name = name } };
            asset.Hierarchy.Parts.Add(part);
            if (depth <= 0)
                return part;

            for (var i = 0; i < childPerPart; ++i)
            {
                var child = BuildPart(asset, name + $"-{i + 1}", depth - 1, childPerPart, ref guidCount);
                part.Part.AddChild(child.Part);
            }
            return part;
        }
    }
}
