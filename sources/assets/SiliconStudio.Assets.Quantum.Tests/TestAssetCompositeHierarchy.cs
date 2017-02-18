using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchy
    {
        private class MyPart : IIdentifiable
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public MyPart Parent { get; private set; }
            public List<MyPart> Children { get; } = new List<MyPart>();
            public void AddChild(MyPart child) { Children.Add(child); child.Parent = this; }
        }

        private class MyPartDesign : IAssetPartDesign<MyPart>
        {
            public BasePart Base { get; set; }
            public MyPart Part { get; set; }
        }

        private class MyAsset : AssetCompositeHierarchy<MyPartDesign, MyPart>
        {
            public override MyPart GetParent(MyPart part) => part.Parent;
            public override int IndexOf(MyPart part) => GetParent(part)?.Children.IndexOf(part) ?? Hierarchy.RootPartIds.IndexOf(part.Id);
            public override MyPart GetChild(MyPart part, int index) => part.Children[index];
            public override int GetChildCount(MyPart part) => part.Children.Count;
            public override IEnumerable<MyPart> EnumerateChildParts(MyPart part, bool isRecursive) => isRecursive ? part.Children.DepthFirst(t => t.Children) : part.Children;
        }

        [AssetPropertyGraph(typeof(MyAsset))]
        private class MyAssetPropertyGraph : AssetCompositeHierarchyPropertyGraph<MyPartDesign, MyPart>
        {
            public MyAssetPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger) : base(container, assetItem, logger) { }
            protected override void AddChildPartToParentPart(MyPart parentPart, MyPart childPart, int index) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Add(childPart, new Index(index));
            protected override void RemoveChildPartFromParentPart(MyPart parentPart, MyPart childPart) => Container.NodeContainer.GetNode(parentPart)[nameof(MyPart.Children)].Target.Remove(childPart, new Index(parentPart.Children.IndexOf(childPart)));
        }

        [Test]
        public void TestSimpleCloneSubHierarchy()
        {
            var graph = BuildAssetAndGraph(3, 3, 3);
            Console.Write(PrintHierarchy(graph.AssetHierarchy));
        }

        private string PrintHierarchy(AssetCompositeHierarchy<MyPartDesign, MyPart> asset)
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

        private MyAssetPropertyGraph BuildAssetAndGraph(int rootCount, int depth, int childPerPart)
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = BuildHierarchy(rootCount,  depth,  childPerPart);
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = (MyAssetPropertyGraph)AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            return graph;
        }

        private MyAsset BuildHierarchy(int rootCount, int depth, int childPerPart)
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

        private MyPartDesign BuildPart(MyAsset asset, string name, int depth, int childPerPart, ref int guidCount)
        {
            var part = new MyPartDesign() { Part = new MyPart() { Id = GuidGenerator.Get(guidCount++), Name = name } };
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