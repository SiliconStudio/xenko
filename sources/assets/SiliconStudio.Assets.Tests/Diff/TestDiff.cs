// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests.Diff
{
    /// <summary>
    /// Test the <see cref="AssetDiff"/>
    /// </summary>
    [TestFixture()]
    public class TestDiff
    {
        public static readonly PropertyKey<string> StringKey = new PropertyKey<string>("StringKey", typeof(TestDiff));
        public static readonly PropertyKey<DiffComponent> ComponentKey = new PropertyKey<DiffComponent>("ComponentKey", typeof(TestDiff));
        public static readonly PropertyKey<DiffComponent> ComponentKey1 = new PropertyKey<DiffComponent>("ComponentKey1", typeof(TestDiff));

        [DataContract("TestDiffAsset")]
        [AssetDescription(".xkdiff")]
        public class TestDiffAsset : Asset
        {
            public TestDiffAsset()
            {
                Parameters = new CustomParameterCollection();
                Components = new List<DiffComponent>();
            }

            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public int Value { get; set; }

            [DataMember(2)]
            public CustomParameterCollection Parameters { get; set; }

            public List<DiffComponent> Components { get; set; }

            public AssetReference AssetReference { get; set; }
        }

        [DataContract]
        public class DiffComponent : IEquatable<DiffComponent>
        {
            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public Vector4 Position { get; set; }

            [DataMember(2)]
            public List<Vector4> Positions { get; set; }

            public bool Equals(DiffComponent other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name)
                    && Position.Equals(other.Position)
                    && ArrayExtensions.ArraysEqual(Positions, other.Positions);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((DiffComponent)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0)*397) ^ Position.GetHashCode() ^ (Positions != null ? ArrayExtensions.ComputeHash(Positions) : 0);
                }
            }

            public static bool operator ==(DiffComponent left, DiffComponent right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(DiffComponent left, DiffComponent right)
            {
                return !Equals(left, right);
            }
        }

        [Test]
        public void TestNoConflict()
        {
            var diff = NewTestDiff();
            var diff3 = diff.Compute();

            var totalNodes = diff3.FindLeafDifferences().ToList();
            Assert.AreEqual(0, totalNodes.Count);
        }

        [Test]
        public void TestAsset1Modified()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.Asset1).Value = 2;
            ((TestDiffAsset)diff.Asset1).Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitY });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();
            
            // 3 changes
            // + 1 for Asset1.Value and  
            // + 2 changesfor Asset.Parameters[ComponentKey].Position.X => from 1 to 0, and Position.Y => from 0 to 1
            Assert.AreEqual(3, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[0].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[1].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[2].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset1Node;
            Assert.AreEqual("Value", member1.MemberDescriptor.Name);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[1].Asset1Node);
            var member2 = (DataVisitMember)nodes[1].Asset1Node;
            Assert.AreEqual("X", member2.MemberDescriptor.Name);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[2].Asset1Node);
            var member3 = (DataVisitMember)nodes[2].Asset1Node;
            Assert.AreEqual("Y", member3.MemberDescriptor.Name);
        }

        [Test]
        public void TestAsset2Modified()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.Asset2).Value = 2;
            ((TestDiffAsset)diff.Asset2).Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitY });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 3 changes
            // + 1 for Asset1.Value and  
            // + 2 changesfor Asset.Parameters[ComponentKey].Position.X => from 1 to 0, and Position.Y => from 0 to 1
            Assert.AreEqual(3, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset2, nodes[0].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset2, nodes[1].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset2, nodes[2].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset2Node;
            Assert.AreEqual("Value", member1.MemberDescriptor.Name);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[1].Asset1Node);
            var member2 = (DataVisitMember)nodes[1].Asset2Node;
            Assert.AreEqual("X", member2.MemberDescriptor.Name);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[2].Asset1Node);
            var member3 = (DataVisitMember)nodes[2].Asset2Node;
            Assert.AreEqual("Y", member3.MemberDescriptor.Name);
        }

        [Test]
        public void TestConflict()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.Asset1).Value = 2;
            ((TestDiffAsset)diff.Asset1).Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitZ });
            ((TestDiffAsset)diff.Asset2).Value = 3;
            ((TestDiffAsset)diff.Asset2).Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitY });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 3 conflicts changes
            // + 1 for Asset1.Value and  
            // + 2 changesfor Asset.Parameters[ComponentKey].Position.X => from 1 to 0, and Position.Y => from 0 to 1
            Assert.AreEqual(4, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.Conflict, nodes[0].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1And2, nodes[1].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset2, nodes[2].ChangeType);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[3].ChangeType);

            // 1) Conflict - Asset1.Value changed 1 => 2 and 1 => 3
            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset2Node;
            Assert.AreEqual("Value", member1.MemberDescriptor.Name);

            // 2) MergeFromAsset1And2 - Asset1/Asset2.Parameters[ComponentKey].Position.X changed 1 => 0
            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[1].Asset1Node);
            var member2 = (DataVisitMember)nodes[1].Asset2Node;
            Assert.AreEqual("X", member2.MemberDescriptor.Name);

            // 3) MergeFromAsset2 - Asset2.Parameters[ComponentKey].Position.Y changed from 0 => 1
            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[2].Asset1Node);
            var member3 = (DataVisitMember)nodes[2].Asset2Node;
            Assert.AreEqual("Y", member3.MemberDescriptor.Name);

            // 3) FromAsset1 - Asset1.Parameters[ComponentKey].Position.Z changed from 0 => 1
            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[3].Asset1Node);
            var member4 = (DataVisitMember)nodes[3].Asset2Node;
            Assert.AreEqual("Z", member4.MemberDescriptor.Name);
        }

        [Test]
        public void MergeList()
        {
            var diff = NewTestDiff();

            var baseAsset = ((TestDiffAsset)diff.BaseAsset);
            var asset1 = ((TestDiffAsset)diff.Asset1);
            var asset2 = ((TestDiffAsset)diff.Asset2);

            baseAsset.Components.Add(new DiffComponent { Name = "comp1" });
            asset1.Components.Add(new DiffComponent { Name = "comp1" });
            asset2.Components.Add(new DiffComponent { Name = "comp1" });

            asset1.Components.Add(new DiffComponent { Name = "comp2" });

            var result = AssetMerge.Merge(diff.BaseAsset, diff.Asset1, diff.Asset2, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.IsFalse(result.HasErrors);
            var asset = (TestDiffAsset)result.Asset;
            Assert.IsNotNull(asset);

            Assert.AreEqual(2, asset.Components.Count);
        }

        [Test]
        public void MergeWithNoConflict()
        {
            var diff = NewTestDiff();

            var asset1 = ((TestDiffAsset)diff.Asset1);
            var asset2 = ((TestDiffAsset)diff.Asset2);

            asset1.Value = 2;
            asset1.Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitZ });
            asset2.Value = 3;
            asset2.Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitY, Positions = new List<Vector4> { Vector4.UnitW } });

            // -----------------------------------
            // First merge
            // -----------------------------------
            var result = AssetMerge.Merge(diff.BaseAsset, diff.Asset1, diff.Asset2, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.IsFalse(result.HasErrors);
            var asset = (TestDiffAsset)result.Asset;
            Assert.IsNotNull(asset);

            // Check merge value: Value is changing (base: 1, v1: 2, v2: 3) but we are assuming that v1 is the actual value
            Assert.AreEqual(2, asset.Value);
            Assert.IsTrue(asset.Parameters.ContainsKey(ComponentKey));
            Assert.AreEqual(new DiffComponent() { Name = "comp1", Position = new Vector4(0, 1, 1, 0), Positions = new List<Vector4> { Vector4.UnitW } }, asset.Parameters[ComponentKey]);  // <= Merge of UnitZ and UnitY => (0, 1, 1, 0)

            // -----------------------------------
            // 2nd merge : Add a new key and a new item in Asset2
            // -----------------------------------
            asset2.Parameters.Set(ComponentKey1, new DiffComponent() { Name = "newKeyFrom2", Position = Vector4.UnitW });
            asset2.Components.Add(new DiffComponent() { Name = "newFrom2", Position = Vector4.UnitX });

            result = AssetMerge.Merge(diff.BaseAsset, asset1, asset2, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);

            Assert.IsFalse(result.HasErrors);
            asset = (TestDiffAsset)result.Asset;
            Assert.IsNotNull(asset);

            // Check merge values
            Assert.AreEqual(2, asset.Value);
            Assert.IsTrue(asset.Parameters.ContainsKey(ComponentKey));
            Assert.IsTrue(asset.Parameters.ContainsKey(ComponentKey1));
            Assert.AreEqual(new DiffComponent() { Name = "comp1", Position = new Vector4(0, 1, 1, 0), Positions = new List<Vector4> { Vector4.UnitW } }, asset.Parameters[ComponentKey]);  // <= Merge of UnitZ and UnitY => (0, 1, 1, 0)
            Assert.AreEqual(new DiffComponent() { Name = "newKeyFrom2", Position = Vector4.UnitW }, asset.Parameters[ComponentKey1]);
            Assert.AreEqual(1, asset.Components.Count);
            Assert.AreEqual(new DiffComponent() { Name = "newFrom2", Position = Vector4.UnitX }, asset.Components[0]);
        }

        [Test]
        public void TestAsset1ListModified()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.Asset1).Components.Add(new DiffComponent() { Name = "item1", Position = Vector4.UnitX });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 1 change MergeFromAsset1
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitListItem), nodes[0].Asset1Node);
            var item1 = (DataVisitListItem)nodes[0].Asset1Node;
            Assert.IsInstanceOf<DiffComponent>(item1.Instance);
        }

        [Test]
        public void TestAsset1ItemInListModified()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.BaseAsset).Components.Add(new DiffComponent() { Name = "item1", Position = Vector4.UnitX });
            ((TestDiffAsset)diff.Asset1).Components.Add(new DiffComponent() { Name = "item1", Position = Vector4.UnitY });
            ((TestDiffAsset)diff.Asset2).Components.Add(new DiffComponent() { Name = "item1", Position = Vector4.UnitX });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 1 change MergeFromAsset1:
            //  DiffComponent
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitListItem), nodes[0].Asset1Node);
            var item1 = (DataVisitListItem)nodes[0].Asset1Node;
            Assert.AreEqual(0, item1.Index);
        }

        [Test]
        public void TestListConflict()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.BaseAsset).Components.Add(new DiffComponent() { Name = "item1", Position = Vector4.UnitX });
            ((TestDiffAsset)diff.Asset1).Components.Add(new DiffComponent() { Name = "item1", Position = new Vector4(2, 0, 0, 0) });
            ((TestDiffAsset)diff.Asset2).Components.Add(new DiffComponent() { Name = "item1", Position = new Vector4(3, 0, 0, 0) });
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 1 conflict 
            //  DiffComponent.X => 1, 2, 3
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.Conflict, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset1Node;
            Assert.AreEqual("X", member1.MemberDescriptor.Name);
        }

        [Test]
        public void TestMergeWithAssetReference()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.Asset1).AssetReference = new AssetReference(Guid.NewGuid(), new UFile("/a"));
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 1 merge
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset1Node;
            Assert.AreEqual("AssetReference", member1.MemberDescriptor.Name);
        }

        [Test]
        public void TestConflictWithAssetReference()
        {
            var diff = NewTestDiff();
            ((TestDiffAsset)diff.BaseAsset).AssetReference = new AssetReference(Guid.Empty, new UFile("/a"));
            ((TestDiffAsset)diff.Asset1).AssetReference = new AssetReference(Guid.NewGuid(), new UFile("/a"));
            ((TestDiffAsset)diff.Asset2).AssetReference = new AssetReference(Guid.Empty, new UFile("/a"));
            var diff3 = diff.Compute();

            var nodes = diff3.FindLeafDifferences().ToList();

            // 1 merge
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset1, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            var member1 = (DataVisitMember)nodes[0].Asset1Node;
            Assert.AreEqual("AssetReference", member1.MemberDescriptor.Name);

            ((TestDiffAsset)diff.Asset2).AssetReference = new AssetReference(Guid.NewGuid(), new UFile("/a"));
            diff.Reset();
            diff3 = diff.Compute();
            nodes = diff3.FindLeafDifferences().ToList();

            // 1 conflict
            Assert.AreEqual(1, nodes.Count);

            // Check that change type is from asset1
            Assert.AreEqual(Diff3ChangeType.Conflict, nodes[0].ChangeType);

            Assert.IsInstanceOf(typeof(DataVisitMember), nodes[0].Asset1Node);
            member1 = (DataVisitMember)nodes[0].Asset1Node;
            Assert.AreEqual("AssetReference", member1.MemberDescriptor.Name);
        }

        /// <summary>
        /// Test diff using <see cref="AssetDiff.UseOverrideMode"/>. Check that lists with ids are correctly handled
        /// </summary>
        [Test]
        public void TestOverrideListDiffWithIdsNoConflicts()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            baseAsset.Components.Add(new DiffComponent() { Name=  "Test1", Position = new Vector4(1, 0, 0, 0)});
            baseAsset.Components.Add(new DiffComponent() { Name = "Test2", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.Components.Add(new DiffComponent() { Name = "Test3", Position = new Vector4(1, 0, 0, 0) });

            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);

            var baseItem = new AssetItem("/base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffList = diff.Compute();

            var diffListStripped = diffList.FindLeafDifferences().ToList();

            // Expecting only 2 diff from TestDiffAsset (3 properties + BuildOrder)
            Assert.AreEqual(4, diffListStripped.Where(item => item.BaseNode.Parent?.Instance is TestDiffAsset).Count());

            // Expecting 6 diffs for DiffComponent (3 elements, 5 properties (Name + Vector4))
            Assert.AreEqual(3 * 5, diffListStripped.Where(item => item.BaseNode.Parent?.Instance is DiffComponent || item.BaseNode.Parent?.Parent?.Instance is DiffComponent).Count());

            // All changes must be from asset2 (considered as new base), as everything is setting base
            Assert.True(diffListStripped.All(item => item.ChangeType == Diff3ChangeType.MergeFromAsset2));

            foreach (var node in diffListStripped.Where(item => item.BaseNode.Parent.Instance is DiffComponent))
            {
                var base1 = (DiffComponent)node.BaseNode.Parent.Instance;
                var asset1 = (DiffComponent)node.Asset1Node.Parent.Instance;
                var asset2 = (DiffComponent)node.Asset2Node.Parent.Instance;

                var baseIndex = baseAsset.Components.IndexOf(base1);
                var asset1Index = childAsset.Components.IndexOf(asset1);
                var asset2Index = newBaseAsset.Components.IndexOf(asset2);

                Assert.AreEqual(baseIndex, asset1Index);
                Assert.AreEqual(baseIndex, asset2Index);
            }
        }


        /// <summary>
        /// Test diff using <see cref="AssetDiff.UseOverrideMode"/>. In this test:
        /// - Change order of items in the list in the new asset.
        /// - Make one property of an item to be "New"
        /// </summary>
        [Test]
        public void TestOverrideListDiffWithIdsOrderChanged()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            baseAsset.Components.Add(new DiffComponent() { Name = "Test1", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.Components.Add(new DiffComponent() { Name = "Test2", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.Components.Add(new DiffComponent() { Name = "Test3", Position = new Vector4(1, 0, 0, 0) });

            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);

            var baseItem = new AssetItem("/base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            // Swap elements in child asset
            var temp = childAsset.Components[0];
            childAsset.Components[0] = childAsset.Components[1];
            childAsset.Components[1] = temp;
            childAsset.Components[0].Name = "Test21";

            // Make New on Name value on first element
            var objDesc = TypeDescriptorFactory.Default.Find(typeof(DiffComponent));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");
            childAsset.Components[0].SetOverride(memberDesc, OverrideType.New);

            // Perform the diff
            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffList = diff.Compute();

            var diffListStripped = diffList.FindLeafDifferences().ToList();

            // Expecting only one field to be merged from asset1 (the new on Name property)
            var mergeFromAsset1 = diffListStripped.Where(item => item.ChangeType == Diff3ChangeType.MergeFromAsset1).ToList();
            Assert.AreEqual(1, mergeFromAsset1.Count);
            var nameMember = mergeFromAsset1[0].Asset1Node as DataVisitMember;
            Assert.NotNull(nameMember);
            Assert.AreEqual("Name", nameMember.MemberDescriptor.Name);

            // Check that DiffComponent are swapped for Asset1 but diff is able to recover this
            foreach (var node in diffListStripped.Where(item => item.BaseNode.Parent.Instance is DiffComponent))
            {
                var base1 = (DiffComponent)node.BaseNode.Parent.Instance;
                var asset1 = (DiffComponent)node.Asset1Node.Parent.Instance;
                var asset2 = (DiffComponent)node.Asset2Node.Parent.Instance;

                var baseIndex = baseAsset.Components.IndexOf(base1);
                var asset1Index = childAsset.Components.IndexOf(asset1);
                var asset2Index = newBaseAsset.Components.IndexOf(asset2);

                Assert.AreEqual(baseIndex, asset2Index);
                switch (baseIndex)
                {
                    // element 0 and 1 are swapped
                    case 0:
                        Assert.AreEqual(1, asset1Index);
                        break;
                    case 1:
                        Assert.AreEqual(0, asset1Index);
                        break;
                    default:
                        Assert.AreEqual(baseIndex, asset1Index);
                        break;
                }
            }
        }

        private AssetDiff NewTestDiff()
        {
            var baseValue = new TestDiffAsset { Name = "base", Value = 1 };
            baseValue.Parameters.Set(StringKey, "string");
            baseValue.Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitX });

            var asset1Value = new TestDiffAsset { Name = "base", Value = 1 };
            asset1Value.Base = new AssetBase("/tata", baseValue);
            asset1Value.Parameters.Set(StringKey, "string");
            asset1Value.Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitX });

            var asset2Value = new TestDiffAsset { Name = "base", Value = 1 };
            asset2Value.Parameters.Set(StringKey, "string");
            asset2Value.Parameters.Set(ComponentKey, new DiffComponent() { Name = "comp1", Position = Vector4.UnitX });

            // Copy Id from base
            asset1Value.Id = baseValue.Id;
            asset2Value.Id = baseValue.Id;

            return new AssetDiff(baseValue, asset1Value, asset2Value);
        }
    }
}