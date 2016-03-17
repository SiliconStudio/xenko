// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests.Diff
{
    /// <summary>
    /// Test the <see cref="AssetDiff"/> using <see cref="AssetDiff.UseOverrideMode"/>
    /// </summary>
    [TestFixture()]
    public class TestDiffTemplatingAndMerge
    {
        [DataContract()]
        [AssetDescription(".xkdiff2")]
        public class TestDiffAsset : Asset
        {
            public TestDiffAsset()
            {
                List = new List<DiffComponent>();
            }

            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public int Value { get; set; }

            [DataMember(2)]
            [DefaultValue(null)]
            public IDiffValue Dynamic { get; set; }

            [DataMember(3)]

            public List<DiffComponent> List { get; set; }
        }

        public interface IDiffValue
        {
        }

        [DataContract]
        public class DiffValueTypeA : IDiffValue
        {
            public string Text { get; set; }

            public string Text2 { get; set; }

            public string Text3 { get; set; }
        }

        [DataContract]
        public class DiffValueTypeB : IDiffValue
        {
            public int Value { get; set; }
        }

        [DataContract]
        public class DiffComponent
        {
            public static readonly PropertyKey<DiffComponent> Key = new PropertyKey<DiffComponent>("Key", typeof(DiffComponent));

            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public Vector4 Position { get; set; }
        }

        [DataContract]
        public class DiffComponentSub : DiffComponent
        {
            public int Value { get; set; }
        }

        [DataContract]
        public class DictionaryContainer
        {
            public DictionaryContainer()
            {
                Items = new Dictionary<string, string>();
            }

            public Dictionary<string, string> Items;
        }

        [DataContract]
        public class ObjectWithPropertyContainer
        {
            public PropertyContainer Items;

            public List<Guid> Ids;
        }

        [Test]
        public void TestNoChanges()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1, Dynamic = new DiffValueTypeA() { Text = "Test1" } };
            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);
            var baseItem = new AssetItem("/base", baseAsset);
            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            var diffResultStripped = diffResult.FindLeafDifferences().ToList();

            // 6: BuildOrder+Name+Value+(Dynamic: Text+Text2+Text3)
            Assert.AreEqual(6, diffResultStripped.Count);

            // Check that everything is merging from asset2
            Assert.True(diffResultStripped.All(node => node.ChangeType == Diff3ChangeType.MergeFromAsset2));
        }

        [Test]
        public void TestWithNewTypeFromNewBase()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1, Dynamic = new DiffValueTypeA() { Text = "Test1"} };
            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.Dynamic = new DiffValueTypeB() { Value = 1 };

            var baseItem = new AssetItem("/base", baseAsset);
            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            var diffResultStripped = diffResult.FindLeafDifferences().ToList();

            // Check that everything is merging from asset2
            Assert.True(diffResultStripped.All(node => node.ChangeType == Diff3ChangeType.MergeFromAsset2));

            // Check that merged result on Dynamic property is instance from asset2
            var mergeResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.True(childAsset.Dynamic is DiffValueTypeB);
            Assert.AreEqual(1, ((DiffValueTypeB)childAsset.Dynamic).Value);
        }

        [Test]
        public void TestWithNewTypeFromChild()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1, Dynamic = new DiffValueTypeA() { Text = "Test1" } };
            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);

            var baseItem = new AssetItem("/base", baseAsset);
            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            // Make New on Name value on first element
            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestDiffAsset));
            var memberDesc = objDesc.Members.First(t => t.Name == "Dynamic");
            childAsset.SetOverride(memberDesc, OverrideType.New); // Override Dynamic and change type
            childAsset.Dynamic = new DiffValueTypeB() { Value = 2 };

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            var diffResultStripped = diffResult.FindLeafDifferences().ToList();

            // Check at least one field has merge from asset 1 (Dynamic)
            Assert.True(diffResultStripped.Any(node => node.ChangeType == Diff3ChangeType.MergeFromAsset1));

            // Check that merged result on Dynamic property is instance from asset2
            var mergeResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.True(childAsset.Dynamic is DiffValueTypeB);
            Assert.AreEqual(2, ((DiffValueTypeB)childAsset.Dynamic).Value);
        }

        [Test]
        public void TestChangeOverrideToBaseSealed()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.Value = 3;

            var baseItem = new AssetItem("/base", baseAsset);
            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            // Change base: Name to Base|Sealed
            // This should result into a reset of the value overriden in child

            // Make New on Name value on first element
            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestDiffAsset));
            var memberDesc = objDesc.Members.First(t => t.Name == "Value");
            newBaseAsset.SetOverride(memberDesc, OverrideType.Base|OverrideType.Sealed);

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            // Check that merged result on Dynamic property is instance from asset2
            var mergeResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.AreEqual(3, childAsset.Value); // Value is coming from base
            Assert.AreEqual(OverrideType.Base|OverrideType.Sealed, childAsset.GetOverride(memberDesc)); // Value is coming from base
        }

        /// <summary>
        /// Test diff using <see cref="AssetDiff.UseOverrideMode"/>. Check that lists with ids are correctly handled
        /// </summary>
        [Test]
        public void TestListWithIdsNoConflicts()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            baseAsset.List.Add(new DiffComponent() { Name=  "Test1", Position = new Vector4(1, 0, 0, 0)});
            baseAsset.List.Add(new DiffComponent() { Name = "Test2", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.List.Add(new DiffComponent() { Name = "Test3", Position = new Vector4(1, 0, 0, 0) });

            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);

            var baseItem = new AssetItem("/base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            var diffResultStripped = diffResult.FindLeafDifferences().ToList();

            // Expecting only 2 diff from TestDiffAsset (3 properties + BuildOrder)
            Assert.AreEqual(4, diffResultStripped.Where(item => item.BaseNode.Parent?.Instance is TestDiffAsset).Count());

            // Expecting 6 diffs for DiffComponent (3 elements, 5 properties (Name + Position))
            Assert.AreEqual(3 * 2, diffResultStripped.Where(item => item.BaseNode.Parent?.Instance is DiffComponent || item.BaseNode.Parent?.Parent?.Instance is DiffComponent).Count());

            // All changes must be from asset2 (considered as new base), as everything is setting base
            Assert.True(diffResultStripped.All(item => item.ChangeType == Diff3ChangeType.MergeFromAsset2));

            foreach (var node in diffResultStripped.Where(item => item.BaseNode.Parent.Instance is DiffComponent))
            {
                var base1 = (DiffComponent)node.BaseNode.Parent.Instance;
                var asset1 = (DiffComponent)node.Asset1Node.Parent.Instance;
                var asset2 = (DiffComponent)node.Asset2Node.Parent.Instance;

                var baseIndex = baseAsset.List.IndexOf(base1);
                var asset1Index = childAsset.List.IndexOf(asset1);
                var asset2Index = newBaseAsset.List.IndexOf(asset2);

                Assert.AreEqual(baseIndex, asset1Index);
                Assert.AreEqual(baseIndex, asset2Index);
            }
        }


        /// <summary>
        /// Change the type of one item in the list from the new base asset
        /// </summary>
        [Test]
        public void TestListWithIdsChangeType()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            baseAsset.List.Add(new DiffComponent() { Name = "Test1", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.List.Add(new DiffComponent() { Name = "Test2", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.List.Add(new DiffComponent() { Name = "Test3", Position = new Vector4(1, 0, 0, 0) });

            // Change type of 2nd element in newBase list
            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.List[1] = new DiffComponentSub() { Value = 1 };

            var baseItem = new AssetItem("/base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };
            var diffResult = diff.Compute();

            var diffList = diffResult.Members.First(node => ((DataVisitMember)node.Asset1Node).MemberDescriptor.Name == "List");

            // Check that we have only 3 items
            Assert.AreEqual(3, diffList.Items.Count);
            Assert.AreEqual(Diff3ChangeType.MergeFromAsset2, diffList.Items[1].ChangeType);

            var mergeResult = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.False(mergeResult.HasErrors);

            Assert.AreEqual(3, childAsset.List.Count);
            Assert.AreEqual("Test1", childAsset.List[0].Name);
            Assert.True(childAsset.List[1] is DiffComponentSub);
            Assert.AreEqual("Test3", childAsset.List[2].Name);
        }

        /// <summary>
        /// Test diff using <see cref="AssetDiff.UseOverrideMode"/>. In this test:
        /// - Change order of items in the list in the new asset.
        /// - Make one property of an item to be "New"
        /// </summary>
        [Test]
        public void TestListDiffWithIdsOrderChanged()
        {
            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            baseAsset.List.Add(new DiffComponent() { Name = "Test1", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.List.Add(new DiffComponent() { Name = "Test2", Position = new Vector4(1, 0, 0, 0) });
            baseAsset.List.Add(new DiffComponent() { Name = "Test3", Position = new Vector4(1, 0, 0, 0) });

            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);

            var baseItem = new AssetItem("/base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();

            // Swap elements in child asset
            var temp = childAsset.List[0];
            childAsset.List[0] = childAsset.List[1];
            childAsset.List[1] = temp;
            childAsset.List[0].Name = "Test21";

            // Make New on Name value on first element
            var objDesc = TypeDescriptorFactory.Default.Find(typeof(DiffComponent));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");
            childAsset.List[0].SetOverride(memberDesc, OverrideType.New);

            // Perform the diff
            var diff = new AssetDiff(baseAsset, childAsset, newBaseAsset) { UseOverrideMode = true };

            var diffResult = diff.Compute();

            var diffResultStripped = diffResult.FindLeafDifferences().ToList();

            // Expecting only one field to be merged from asset1 (the new on Name property)
            var mergeFromAsset1 = diffResultStripped.Where(item => item.ChangeType == Diff3ChangeType.MergeFromAsset1).ToList();
            Assert.AreEqual(1, mergeFromAsset1.Count);
            var nameMember = mergeFromAsset1[0].Asset1Node as DataVisitMember;
            Assert.NotNull(nameMember);
            Assert.AreEqual("Name", nameMember.MemberDescriptor.Name);

            // Check that DiffComponent are swapped for Asset1 but diff is able to recover this
            foreach (var node in diffResultStripped.Where(item => item.BaseNode.Parent.Instance is DiffComponent))
            {
                var base1 = (DiffComponent)node.BaseNode.Parent.Instance;
                var asset1 = (DiffComponent)node.Asset1Node.Parent.Instance;
                var asset2 = (DiffComponent)node.Asset2Node.Parent.Instance;

                var baseIndex = baseAsset.List.IndexOf(base1);
                var asset1Index = childAsset.List.IndexOf(asset1);
                var asset2Index = newBaseAsset.List.IndexOf(asset2);

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

        [Test]
        public void TestMergeListGuidsWithSwapItems()
        {
            var item0 = new Guid("9a656db2-d387-4805-a18d-7727d26c0a7a");
            var item1 = new Guid("3d22a49d-d891-451f-8e2d-f7cabb11a602");
            var item2 = new Guid("3a0c78e7-a961-48ac-870f-3a8cdc6b2c4b");
            var newItem = new Guid("481331cc-b3ea-4d48-bdb6-f7741d853eaf");

            var baseList = new List<Guid>()
            {
                item0,
                item1,
                item2,
            };

            var asset1List = new List<Guid>()
            {
                item0,
                item1,
                item2,
                newItem, // new Item from 1
            };

            var asset2List = new List<Guid>()
            {
                // new Guid("9a656db2-d387-4805-a18d-7727d26c0a7a"), Item deleted
                item2, // Item[2] -> Item[0]
                item1, // Item[1] -> Item[1]
            };


            // Final list must be: item2, item1, newItem
            var diff = new AssetDiff(AssetCloner.Clone(baseList), asset1List, AssetCloner.Clone(asset2List)) { UseOverrideMode = true };

            var result = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.False(result.HasErrors);

            Assert.AreEqual(3, asset1List.Count);

            Assert.AreEqual(item2, asset1List[0]);
            Assert.AreEqual(item1, asset1List[1]);
            Assert.AreEqual(newItem, asset1List[2]);
        }

        [Test]
        public void TestMergeListGuids2()
        {
            var item0 = new Guid("9a656db2-d387-4805-a18d-7727d26c0a7a");
            var item1 = new Guid("3d22a49d-d891-451f-8e2d-f7cabb11a602");
            var item2 = new Guid("3a0c78e7-a961-48ac-870f-3a8cdc6b2c4b");
            var newItem = new Guid("481331cc-b3ea-4d48-bdb6-f7741d853eaf");

            var baseList = new List<Guid>()
            {
                item0,
                item1,
                item2,
            };

            var asset1List = new List<Guid>()
            {
                item0,
                item1,
                item2,
                newItem,
            };

            var asset2List = new List<Guid>()
            {
                item0,
                item2, 
            };


            // Final list must be: item0, item2, newItem
            var diff = new AssetDiff(AssetCloner.Clone(baseList), asset1List, AssetCloner.Clone(asset2List)) { UseOverrideMode = true };

            var result = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.False(result.HasErrors);

            Assert.AreEqual(3, asset1List.Count);

            Assert.AreEqual(item0, asset1List[0]);
            Assert.AreEqual(item2, asset1List[1]);
            Assert.AreEqual(newItem, asset1List[2]);
        }


        [Test]
        public void TestMergeDictionaryNewKeyValue()
        {
            var baseDic = new DictionaryContainer()
            {
                Items = new Dictionary<string, string>()
                {
                    { "A", "AValue" },
                    { "B", "BValue" },
                    { "C", "CValue" },
                }
            };

            var newDic = new DictionaryContainer()
            {
                Items = new Dictionary<string, string>(baseDic.Items)
            };

            var newBaseDic = new DictionaryContainer()
            {
                Items = new Dictionary<string, string>()
                {
                    { "A", "AValue" },
                    { "B", "BValue" },
                    { "C", "CValue" },
                    { "D", "DValue" },
                }
            };

            var diff = new AssetDiff(AssetCloner.Clone(baseDic), newDic, AssetCloner.Clone(newBaseDic)) { UseOverrideMode = true };

            var result = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.False(result.HasErrors);

            Assert.AreEqual(4, newDic.Items.Count);
        }

        [Test]
        public void TestMergePropertyContainer()
        {
            var baseDic = new ObjectWithPropertyContainer();

            var newDic = new ObjectWithPropertyContainer();

            var newBaseDic = new ObjectWithPropertyContainer()
            {
                Items = new PropertyContainer()
                {
                    { DiffComponent.Key, new DiffComponent() { Name = "NewComponent"} },
                }
            };

            var diff = new AssetDiff(AssetCloner.Clone(baseDic), newDic, AssetCloner.Clone(newBaseDic)) { UseOverrideMode = true };

            var result = AssetMerge.Merge(diff, AssetMergePolicies.MergePolicyAsset2AsNewBaseOfAsset1);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newDic.Items.Count);
        }

        [Test]
        public void TestPackageAnalysis()
        {
            var package = new Package();

            var baseAsset = new TestDiffAsset() { Name = "Red", Value = 1 };
            var baseItem = new AssetItem("base", baseAsset);

            var childAsset = (TestDiffAsset)baseItem.CreateChildAsset();
            var childItem = new AssetItem("child", childAsset);

            var newBaseAsset = (TestDiffAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.Name = "Green";
            var newBaseItem = new AssetItem("base", newBaseAsset);

            package.Assets.Add(newBaseItem);
            package.Assets.Add(childItem);

            var session = new PackageSession();
            session.Packages.Add(package);

            var result = new LoggerResult();
            var analysis = new PackageAssetTemplatingAnalysis(package, result);
            analysis.Run();

            Assert.False(result.HasErrors);

            var assetModified = (TestDiffAsset)package.Assets.Find("child").Asset;
            Assert.AreEqual("Green", assetModified.Name);
        }
    }
}