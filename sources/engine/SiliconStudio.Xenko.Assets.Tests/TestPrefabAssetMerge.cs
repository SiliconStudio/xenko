// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Tests
{
    [DataContract("TestEntityComponent")]
    public sealed class TestEntityComponent : EntityComponent
    {
        public Entity EntityLink { get; set; }

        public EntityComponent EntityComponentLink { get; set; }
    }

    [TestFixture]
    public class TestPrefabAssetMerge
    {
        // TODO: Some tests are quite long. It woult be better to develop smaller test cases on particular cases
        // Bigger tests are still necessary for ensuring cascading updates are working 
        // and when things are less trivial (multiple children...etc.)

        [Test]
        public void TestCreateChildAsset()
        {
            // Create an Entity child asset

            // base: EA, EB, EC
            // newAsset: EA'(base: EA), EB'(base: EB), EC'(base: EC)

            var entityA = new Entity() { Name = "A" };
            var entityB = new Entity() { Name = "B" };
            var entityC = new Entity() { Name = "C" };

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityB));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityC));
            baseAsset.Hierarchy.RootPartIds.Add(entityA.Id);
            baseAsset.Hierarchy.RootPartIds.Add(entityB.Id);
            baseAsset.Hierarchy.RootPartIds.Add(entityC.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // On a derive asset all entities must have a base value and base must come from baseAsset
            Assert.True(newAsset.Hierarchy.Parts.All(item => item.BaseId.HasValue && baseAsset.Hierarchy.Parts.ContainsKey(item.BaseId.Value)));

            // Verify that we have exactly the same number of entities
            Assert.AreEqual(baseAsset.Hierarchy.RootPartIds.Count, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseAsset.Hierarchy.Parts.Count, newAsset.Hierarchy.Parts.Count);

            // Verify that baseId and newId is correctly setup
            var entityAInNew = newAsset.Hierarchy.Parts.FirstOrDefault(item => item.BaseId.Value == entityA.Id && item.Entity.Id != item.BaseId.Value);
            Assert.NotNull(entityAInNew);

            var entityBInNew = newAsset.Hierarchy.Parts.FirstOrDefault(item => item.BaseId.Value == entityB.Id && item.Entity.Id != item.BaseId.Value);
            Assert.NotNull(entityBInNew);

            var entityCInNew = newAsset.Hierarchy.Parts.FirstOrDefault(item => item.BaseId.Value == entityC.Id && item.Entity.Id != item.BaseId.Value);
            Assert.NotNull(entityCInNew);

            // Verify that RootEntities are also correctly mapped
            Assert.AreEqual(entityAInNew.Entity.Id, newAsset.Hierarchy.RootPartIds[0]);
            Assert.AreEqual(entityBInNew.Entity.Id, newAsset.Hierarchy.RootPartIds[1]);
            Assert.AreEqual(entityCInNew.Entity.Id, newAsset.Hierarchy.RootPartIds[2]);
        }

        [Test]
        public void TestMergeSimpleHierarchy()
        {
            // Test merging a simple Entity Asset that has 3 entities
            //
            // base: EA, EB, EC
            // newBase: EA, EB, EC, ED
            // newAsset: EA, EB, EC
            //
            // Result Merge: EA, EB, EC, ED

            var entityA = new Entity() { Name = "A" };
            var entityB = new Entity() { Name = "B" };
            var entityC = new Entity() { Name = "C" };

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityB));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(entityC));
            baseAsset.Hierarchy.RootPartIds.Add(entityA.Id);
            baseAsset.Hierarchy.RootPartIds.Add(entityB.Id);
            baseAsset.Hierarchy.RootPartIds.Add(entityC.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var entityD = new Entity() { Name = "D" };
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.Hierarchy.Parts.Add(new EntityDesign(entityD));
            newBaseAsset.Hierarchy.RootPartIds.Add(entityD.Id);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityHierarchyAssetBase)AssetCloner.Clone(baseAsset), (EntityHierarchyAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            // Both root and entities must be the same
            Assert.AreEqual(4, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Parts.Count);

            // All entities must have a base value
            Assert.True(newAsset.Hierarchy.Parts.All(item => item.BaseId.HasValue));

            var entityAInNewAsset = newAsset.Hierarchy.Parts.Where(item => item.BaseId.Value == entityA.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityAInNewAsset);
            var entityBInNewAsset = newAsset.Hierarchy.Parts.Where(item => item.BaseId.Value == entityB.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityBInNewAsset);
            var entityCInNewAsset = newAsset.Hierarchy.Parts.Where(item => item.BaseId.Value == entityC.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityCInNewAsset);

            var entityDInNewAsset = newAsset.Hierarchy.Parts.Where(item => item.BaseId.Value == entityD.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityDInNewAsset);

            // Hierarchy must be: EA, EB, EC, ED
            Assert.AreEqual(entityAInNewAsset.Id, newAsset.Hierarchy.RootPartIds[0]);
            Assert.AreEqual(entityBInNewAsset.Id, newAsset.Hierarchy.RootPartIds[1]);
            Assert.AreEqual(entityCInNewAsset.Id, newAsset.Hierarchy.RootPartIds[2]);
            Assert.AreEqual(entityDInNewAsset.Id, newAsset.Hierarchy.RootPartIds[3]);
        }

        [Test]
        public void TestMergeEntityWithChildren()
        {
            // Test merging an PrefabAsset with a root entity EA, and 3 child entities
            // - Add a child entity to NewBase
            // - Remove a child entity from NewAsset
            //
            //       Base         NewBase       NewAsset                  NewAsset (Merged)
            // 
            //       EA           EA            EA'(base: EA)             EA'(base: EA)
            //        |-EA1       |-EA1         |-EA1'(base: EA1)         |-EA1'(base: EA1)
            //        |-EA2       |-EA2         |                         |
            //        |-EA3       |-EA3         |-EA3'(base: EA3)         |-EA3'(base: EA3)
            //                    |-EA4                                   |-EA4'(base: EA4)
            //

            var eA = new Entity() { Name = "A" };
            var eA1 = new Entity() { Name = "A1" };
            var eA2 = new Entity() { Name = "A2" };
            var eA3 = new Entity() { Name = "A3" };
            eA.Transform.Children.Add(eA1.Transform);
            eA.Transform.Children.Add(eA2.Transform);
            eA.Transform.Children.Add(eA3.Transform);

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA1));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA2));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA3));
            baseAsset.Hierarchy.RootPartIds.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA2FromNewBase = newBaseAsset.Hierarchy.Parts.First(item => item.Entity.Id == eA2.Id);
            newBaseAsset.Hierarchy.Parts[eA.Id].Entity.Transform.Children.Remove(eA2FromNewBase.Entity.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();
            var eA4 = new Entity() { Name = "A4" };
            newAsset.Hierarchy.Parts.Add(new EntityDesign(eA4));
            newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()].Entity.Transform.Children.Add(eA4.Transform);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityHierarchyAssetBase)AssetCloner.Clone(baseAsset), (EntityHierarchyAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Parts.Count); // EA, EA1', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()];

            Assert.AreEqual(3, rootEntity.Entity.Transform.Children.Count);

            Assert.AreEqual("A1", rootEntity.Entity.Transform.Children[0].Entity.Name);
            Assert.AreEqual("A3", rootEntity.Entity.Transform.Children[1].Entity.Name);
            Assert.AreEqual("A4", rootEntity.Entity.Transform.Children[2].Entity.Name);
        }

        [Test]
        public void TestMergeAddEntityWithLinks()
        {
            // Test merging an PrefabAsset with a root entity EA, and 3 child entities
            // - Add a child entity to NewBase that has a link to an another entity + a link to the component of another entity
            //
            //       Base         NewBase                      NewAsset                  NewAsset (Merged)
            //                                                 
            //       EA           EA                           EA'(base: EA)             EA'(base: EA)
            //        |-EA1       |-EA1                        |-EA1'(base: EA1)         |-EA1'(base: EA1)
            //        |-EA2       |-EA2                        |-EA2'(base: EA2)         |-EA2'(base: EA2)
            //        |-EA3       |-EA3                        |-EA3'(base: EA3)         |-EA3'(base: EA3)
            //                    |-EA4 + link EA1 + link EA2                            |-EA4'(base: EA4) + link EA1' + link EA2'
            //

            var eA = new Entity() { Name = "A" };
            var eA1 = new Entity() { Name = "A1" };
            var eA2 = new Entity() { Name = "A2" };
            var eA3 = new Entity() { Name = "A3" };
            eA.Transform.Children.Add(eA1.Transform);
            eA.Transform.Children.Add(eA2.Transform);
            eA.Transform.Children.Add(eA3.Transform);

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA1));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA2));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA3));
            baseAsset.Hierarchy.RootPartIds.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA4 = new Entity() { Name = "A4" };
            var rootInNewBase = newBaseAsset.Hierarchy.Parts[newBaseAsset.Hierarchy.RootPartIds.First()];
            var eA1InNewBaseTransform = rootInNewBase.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Id == eA1.Id);
            Assert.NotNull(eA1InNewBaseTransform);

            var eA2InNewBaseTransform = rootInNewBase.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Id == eA2.Id);
            Assert.NotNull(eA2InNewBaseTransform);

            // Add EA4 with link to EA1 entity and EA2 component
            var testComponent = new TestEntityComponent
            {
                EntityLink = eA1InNewBaseTransform.Entity,
                EntityComponentLink = eA2InNewBaseTransform
            };

            eA4.Add(testComponent);
            newBaseAsset.Hierarchy.Parts.Add(new EntityDesign(eA4));
            rootInNewBase.Entity.Transform.Children.Add(eA4.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityHierarchyAssetBase)AssetCloner.Clone(baseAsset), (EntityHierarchyAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(5, newAsset.Hierarchy.Parts.Count); // EA, EA1', EA2', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()];

            Assert.AreEqual(4, rootEntity.Entity.Transform.Children.Count);

            var eA1Merged = rootEntity.Entity.Transform.Children[0].Entity;
            var eA2Merged = rootEntity.Entity.Transform.Children[1].Entity;
            var eA4Merged = rootEntity.Entity.Transform.Children[3].Entity;
            Assert.AreEqual("A1", eA1Merged.Name);
            Assert.AreEqual("A2", eA2Merged.Name);
            Assert.AreEqual("A3", rootEntity.Entity.Transform.Children[2].Entity.Name);
            Assert.AreEqual("A4", eA4Merged.Name);

            var testComponentMerged = eA4Merged.Get<TestEntityComponent>();
            Assert.NotNull(testComponentMerged);

            Assert.AreEqual(eA1Merged, testComponentMerged.EntityLink);
            Assert.AreEqual(eA2Merged.Transform, testComponentMerged.EntityComponentLink);
        }

        [Test]
        public void TestMergeRemoveEntityWithLinks()
        {
            // Test merging an PrefabAsset with a root entity EA, and 3 child entities
            // - Remove a child entity from NewBase (EA2)
            // - Add a child entity (EA4) to NewBase that has a link to the EA2 entity
            //
            //       Base         NewBase     NewAsset                         NewAsset (Merged)
            //                                                                 
            //       EA           EA          EA'(base: EA)                    EA'(base: EA)
            //        |-EA1       |-EA1       |-EA1'(base: EA1)                |-EA1'(base: EA1)
            //        |-EA2       |           |-EA2'(base: EA2)                |
            //        |-EA3       |-EA3       |-EA3'(base: EA3)                |-EA3'(base: EA3)
            //                                |-EA4' + link EA2'               |-EA4'(base: EA4) + no more links
            //

            var eA = new Entity() { Name = "A" };
            var eA1 = new Entity() { Name = "A1" };
            var eA2 = new Entity() { Name = "A2" };
            var eA3 = new Entity() { Name = "A3" };
            eA.Transform.Children.Add(eA1.Transform);
            eA.Transform.Children.Add(eA2.Transform);
            eA.Transform.Children.Add(eA3.Transform);

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA1));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA2));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA3));
            baseAsset.Hierarchy.RootPartIds.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA2FromNewBase = newBaseAsset.Hierarchy.Parts.First(item => item.Entity.Id == eA2.Id);
            newBaseAsset.Hierarchy.Parts[eA.Id].Entity.Transform.Children.Remove(eA2FromNewBase.Entity.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            var eA4 = new Entity() { Name = "A4" };

            var rootInNew = newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()];
            var eA2InNewTransform = rootInNew.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Name == "A2");
            Assert.NotNull(eA2InNewTransform);

            // Add EA4 with link to EA1 entity and EA2 component
            var testComponent = new TestEntityComponent
            {
                EntityLink = eA2InNewTransform.Entity,
            };

            eA4.Add(testComponent);
            newAsset.Hierarchy.Parts.Add(new EntityDesign(eA4));
            rootInNew.Entity.Transform.Children.Add(eA4.Transform);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityHierarchyAssetBase)AssetCloner.Clone(baseAsset), (EntityHierarchyAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Parts.Count); // EA, EA1', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()];

            Assert.AreEqual(3, rootEntity.Entity.Transform.Children.Count);

            var eA4Merged = rootEntity.Entity.Transform.Children[2].Entity;
            Assert.AreEqual("A1", rootEntity.Entity.Transform.Children[0].Entity.Name);
            Assert.AreEqual("A3", rootEntity.Entity.Transform.Children[1].Entity.Name);
            Assert.AreEqual("A4", eA4Merged.Name);

            var testComponentMerged = eA4Merged.Get<TestEntityComponent>();
            Assert.NotNull(testComponentMerged);

            Assert.Null(testComponentMerged.EntityLink);
        }

        [Test]
        public void TestMergeAddEntityWithLinks2()
        {
            // Test merging an PrefabAsset with a root entity EA, and 3 child entities
            // - Add a child entity to NewBase that has a link to an another entity + a link to the component of another entity
            //
            //       Base         NewBase                      NewAsset                  NewAsset (Merged)
            //                                                 
            //       EA           EA                           EA'(base: EA)             EA'(base: EA)
            //        |-EA1       |-EA1                        |-EA1'(base: EA1)         |-EA1'(base: EA1)
            //        |-EA2       |-EA2 + link EA4             |-EA2'(base: EA2)         |-EA2'(base: EA2) + link EA4'
            //        |-EA3       |-EA3                        |-EA3'(base: EA3)         |-EA3'(base: EA3)
            //                    |-EA4                                                  |-EA4'(base: EA4)
            //
            var eA = new Entity() { Name = "A" };
            var eA1 = new Entity() { Name = "A1" };
            var eA2 = new Entity() { Name = "A2" };
            var eA3 = new Entity() { Name = "A3" };
            eA.Transform.Children.Add(eA1.Transform);
            eA.Transform.Children.Add(eA2.Transform);
            eA.Transform.Children.Add(eA3.Transform);

            // Create Base Asset
            var baseAsset = new PrefabAsset();
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA1));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA2));
            baseAsset.Hierarchy.Parts.Add(new EntityDesign(eA3));
            baseAsset.Hierarchy.RootPartIds.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA4 = new Entity() { Name = "A4" };
            var rootInNewBase = newBaseAsset.Hierarchy.Parts[newBaseAsset.Hierarchy.RootPartIds.First()];

            var eA2InNewBaseTransform = rootInNewBase.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Id == eA2.Id);
            Assert.NotNull(eA2InNewBaseTransform);

            // Add EA4 with link to EA1 entity and EA2 component
            var testComponent = new TestEntityComponent
            {
                EntityLink = eA4,
            };

            eA2InNewBaseTransform.Entity.Add(testComponent);
            newBaseAsset.Hierarchy.Parts.Add(new EntityDesign(eA4));
            rootInNewBase.Entity.Transform.Children.Add(eA4.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityHierarchyAssetBase)AssetCloner.Clone(baseAsset), (EntityHierarchyAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(5, newAsset.Hierarchy.Parts.Count); // EA, EA1', EA2', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Parts[newAsset.Hierarchy.RootPartIds.First()];

            Assert.AreEqual(4, rootEntity.Entity.Transform.Children.Count);

            var eA1Merged = rootEntity.Entity.Transform.Children[0].Entity;
            var eA2Merged = rootEntity.Entity.Transform.Children[1].Entity;
            var eA4Merged = rootEntity.Entity.Transform.Children[3].Entity;
            Assert.AreEqual("A1", eA1Merged.Name);
            Assert.AreEqual("A2", eA2Merged.Name);
            Assert.AreEqual("A3", rootEntity.Entity.Transform.Children[2].Entity.Name);
            Assert.AreEqual("A4", eA4Merged.Name);

            var testComponentMerged = eA2Merged.Get<TestEntityComponent>();
            Assert.NotNull(testComponentMerged);

            Assert.AreEqual(eA4Merged, testComponentMerged.EntityLink);
        }

        [Test]
        public void TestMergeSimpleWithBasePartsAndLinks()
        {
            // part1:                part2:                  newAsset (BaseParts: part1):       newAssetMerged (BaseParts: part2):
            //   EA                    EA                      EA1 (base: EA)                     [0] EA1' (base: EA) 
            //   EB                    EB                      EB1 (base: EB)                     [1] EB1' (base: EB)
            //   EC + link: EA         EC + link: EA           EC1 (base: EC) + link: EA1         [2] EC1' (base: EC) + link: EA1'
            //                         ED + link: EB           EA2 (base: EA)                     [3] EA2' (base: EA)
            //                                                 EB2 (base: EB)                     [4] EB2' (base: EB)
            //                                                 EC2 (base: EC) + link: EA2         [5] EC2' (base: EC) + link: EA2'
            //                                                                                    [6] ED1' (base: ED) + link: EB1'
            //                                                                                    [7] ED2' (base: ED) + link: EB2'
            var entityA = new Entity() { Name = "A" };
            var entityB = new Entity() { Name = "B" };
            var entityC = new Entity() { Name = "C" };
            // EC + link: EA
            entityC.Add(new TestEntityComponent() { EntityLink = entityA });

            // part1 Asset
            var basePart = new PrefabAsset();
            basePart.Hierarchy.Parts.Add(new EntityDesign(entityA));
            basePart.Hierarchy.Parts.Add(new EntityDesign(entityB));
            basePart.Hierarchy.Parts.Add(new EntityDesign(entityC));
            basePart.Hierarchy.RootPartIds.Add(entityA.Id);
            basePart.Hierarchy.RootPartIds.Add(entityB.Id);
            basePart.Hierarchy.RootPartIds.Add(entityC.Id);

            // originalAsset: Add a new instanceId for this part
            var asset = new PrefabAsset();

            // Create part1 asset
            Guid part1InstanceId;
            Guid part12InstanceId;
            var part1 = basePart.CreatePrefabInstance(asset, "part", out part1InstanceId);
            var entityB1 = part1.Parts.First(it => it.Entity.Name == "B").Entity;
            var part12 = basePart.CreatePrefabInstance(asset, "part", out part12InstanceId);
            var entityB2 = part12.Parts.First(it => it.Entity.Name == "B").Entity;

            // create part2 assset
            var entityD = new Entity() { Name = "D" };
            basePart.Hierarchy.Parts.Add(new EntityDesign(entityD));
            basePart.Hierarchy.RootPartIds.Add(entityD.Id);
            // ED + link: EB
            var entityBFrom2 = basePart.Hierarchy.Parts.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            entityD.Add(new TestEntityComponent() { EntityLink = entityBFrom2 });

            asset.Hierarchy.Parts.AddRange(part1.Parts);
            asset.Hierarchy.Parts.AddRange(part12.Parts);
            asset.Hierarchy.RootPartIds.AddRange(part1.RootPartIds);
            asset.Hierarchy.RootPartIds.AddRange(part12.RootPartIds);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var entityMerge = asset.Merge(null, null, new List<AssetBase>() { new AssetBase("part", (Asset)AssetCloner.Clone(basePart)) } );
            Assert.False(entityMerge.HasErrors);

            // EntityD must be now part of the new asset
            Assert.AreEqual(8, asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(8, asset.Hierarchy.Parts.Count);
            Assert.AreEqual(2, asset.Hierarchy.Parts.Count(it => it.Entity.Name == "D"));

            foreach (var entity in asset.Hierarchy.Parts.Where(it => it.Entity.Name == "D"))
            {
                // Check that we have the correct baesId and basePartInstanceId
                Assert.True(entity.BasePartInstanceId.HasValue);
                Assert.True(entity.BaseId.HasValue);
                Assert.AreEqual(entityD.Id, entity.BaseId.Value);

                // Make sure that the entity is in the RootEntities
                Assert.True(asset.Hierarchy.RootPartIds.Contains(entity.Entity.Id));
            }

            var entityDesignD1 = asset.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "D" && it.BasePartInstanceId == part1InstanceId);
            Assert.NotNull(entityDesignD1);

            var entityDesignD2 = asset.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "D" && it.BasePartInstanceId == part12InstanceId);
            Assert.NotNull(entityDesignD2);

            // Check components
            var testComponentD1 = entityDesignD1.Entity.Get<TestEntityComponent>();
            Assert.NotNull(testComponentD1);
            Assert.AreEqual(entityB1, testComponentD1.EntityLink);

            var testComponentD2 = entityDesignD2.Entity.Get<TestEntityComponent>();
            Assert.NotNull(testComponentD2);
            Assert.AreEqual(entityB2, testComponentD2.EntityLink);
        }

        [Test]
        public void TestMergeSimpleWithBasePartsAndLinksForChildren()
        {
            // Similar to TestMergeSimpleWithBasePartsAndLinks, but perform merge on Entity.Transform.Children instead
            // Also Check that an asset that was removed is not added and links are actually removed as well

            // part1:                part2:                  newAsset (BaseParts: part1):       newAssetMerged (BaseParts: part2):
            // ERoot                 ERoot                   ERoot1                             ERoot1
            //   EA                    EA                      EA1 (base: EA)                     EA1' (base: EA) 
            //   EB                    EB                      EB1 (base: EB)                     EB1' (base: EB)
            //   EC + link: EA         EC + link: EA           EC1 (base: EC) + link: EA1         EC1' (base: EC) + link: EA1'
            //                         ED + link: EB                                              ED1' (base: ED) + link: EB1'
            //                                               ERoot2                             ERoot2
            //                                                 EA2 (base: EA)                     EA2' (base: EA)
            //                                                                       
            //                                                 EC2 (base: EC) + link: EA2         EC2' (base: EC) + link: EA2'
            //                                                                                    ED2' (base: ED) + noLink
            var eRoot = new Entity("Root");
            var entityA = new Entity() { Name = "A" };
            var entityB = new Entity() { Name = "B" };
            var entityC = new Entity() { Name = "C" };
            // EC + link: EA
            entityC.Add(new TestEntityComponent() { EntityLink = entityA });
            eRoot.Transform.Children.Add(entityA.Transform);
            eRoot.Transform.Children.Add(entityB.Transform);
            eRoot.Transform.Children.Add(entityC.Transform);

            // part1 Asset
            var part1 = new PrefabAsset();
            part1.Hierarchy.Parts.Add(new EntityDesign(eRoot));
            part1.Hierarchy.Parts.Add(new EntityDesign(entityA));
            part1.Hierarchy.Parts.Add(new EntityDesign(entityB));
            part1.Hierarchy.Parts.Add(new EntityDesign(entityC));
            part1.Hierarchy.RootPartIds.Add(eRoot.Id);

            // part2 Asset
            var part2 = (PrefabAsset)AssetCloner.Clone(part1);
            var eRootPart2 = part2.Hierarchy.Parts.Where(it => it.Entity.Name == "Root").Select(it => it.Entity).First();

            var entityD = new Entity() { Name = "D" };
            eRootPart2.Transform.Children.Add(entityD.Transform);

            // ED + link: EB
            var entityBFrom2 = part2.Hierarchy.Parts.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            entityD.Add(new TestEntityComponent() { EntityLink = entityBFrom2 });
            part2.Hierarchy.Parts.Add(new EntityDesign(entityD));

            // originalAsset: Add a new instanceId for this part
            var asset = new PrefabAsset();

            // Create derived parts
            //var eRoot1Asset = (PrefabAsset)part1.CreateChildAsset("part");
            //var eRoot2Asset = (PrefabAsset)part1.CreateChildAsset("part");
            //asset.AddPart(eRoot1Asset);
            //asset.AddPart(eRoot2Asset);
            Guid eRoot1Id;
            Guid eRoot2Id;
            var eRoot1Asset = part1.CreatePrefabInstance(asset, "part", out eRoot1Id);
            var eRoot2Asset = part1.CreatePrefabInstance(asset, "part", out eRoot2Id);
            asset.Hierarchy.Parts.AddRange(eRoot1Asset.Parts);
            asset.Hierarchy.Parts.AddRange(eRoot2Asset.Parts);
            asset.Hierarchy.RootPartIds.AddRange(eRoot1Asset.RootPartIds);
            asset.Hierarchy.RootPartIds.AddRange(eRoot2Asset.RootPartIds);

            //var eRoot2 = asset.Hierarchy.Entities[eRoot2Asset.Hierarchy.RootEntities[0]];
            var eRoot2 = asset.Hierarchy.Parts[eRoot2Asset.RootPartIds[0]];

            var entityToRemove = eRoot2.Entity.Transform.Children.First(it => it.Entity.Name == "B");
            eRoot2.Entity.Transform.Children.Remove(entityToRemove);
            asset.Hierarchy.Parts.Remove(entityToRemove.Entity.Id);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var entityMerge = asset.Merge(null, null, new List<AssetBase>() { new AssetBase("part", part2) } );
            Assert.False(entityMerge.HasErrors);

            // EntityD must be now part of the new asset
            Assert.AreEqual(2, asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(9, asset.Hierarchy.Parts.Count);
            Assert.AreEqual(2, asset.Hierarchy.Parts.Count(it => it.Entity.Name == "D"));

            foreach (var entity in asset.Hierarchy.Parts.Where(it => it.Entity.Name == "D"))
            {
                // Check that we have the correct baesId and basePartInstanceId
                Assert.True(entity.BasePartInstanceId.HasValue);
                Assert.True(entity.BaseId.HasValue);
                Assert.AreEqual(entityD.Id, entity.BaseId.Value);
            }

            var entityDesignD1 = asset.Hierarchy.Parts[asset.Hierarchy.Parts[asset.Hierarchy.RootPartIds[0]].Entity.Transform.Children.Where(it => it.Entity.Name == "D").Select(it => it.Entity.Id).FirstOrDefault()];
            Assert.NotNull(entityDesignD1);
            //Assert.AreEqual(eRoot1Asset.Id, entityDesignD1.Design.BasePartInstanceId);
            Assert.AreEqual(eRoot1Id, entityDesignD1.BasePartInstanceId);
            var testComponentD1 = entityDesignD1.Entity.Get<TestEntityComponent>();
            Assert.NotNull(testComponentD1);
            var entityB1 = asset.Hierarchy.Parts[asset.Hierarchy.RootPartIds[0]].Entity.Transform.Children.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            Assert.AreEqual(entityB1, testComponentD1.EntityLink);

            var entityDesignD2 = asset.Hierarchy.Parts[asset.Hierarchy.Parts[asset.Hierarchy.RootPartIds[1]].Entity.Transform.Children.Where(it => it.Entity.Name == "D").Select(it => it.Entity.Id).FirstOrDefault()];
            Assert.NotNull(entityDesignD2);
            //Assert.AreEqual(eRoot2Asset.Id, entityDesignD2.Design.BasePartInstanceId);
            Assert.AreEqual(eRoot2Id, entityDesignD2.BasePartInstanceId);
            var testComponentD2 = entityDesignD2.Entity.Get<TestEntityComponent>();
            Assert.NotNull(testComponentD2);
            Assert.AreEqual(null, testComponentD2.EntityLink);
        }

        [Test]
        public void TestCascadedInheritance()
        {
            // Test with:
            // a1: an asset with 2 entities
            // a2: an asset using a1 by composition with 2 instances
            // a3: an asset based on a2
            //
            // Add one entity to a1. Check that a2 and a3 will get correctly the entities replicated

            // Before Merge
            // a1:      a2: (baseParts: a1, 2 instances)     a3: (base: a2)
            //  | ea     | ea1 (base: ea)                     | ea1' (base: ea1)
            //  | eb     | eb1 (base: eb)                     | eb1' (base: eb1)
            //           | ea2 (base: ea)                     | ea2' (base: ea2)
            //           | eb2 (base: eb)                     | eb2' (base: eb2)


            // After Merge
            // We add one entity to the base a1 
            // a1:      a2: (baseParts: a1, 2 instances)     a3: (base: a2)
            //  | ea     | ea1 (base: ea)                     | ea1' (base: ea1)
            //  | eb     | eb1 (base: eb)                     | eb1' (base: eb1)
            //  | ec     | ec1 (base: ec)                     | ec1' (base: ec1)
            //           | ea2 (base: ea)                     | ea2' (base: ea2)
            //           | eb2 (base: eb)                     | eb2' (base: eb2)
            //           | ec2 (base: ec)                     | ec2' (base: ec2)

            var a1 = new PrefabAsset();
            var ea = new Entity("ea");
            var eb = new Entity("eb");
            a1.Hierarchy.Parts.Add(new EntityDesign(ea));
            a1.Hierarchy.Parts.Add(new EntityDesign(eb));
            a1.Hierarchy.RootPartIds.Add(ea.Id);
            a1.Hierarchy.RootPartIds.Add(eb.Id);

            var a2 = new PrefabAsset();
            var aPartInstance1 = a1.CreatePrefabInstance(a2, "a1");
            var aPartInstance2 = a1.CreatePrefabInstance(a2, "a1");
            a2.Hierarchy.Parts.AddRange(aPartInstance1.Parts);
            a2.Hierarchy.Parts.AddRange(aPartInstance2.Parts);
            a2.Hierarchy.RootPartIds.AddRange(aPartInstance1.RootPartIds);
            a2.Hierarchy.RootPartIds.AddRange(aPartInstance2.RootPartIds);

            // Modify a1 to add entity ec
            var ec = new Entity("ec");
            a1.Hierarchy.Parts.Add(new EntityDesign(ec));
            a1.Hierarchy.RootPartIds.Add(ec.Id);

            var a3 = (PrefabAsset)a2.CreateChildAsset("a2");

            // Merge a2
            var result2 = a2.Merge(null, null, new List<AssetBase>()
            {
                new AssetBase("a1", (Asset)AssetCloner.Clone(a1))
            });

            Assert.False(result2.HasErrors);
            Assert.AreEqual(6, a2.Hierarchy.RootPartIds.Count);
            Assert.True(a2.Hierarchy.Parts.All(it => it.BaseId.HasValue && it.BasePartInstanceId.HasValue));

            // Merge a3
            var result3 = a3.Merge((Asset)AssetCloner.Clone(a3.Base.Asset), (Asset)AssetCloner.Clone(a2), null);

            Assert.False(result3.HasErrors);
            Assert.AreEqual(6, a3.Hierarchy.RootPartIds.Count);
            Assert.True(a3.Hierarchy.Parts.All(it => !it.BasePartInstanceId.HasValue));
            Assert.True(a3.Hierarchy.Parts.All(it => it.BaseId.HasValue && a2.Hierarchy.Parts.ContainsKey(it.BaseId.Value)));
        }

        [Test]
        public void TestMultiplePrefabsMixedInheritance()
        {
            PrefabAssetMerge.Debug = true;

            // The purpose of this test is to check that modifying a prefab base is correctly propagated through all 
            // derived prefabs. We use the following scenario:
            // a1: base asset)
            // a2: inherit from a1 by composition with 2 instances (baseParts: a1, 2 instances)
            // a3: direct inheritance from a2 (base: a2)
            // This scenario doesn't happen in practice, as we have restricted only to inheritance by composition for prefabs
            // but we verify that the code is actually working for this scenario

            var package = new Package();

            var assetItems = package.Assets;

            // Before Adding Package
            // a1:      a2: (baseParts: a1, 2 instances)     a3: (base: a2)
            //  | ea     | ea1 (base: ea)                     | ea1' (base: ea1)
            //  | eb     | eb1 (base: eb)                     | eb1' (base: eb1)
            //           | ea2 (base: ea)                     | ea2' (base: ea2)
            //           | eb2 (base: eb)                     | eb2' (base: eb2)


            // After adding the package to the session 
            // We add one entity to the base a1 
            // a1:      a2: (baseParts: a1, 2 instances)     a3: (base: a2)
            //  | ea     | ea1 (base: ea)                     | ea1' (base: ea1)
            //  | eb     | eb1 (base: eb)                     | eb1' (base: eb1)
            //  | ec     | ec1 (base: ec)                     | ec1' (base: ec1)
            //           | ea2 (base: ea)                     | ea2' (base: ea2)
            //           | eb2 (base: eb)                     | eb2' (base: eb2)
            //           | ec2 (base: ec)                     | ec2' (base: ec2)

            var a1 = new PrefabAsset();
            var ea = new Entity("ea");
            var eb = new Entity("eb");
            a1.Hierarchy.Parts.Add(new EntityDesign(ea));
            a1.Hierarchy.Parts.Add(new EntityDesign(eb));
            a1.Hierarchy.RootPartIds.Add(ea.Id);
            a1.Hierarchy.RootPartIds.Add(eb.Id);

            assetItems.Add(new AssetItem("a1", a1));

            var a2 = new PrefabAsset();
            var aPartInstance1 = a1.CreatePrefabInstance(a2, "a1");
            var aPartInstance2 = a1.CreatePrefabInstance(a2, "a1");
            a2.Hierarchy.Parts.AddRange(aPartInstance1.Parts);
            a2.Hierarchy.Parts.AddRange(aPartInstance2.Parts);
            a2.Hierarchy.RootPartIds.AddRange(aPartInstance1.RootPartIds);
            a2.Hierarchy.RootPartIds.AddRange(aPartInstance2.RootPartIds);
            assetItems.Add(new AssetItem("a2", a2));

            // Modify a1 to add entity ec
            var ec = new Entity("ec");
            a1.Hierarchy.Parts.Add(new EntityDesign(ec));
            a1.Hierarchy.RootPartIds.Add(ec.Id);

            var a3 = (PrefabAsset)a2.CreateChildAsset("a2");

            assetItems.Add(new AssetItem("a3", a3));

            // Create a session with this project
            using (var session = new PackageSession())
            {
                var logger = new LoggerResult();
                session.AddExistingPackage(package, logger);

                Assert.False(logger.HasErrors);

                Assert.AreEqual(6, a2.Hierarchy.RootPartIds.Count);
                Assert.True(a2.Hierarchy.Parts.All(it => it.BaseId.HasValue && it.BasePartInstanceId.HasValue));

                Assert.AreEqual(6, a3.Hierarchy.RootPartIds.Count);
                Assert.True(a3.Hierarchy.Parts.All(it => !it.BasePartInstanceId.HasValue));
                Assert.True(a3.Hierarchy.Parts.All(it => it.BaseId.HasValue && a2.Hierarchy.Parts.ContainsKey(it.BaseId.Value)));
            }
        }


        [Test]
        public void TestMultiplePrefabsInheritanceAndChildren()
        {
            PrefabAssetMerge.Debug = true;

            // The purpose of this test is to check that modifying a prefab base is correctly propagated through all 
            // derived prefabs. We use the following scenario:
            //
            // a1: base asset
            // a2: inherit from a1 by composition with 2 instances (baseParts: a1 => 2 instances)
            // a3: inherit from a1 by composition with 1 instances (baseParts: a1 => 1 instances)
            // a4: inherit from a2 and a3 by composition with 1 instances for each (baseParts: a1 => 1 instance, a2 => 1 instance)
            //
            // Unlike TestMultiplePrefabsMixedInheritance, we use only inheritance by composition for this scenario to match current use cases

            var package = new Package();

            var assetItems = package.Assets;


            // First we create assets with the following configuration:
            // a1:                  a2: (baseParts: a1, 2 instances)     a3: (baseParts: a1)               a4: (baseParts: a2 x 1, a3 x 1)
            //  | er                 | er1 (base: er)                     | er1' (base: er)                 | eRoot
            //    | ea                 | ea1 (base: ea)                     | ea1' (base: ea)                 | er1* (base: er)  
            //    | eb                 | eb1 (base: eb)                     | eb1' (base: eb)                   | ea1* (base: ea)
            //    | ec                 | ec1 (base: ec)                     | ec1' (base: ec)                   | eb1* (base: eb)
            //                       | er2 (base: er)                                                           | ec1* (base: ec)
            //                         | ea2 (base: ea)                                                       | er2* (base: er)  
            //                         | eb2 (base: eb)                                                         | ea2* (base: ea)
            //                         | ec2 (base: ec)                                                         | eb2* (base: eb)
            //                                                                                                  | ec2* (base: ec)  
            //                                                                                              | er1'* (base: er)     
            //                                                                                                | ea1'* (base: ea)    
            //                                                                                                | eb1'* (base: eb)  
            //                                                                                                | ec1'* (base: ec)
            var a1 = new PrefabAsset();
            var er = new Entity("er");
            var ea = new Entity("ea");
            var eb = new Entity("eb");
            var ec = new Entity("ec");
            a1.Hierarchy.Parts.Add(new EntityDesign(er));
            a1.Hierarchy.Parts.Add(new EntityDesign(ea));
            a1.Hierarchy.Parts.Add(new EntityDesign(eb));
            a1.Hierarchy.Parts.Add(new EntityDesign(ec));
            a1.Hierarchy.RootPartIds.Add(er.Id);
            er.AddChild(ea);
            er.AddChild(eb);
            er.AddChild(ec);

            assetItems.Add(new AssetItem("a1", a1));

            var member = TypeDescriptorFactory.Default.Find(typeof(Entity))["Name"];
            
            var a2 = new PrefabAsset();
            var a2PartInstance1 = a1.CreatePrefabInstance(a2, "a1");
            foreach (var entity in a2PartInstance1.Parts)
            {
                entity.Entity.Name += "1";
                entity.Entity.SetOverride(member, OverrideType.New);
            }

            var a2PartInstance2 = a1.CreatePrefabInstance(a2, "a1");
            foreach (var entity in a2PartInstance2.Parts)
            {
                entity.Entity.Name += "2";
                entity.Entity.SetOverride(member, OverrideType.New);
            }

            a2.Hierarchy.Parts.AddRange(a2PartInstance1.Parts);
            a2.Hierarchy.Parts.AddRange(a2PartInstance2.Parts);
            a2.Hierarchy.RootPartIds.AddRange(a2PartInstance1.RootPartIds);
            a2.Hierarchy.RootPartIds.AddRange(a2PartInstance2.RootPartIds);
            Assert.AreEqual(8, a2.Hierarchy.Parts.Count);
            Assert.AreEqual(2, a2.Hierarchy.RootPartIds.Count);
            assetItems.Add(new AssetItem("a2", a2));

            var a3 = new PrefabAsset();
            var a3PartInstance1 = a1.CreatePrefabInstance(a3, "a1");
            foreach (var entity in a3PartInstance1.Parts)
            {
                entity.Entity.Name += "1'";
                entity.Entity.SetOverride(member, OverrideType.New);
            }
            a3.Hierarchy.Parts.AddRange(a3PartInstance1.Parts);
            a3.Hierarchy.RootPartIds.AddRange(a3PartInstance1.RootPartIds);
            Assert.AreEqual(4, a3.Hierarchy.Parts.Count);
            Assert.AreEqual(1, a3.Hierarchy.RootPartIds.Count);
            assetItems.Add(new AssetItem("a3", a3));

            var a4 = new PrefabAsset();
            var eRoot = new Entity("eRoot");
            var a2PartInstance3 = a2.CreatePrefabInstance(a4, "a2");

            foreach (var entity in a2PartInstance3.Parts)
            {
                entity.Entity.Name += "*";
                entity.Entity.SetOverride(member, OverrideType.New);
            }
            foreach (var entity in a2PartInstance3.Parts.Where(t => a2PartInstance3.RootPartIds.Contains(t.Entity.Id)))
            {
                eRoot.AddChild(entity.Entity);
            }
            var a3PartInstance2 = a3.CreatePrefabInstance(a4, "a3");
            foreach (var entity in a3PartInstance2.Parts)
            {
                entity.Entity.Name += "*";
                entity.Entity.SetOverride(member, OverrideType.New);
            }

            a4.Hierarchy.Parts.Add(new EntityDesign(eRoot));
            a4.Hierarchy.Parts.AddRange(a2PartInstance3.Parts);
            a4.Hierarchy.Parts.AddRange(a3PartInstance2.Parts);
            a4.Hierarchy.RootPartIds.Add(eRoot.Id);
            a4.Hierarchy.RootPartIds.AddRange(a3PartInstance2.RootPartIds);

            Assert.AreEqual(13, a4.Hierarchy.Parts.Count);
            Assert.AreEqual(2, a4.Hierarchy.RootPartIds.Count);

            assetItems.Add(new AssetItem("a4", a4));

            Assert.True(a1.DumpTo(Console.Out, "a1 BEFORE PrefabMergeAsset"));
            Assert.True(a2.DumpTo(Console.Out, "a2 BEFORE PrefabMergeAsset"));
            Assert.True(a3.DumpTo(Console.Out, "a3 BEFORE PrefabMergeAsset"));
            Assert.True(a4.DumpTo(Console.Out, "a4 BEFORE PrefabMergeAsset"));


            // Then we simulate a concurrent change to a1 by someone that didn't have a2/a3/a4
            // - Add one component to a1, linking to an existing entity ea
            // - Add a root entity to a1 with a link to an existing entity eb
            //
            // a1:                  a2: (baseParts: a1, 2 instances)     a3: (baseParts: a1)                a4: (baseParts: a2 x 1, a3 x 1)
            //  | er                 | er1 (base: er)                     | er1' (base: er)                  | eRoot
            //    | ea                 | ea1 (base: ea)                     | ea1' (base: ea)                  | er1* (base: er)  
            //    | eb                 | eb1 (base: eb)                     | eb1' (base: eb)                    | ea1* (base: ea)
            //    | ec + link ea       | ec1 + link ea1 (base: ec)          | ec1' + link ea1' (base: ec)        | eb1* (base: eb)
            //  | ex                 | er2 (base: er)                     | ex(1') (base: ex)                    | ec1* + link ea1* (base: ec)
            //    | ey + link eb       | ea2 (base: ea)                     | ey(1') + link eb1'               | er2* (base: er)  
            //                         | eb2 (base: eb)                                                          | ea2* (base: ea)
            //                         | ec2 + link ea2 (base: ec)                                               | eb2* (base: eb)
            //                       | ex(1)                                                                     | ec2* + link ea2* (base: ec)  
            //                         | ey(1) + link eb1                                                    | er1'* (base: er)     
            //                       | ex(2)                                                                   | ea1'* (base: ea)
            //                         | ey(2) + link eb2                                                      | eb1'* (base: eb)  
            //                                                                                                 | ec1'* + link ea1'* (base: ec)  
            //                                                                                               | ex(1*)
            //                                                                                                 | ey(1*) + link eb1*
            //                                                                                               | ex(2*)
            //                                                                                                 | ey(2*) + link eb2*
            //                                                                                               | ex(1') (base: ex)   
            //                                                                                                 | ey(1') + link eb1'*
            ec.Components.Add(new TestEntityComponent() { EntityLink = ea });

            var ex = new Entity("ex");
            var ey = new Entity("ey");
            ey.Components.Add(new TestEntityComponent() { EntityLink = eb });
            ex.AddChild(ey);
            a1.Hierarchy.Parts.Add(new EntityDesign(ex));
            a1.Hierarchy.Parts.Add(new EntityDesign(ey));
            a1.Hierarchy.RootPartIds.Add(ex.Id);
            Assert.AreEqual(6, a1.Hierarchy.Parts.Count);
            Assert.AreEqual(2, a1.Hierarchy.RootPartIds.Count);

            // Simulates the loading of this package
            using (var session = new PackageSession())
            {
                var logger = new LoggerResult();
                session.AddExistingPackage(package, logger);

                Assert.False(logger.HasErrors);

                Assert.True(a1.DumpTo(Console.Out, "a1 AFTER PrefabMergeAsset"));

                // ------------------------------------------------
                // Check for a2
                // ------------------------------------------------
                // a2: (baseParts: a1, 2 instances)
                //  | er1 (base: er)               
                //    | ea1 (base: ea)             
                //    | eb1 (base: eb)             
                //    | ec1 + link ea1 (base: ec)  
                //  | er2 (base: er)               
                //    | ea2 (base: ea)             
                //    | eb2 (base: eb)             
                //    | ec2 + link ea2 (base: ec)  
                //  | ex(1)                          
                //    | ey(1) + link eb1             
                //  | ex(2)                          
                //    | ey(2) + link eb2             
                {
                    Assert.True(a2.DumpTo(Console.Out, "a2 AFTER PrefabMergeAsset"));
                    Assert.AreEqual(4, a2.Hierarchy.RootPartIds.Count);
                    Assert.True(a2.Hierarchy.Parts.All(it => it.BaseId.HasValue && it.BasePartInstanceId.HasValue));

                    // Check that we have all expected entities
                    Assert.AreEqual(12, a2.Hierarchy.Parts.Count);

                    var eb1 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb1")?.Entity;
                    var eb2 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb2")?.Entity;
                    Assert.NotNull(eb1);
                    Assert.NotNull(eb2);

                    // Check that we have ex and ey
                    var exList = a2.Hierarchy.Parts.Where(it => it.Entity.Name == ex.Name).ToList();
                    Assert.AreEqual(2, exList.Count);

                    // Check that both [ex] have both 1 element [ey] and the links to eb1/eb2 are correct
                    {
                        var expecting = new List<Entity>() { eb1, eb2 };
                        for (int i = 0; i < exList.Count; i++)
                        {
                            var ex1 = exList[i].Entity;
                            Assert.AreEqual(1, ex1.Transform.Children.Count);
                            var ey1 = ex1.Transform.Children[0].Entity;
                            Assert.AreEqual(ey.Name, ey1.Name);
                            Assert.NotNull(ey1.Get<TestEntityComponent>());

                            var entityLink = ey1.Get<TestEntityComponent>().EntityLink;
                            Assert.True(expecting.Contains(entityLink));
                            expecting.Remove(entityLink);
                        }
                    }

                    // Check link from ec1 to ea1
                    {
                        var ec1 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ec1")?.Entity;
                        Assert.NotNull(ec1);

                        var ea1 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ea1")?.Entity;
                        Assert.NotNull(ea1);

                        Assert.NotNull(ec1.Get<TestEntityComponent>());
                        Assert.AreEqual(ea1, ec1.Get<TestEntityComponent>().EntityLink);
                    }

                    // Check link from ec2 to ea2
                    {
                        var ec2 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ec2")?.Entity;
                        Assert.NotNull(ec2);

                        var ea2 = a2.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ea2")?.Entity;
                        Assert.NotNull(ea2);

                        Assert.NotNull(ec2.Get<TestEntityComponent>());
                        Assert.AreEqual(ea2, ec2.Get<TestEntityComponent>().EntityLink);
                    }
                }

                // ------------------------------------------------
                // Check for a3
                // ------------------------------------------------
                // a3: (baseParts: a1)             
                //  | er1' (base: er)              
                //    | ea1' (base: ea)            
                //    | eb1' (base: eb)            
                //    | ec1' + link ea1' (base: ec)
                //  | ex1' (base: ex)              
                //    | ey1' + link eb1'           
                {
                    Assert.True(a3.DumpTo(Console.Out, "a3 AFTER PrefabMergeAsset"));

                    Assert.AreEqual(2, a3.Hierarchy.RootPartIds.Count);
                    Assert.True(a3.Hierarchy.Parts.All(it => it.BaseId.HasValue && it.BasePartInstanceId.HasValue));

                    // Check that we have all expected entities
                    Assert.AreEqual(6, a3.Hierarchy.Parts.Count);

                    var eb1 = a3.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb1'")?.Entity;
                    Assert.NotNull(eb1);

                    // Check that we have ex and ey
                    var exList = a3.Hierarchy.Parts.Where(it => it.Entity.Name == ex.Name).ToList();
                    Assert.AreEqual(1, exList.Count);

                    // Check that [ex] have 1 element [ey] and the link to eb1 is correct
                    {
                        var ex1 = exList[0].Entity;
                        Assert.AreEqual(1, ex1.Transform.Children.Count);
                        var ey1 = ex1.Transform.Children[0].Entity;
                        Assert.AreEqual(ey.Name, ey1.Name);
                        Assert.NotNull(ey1.Get<TestEntityComponent>());

                        Assert.AreEqual(eb1, ey1.Get<TestEntityComponent>().EntityLink);
                    }

                    {
                        var ec1 = a3.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ec1'")?.Entity;
                        Assert.NotNull(ec1);

                        var ea1 = a3.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "ea1'")?.Entity;
                        Assert.NotNull(ea1);

                        Assert.NotNull(ec1.Get<TestEntityComponent>());
                        Assert.AreEqual(ea1, ec1.Get<TestEntityComponent>().EntityLink);
                    }
                }

                // ------------------------------------------------
                // Check for a4
                // ------------------------------------------------
                // a4: (baseParts: a2 x 1, a3 x 1)
                //  | eNewRoot
                //    | er1* (base: er)  
                //      | ea1* (base: ea)
                //      | eb1* (base: eb)
                //      | ec1* + link ea1* (base: ec)
                //    | er2* (base: er)  
                //      | ea2* (base: ea)
                //      | eb2* (base: eb)
                //      | ec2* + link ea2* (base: ec)
                //  | er1'* (base: er)     
                //    | ea1'* (base: ea)
                //    | eb1'* (base: eb)  
                //    | ec1'* + link ea1'* (base: ec)
                //  | ex(1*)
                //    | ey(1*) + link eb1*
                //  | ex(2*)
                //    | ey(2*) + link eb2*
                //  | ex(1') (base: ex)   
                //    | ey(1') + link eb1'*
                {
                    Assert.True(a4.DumpTo(Console.Out, "a4 AFTER PrefabMergeAsset"));

                    Assert.AreEqual(5, a4.Hierarchy.RootPartIds.Count);
                    Assert.True(a4.Hierarchy.Parts.Where(it => it.Entity.Name != "eRoot").All(it => it.Entity.Name != "eRoot" && it.BaseId.HasValue && it.BasePartInstanceId.HasValue));

                    // Check that we have all expected entities
                    Assert.AreEqual(19, a4.Hierarchy.Parts.Count);

                    var eb1 = a4.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb1*")?.Entity;
                    var eb1_2 = a4.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb1'*")?.Entity;
                    var eb2 = a4.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == "eb2*")?.Entity;
                    Assert.NotNull(eb1);
                    Assert.NotNull(eb1_2);
                    Assert.NotNull(eb2);

                    // Check that we have ex and ey
                    var exList = a4.Hierarchy.Parts.Where(it => it.Entity.Name == ex.Name).ToList();
                    Assert.AreEqual(3, exList.Count);

                    // Check that both [ex] have both 1 element [ey] and the links to eb1/eb2 are correct
                    {
                        var expecting = new List<Entity>() { eb1, eb1_2, eb2 };

                        for (int i = 0; i < exList.Count; i++)
                        {
                            var ex1 = exList[i].Entity;
                            Assert.AreEqual(1, ex1.Transform.Children.Count);
                            var ey1 = ex1.Transform.Children[0].Entity;
                            Assert.AreEqual(ey.Name, ey1.Name);
                            Assert.NotNull(ey1.Get<TestEntityComponent>());

                            var entityLink = ey1.Get<TestEntityComponent>().EntityLink;
                            Assert.True(expecting.Contains(entityLink));
                            expecting.Remove(entityLink);
                        }
                    }

                    // Check all [er] entities
                    Action<string> checkErX = (erName) =>
                    {
                        var er1 = a4.Hierarchy.Parts.FirstOrDefault(it => it.Entity.Name == erName)?.Entity;
                        Assert.NotNull(er1);
                        Assert.AreEqual(3, er1.Transform.Children.Count);

                        var ec1 = er1.Transform.Children.FirstOrDefault(it => it.Entity.Name.StartsWith("ec"))?.Entity;
                        Assert.NotNull(ec1);
                        var ea1 = er1.Transform.Children.FirstOrDefault(it => it.Entity.Name.StartsWith("ea"))?.Entity;
                        Assert.NotNull(ea1);

                        Assert.NotNull(ec1.Get<TestEntityComponent>());

                        Assert.AreEqual(ea1, ec1.Get<TestEntityComponent>().EntityLink);
                    };

                    checkErX("er1*");
                    checkErX("er2*");
                    checkErX("er1'*");
                }
            }
        }
    }
}
