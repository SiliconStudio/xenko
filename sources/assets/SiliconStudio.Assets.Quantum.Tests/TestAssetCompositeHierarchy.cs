// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchy
    {
        [Test]
        public void TestSimpleCloneSubHierarchy()
        {
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2);
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReference = x.Parts[GuidGenerator.Get(6)].Part);
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReferences = new List<Types.MyPart> { x.Parts[GuidGenerator.Get(2)].Part });
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.None, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
            Assert.AreEqual(graph.Asset.Hierarchy.Parts[GuidGenerator.Get(2)].Part, cloneRoot.Part.Children[0].MyReferences[0]);
        }

        [Test]
        public void TestSimpleCloneSubHierarchyWithCleanExternalReferences()
        {
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2);
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReference = x.Parts[GuidGenerator.Get(6)].Part);
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
            var graph = AssetHierarchyHelper.BuildAssetAndGraph(2, 2, 2, x => x.Parts[GuidGenerator.Get(5)].Part.MyReferences = new List<Types.MyPart> { x.Parts[GuidGenerator.Get(2)].Part });
            Debug.Write(AssetHierarchyHelper.PrintHierarchy(graph.Asset));
            var originalRoot = graph.Asset.Hierarchy.Parts[graph.Asset.Hierarchy.RootParts[1].Id];
            Dictionary<Guid, Guid> remapping;
            var clone = AssetCompositeHierarchyPropertyGraph<Types.MyPartDesign, Types.MyPart>.CloneSubHierarchies(graph.Container.NodeContainer, graph.Asset, originalRoot.Part.Id.Yield(), SubHierarchyCloneFlags.CleanExternalReferences, out remapping);
            var cloneRoot = clone.Parts[clone.RootParts.Single().Id];
            Assert.IsNull(remapping);
            Assert.AreEqual(3, clone.Parts.Count);
            Assert.AreEqual(1, clone.RootParts.Count);
            foreach (var part in clone.Parts)
            {
                var matchingPart = graph.Asset.Hierarchy.Parts[part.Part.Id];
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
    }
}
