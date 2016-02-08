// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
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
    public class TestEntityGroupAssetMerge
    {

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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityB, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityC, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(entityA.Id);
            baseAsset.Hierarchy.RootEntities.Add(entityB.Id);
            baseAsset.Hierarchy.RootEntities.Add(entityC.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // On a derive asset all entities must have a base value and base must come from baseAsset
            Assert.True(newAsset.Hierarchy.Entities.All(item => item.Design.BaseId.HasValue && baseAsset.Hierarchy.Entities.ContainsKey(item.Design.BaseId.Value)));

            // Verify that we have exactly the same number of entities
            Assert.AreEqual(baseAsset.Hierarchy.RootEntities.Count, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(baseAsset.Hierarchy.Entities.Count, newAsset.Hierarchy.Entities.Count);

            // Verify that baseId and newId is correctly setup
            var entityAInNew = newAsset.Hierarchy.Entities.FirstOrDefault(item => item.Design.BaseId.Value == entityA.Id && item.Entity.Id != item.Design.BaseId.Value);
            Assert.NotNull(entityAInNew);

            var entityBInNew = newAsset.Hierarchy.Entities.FirstOrDefault(item => item.Design.BaseId.Value == entityB.Id && item.Entity.Id != item.Design.BaseId.Value);
            Assert.NotNull(entityBInNew);

            var entityCInNew = newAsset.Hierarchy.Entities.FirstOrDefault(item => item.Design.BaseId.Value == entityC.Id && item.Entity.Id != item.Design.BaseId.Value);
            Assert.NotNull(entityCInNew);

            // Verify that RootEntities are also correctly mapped
            Assert.AreEqual(entityAInNew.Entity.Id, newAsset.Hierarchy.RootEntities[0]);
            Assert.AreEqual(entityBInNew.Entity.Id, newAsset.Hierarchy.RootEntities[1]);
            Assert.AreEqual(entityCInNew.Entity.Id, newAsset.Hierarchy.RootEntities[2]);
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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityB, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(entityC, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(entityA.Id);
            baseAsset.Hierarchy.RootEntities.Add(entityB.Id);
            baseAsset.Hierarchy.RootEntities.Add(entityC.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var entityD = new Entity() { Name = "D" };
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            newBaseAsset.Hierarchy.Entities.Add(new EntityDesign(entityD, new EntityDesignData()));
            newBaseAsset.Hierarchy.RootEntities.Add(entityD.Id);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityGroupAssetBase)AssetCloner.Clone(baseAsset), (EntityGroupAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            // Both root and entities must be the same
            Assert.AreEqual(4, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Entities.Count);

            // All entities must have a base value
            Assert.True(newAsset.Hierarchy.Entities.All(item => item.Design.BaseId.HasValue));

            var entityAInNewAsset = newAsset.Hierarchy.Entities.Where(item => item.Design.BaseId.Value == entityA.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityAInNewAsset);
            var entityBInNewAsset = newAsset.Hierarchy.Entities.Where(item => item.Design.BaseId.Value == entityB.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityBInNewAsset);
            var entityCInNewAsset = newAsset.Hierarchy.Entities.Where(item => item.Design.BaseId.Value == entityC.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityCInNewAsset);

            var entityDInNewAsset = newAsset.Hierarchy.Entities.Where(item => item.Design.BaseId.Value == entityD.Id).Select(item => item.Entity).FirstOrDefault();
            Assert.NotNull(entityDInNewAsset);

            // Hierarchy must be: EA, EB, EC, ED
            Assert.AreEqual(entityAInNewAsset.Id, newAsset.Hierarchy.RootEntities[0]);
            Assert.AreEqual(entityBInNewAsset.Id, newAsset.Hierarchy.RootEntities[1]);
            Assert.AreEqual(entityCInNewAsset.Id, newAsset.Hierarchy.RootEntities[2]);
            Assert.AreEqual(entityDInNewAsset.Id, newAsset.Hierarchy.RootEntities[3]);
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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA1, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA2, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA3, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA2FromNewBase = newBaseAsset.Hierarchy.Entities.First(item => item.Entity.Id == eA2.Id);
            newBaseAsset.Hierarchy.Entities[eA.Id].Entity.Transform.Children.Remove(eA2FromNewBase.Entity.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();
            var eA4 = new Entity() { Name = "A4" };
            newAsset.Hierarchy.Entities.Add(new EntityDesign(eA4, new EntityDesignData()));
            newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()].Entity.Transform.Children.Add(eA4.Transform);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityGroupAssetBase)AssetCloner.Clone(baseAsset), (EntityGroupAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Entities.Count); // EA, EA1', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()];

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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA1, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA2, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA3, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA4 = new Entity() { Name = "A4" };
            var rootInNewBase = newBaseAsset.Hierarchy.Entities[newBaseAsset.Hierarchy.RootEntities.First()];
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
            newBaseAsset.Hierarchy.Entities.Add(new EntityDesign(eA4, new EntityDesignData()));
            rootInNewBase.Entity.Transform.Children.Add(eA4.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityGroupAssetBase)AssetCloner.Clone(baseAsset), (EntityGroupAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(5, newAsset.Hierarchy.Entities.Count); // EA, EA1', EA2', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()];

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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA1, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA2, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA3, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA2FromNewBase = newBaseAsset.Hierarchy.Entities.First(item => item.Entity.Id == eA2.Id);
            newBaseAsset.Hierarchy.Entities[eA.Id].Entity.Transform.Children.Remove(eA2FromNewBase.Entity.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            var eA4 = new Entity() { Name = "A4" };

            var rootInNew = newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()];
            var eA2InNewTransform = rootInNew.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Name == "A2");
            Assert.NotNull(eA2InNewTransform);

            // Add EA4 with link to EA1 entity and EA2 component
            var testComponent = new TestEntityComponent
            {
                EntityLink = eA2InNewTransform.Entity,
            };

            eA4.Add(testComponent);
            newAsset.Hierarchy.Entities.Add(new EntityDesign(eA4, new EntityDesignData()));
            rootInNew.Entity.Transform.Children.Add(eA4.Transform);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityGroupAssetBase)AssetCloner.Clone(baseAsset), (EntityGroupAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(4, newAsset.Hierarchy.Entities.Count); // EA, EA1', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()];

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
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA1, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA2, new EntityDesignData()));
            baseAsset.Hierarchy.Entities.Add(new EntityDesign(eA3, new EntityDesignData()));
            baseAsset.Hierarchy.RootEntities.Add(eA.Id);

            var baseAssetItem = new AssetItem("base", baseAsset);

            // Create new Base Asset
            var newBaseAsset = (PrefabAsset)AssetCloner.Clone(baseAsset);
            var eA4 = new Entity() { Name = "A4" };
            var rootInNewBase = newBaseAsset.Hierarchy.Entities[newBaseAsset.Hierarchy.RootEntities.First()];

            var eA2InNewBaseTransform = rootInNewBase.Entity.Transform.Children.FirstOrDefault(item => item.Entity.Id == eA2.Id);
            Assert.NotNull(eA2InNewBaseTransform);

            // Add EA4 with link to EA1 entity and EA2 component
            var testComponent = new TestEntityComponent
            {
                EntityLink = eA4,
            };

            eA2InNewBaseTransform.Entity.Add(testComponent);
            newBaseAsset.Hierarchy.Entities.Add(new EntityDesign(eA4, new EntityDesignData()));
            rootInNewBase.Entity.Transform.Children.Add(eA4.Transform);

            // Create new Asset (from base)
            var newAsset = (PrefabAsset)baseAssetItem.CreateChildAsset();

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var result = newAsset.Merge((EntityGroupAssetBase)AssetCloner.Clone(baseAsset), (EntityGroupAssetBase)AssetCloner.Clone(newBaseAsset), null);
            Assert.False(result.HasErrors);

            Assert.AreEqual(1, newAsset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(5, newAsset.Hierarchy.Entities.Count); // EA, EA1', EA2', EA3', EA4'

            var rootEntity = newAsset.Hierarchy.Entities[newAsset.Hierarchy.RootEntities.First()];

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
            basePart.Hierarchy.Entities.Add(new EntityDesign(entityA, new EntityDesignData()));
            basePart.Hierarchy.Entities.Add(new EntityDesign(entityB, new EntityDesignData()));
            basePart.Hierarchy.Entities.Add(new EntityDesign(entityC, new EntityDesignData()));
            basePart.Hierarchy.RootEntities.Add(entityA.Id);
            basePart.Hierarchy.RootEntities.Add(entityB.Id);
            basePart.Hierarchy.RootEntities.Add(entityC.Id);

            // Create part1 asset
            var part1 = (PrefabAsset)basePart.CreateChildAsset("part");
            var entityB1 = part1.Hierarchy.Entities.First(it => it.Entity.Name == "B").Entity;
            var part12 = (PrefabAsset)basePart.CreateChildAsset("part");
            var entityB2 = part12.Hierarchy.Entities.First(it => it.Entity.Name == "B").Entity;

            // create part2 assset
            var entityD = new Entity() { Name = "D" };
            basePart.Hierarchy.Entities.Add(new EntityDesign(entityD, new EntityDesignData()));
            basePart.Hierarchy.RootEntities.Add(entityD.Id);
            // ED + link: EB
            var entityBFrom2 = basePart.Hierarchy.Entities.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            entityD.Add(new TestEntityComponent() { EntityLink = entityBFrom2 });

            // originalAsset: Add a new instanceId for this part
            var asset = new PrefabAsset();
            asset.AddPart(part1);
            asset.AddPart(part12);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var entityMerge = asset.Merge(null, null, new List<AssetBase>() { new AssetBase("part", (Asset)AssetCloner.Clone(basePart)) } );
            Assert.False(entityMerge.HasErrors);

            // EntityD must be now part of the new asset
            Assert.AreEqual(8, asset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(8, asset.Hierarchy.Entities.Count);
            Assert.AreEqual(2, asset.Hierarchy.Entities.Count(it => it.Entity.Name == "D"));

            foreach (var entity in asset.Hierarchy.Entities.Where(it => it.Entity.Name == "D"))
            {
                // Check that we have the correct baesId and basePartInstanceId
                Assert.True(entity.Design.BasePartInstanceId.HasValue);
                Assert.True(entity.Design.BaseId.HasValue);
                Assert.AreEqual(entityD.Id, entity.Design.BaseId.Value);

                // Make sure that the entity is in the RootEntities
                Assert.True(asset.Hierarchy.RootEntities.Contains(entity.Entity.Id));
            }

            var entityDesignD1 = asset.Hierarchy.Entities.FirstOrDefault(it => it.Entity.Name == "D" && it.Design.BasePartInstanceId == part1.Id);
            Assert.NotNull(entityDesignD1);

            var entityDesignD2 = asset.Hierarchy.Entities.FirstOrDefault(it => it.Entity.Name == "D" && it.Design.BasePartInstanceId == part12.Id);
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
            part1.Hierarchy.Entities.Add(new EntityDesign(eRoot, new EntityDesignData()));
            part1.Hierarchy.Entities.Add(new EntityDesign(entityA, new EntityDesignData()));
            part1.Hierarchy.Entities.Add(new EntityDesign(entityB, new EntityDesignData()));
            part1.Hierarchy.Entities.Add(new EntityDesign(entityC, new EntityDesignData()));
            part1.Hierarchy.RootEntities.Add(eRoot.Id);

            // part2 Asset
            var part2 = (PrefabAsset)AssetCloner.Clone(part1);
            var eRootPart2 = part2.Hierarchy.Entities.Where(it => it.Entity.Name == "Root").Select(it => it.Entity).First();

            var entityD = new Entity() { Name = "D" };
            eRootPart2.Transform.Children.Add(entityD.Transform);

            // ED + link: EB
            var entityBFrom2 = part2.Hierarchy.Entities.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            entityD.Add(new TestEntityComponent() { EntityLink = entityBFrom2 });
            part2.Hierarchy.Entities.Add(new EntityDesign(entityD, new EntityDesignData()));

            // originalAsset: Add a new instanceId for this part
            var asset = new PrefabAsset();

            // Create derived parts
            var eRoot1Asset = (PrefabAsset)part1.CreateChildAsset("part");
            var eRoot2Asset = (PrefabAsset)part1.CreateChildAsset("part");
            asset.AddPart(eRoot1Asset);
            asset.AddPart(eRoot2Asset);

            var eRoot2 = asset.Hierarchy.Entities[eRoot2Asset.Hierarchy.RootEntities[0]];

            var entityToRemove = eRoot2.Entity.Transform.Children.First(it => it.Entity.Name == "B");
            eRoot2.Entity.Transform.Children.Remove(entityToRemove);
            asset.Hierarchy.Entities.Remove(entityToRemove.Entity.Id);

            // Merge entities (NOTE: it is important to clone baseAsset/newBaseAsset)
            var entityMerge = asset.Merge(null, null, new List<AssetBase>() { new AssetBase("part", part2) } );
            Assert.False(entityMerge.HasErrors);

            // EntityD must be now part of the new asset
            Assert.AreEqual(2, asset.Hierarchy.RootEntities.Count);
            Assert.AreEqual(9, asset.Hierarchy.Entities.Count);
            Assert.AreEqual(2, asset.Hierarchy.Entities.Count(it => it.Entity.Name == "D"));

            foreach (var entity in asset.Hierarchy.Entities.Where(it => it.Entity.Name == "D"))
            {
                // Check that we have the correct baesId and basePartInstanceId
                Assert.True(entity.Design.BasePartInstanceId.HasValue);
                Assert.True(entity.Design.BaseId.HasValue);
                Assert.AreEqual(entityD.Id, entity.Design.BaseId.Value);
            }

            var entityDesignD1 = asset.Hierarchy.Entities[asset.Hierarchy.Entities[asset.Hierarchy.RootEntities[0]].Entity.Transform.Children.Where(it => it.Entity.Name == "D").Select(it => it.Entity.Id).FirstOrDefault()];
            Assert.NotNull(entityDesignD1);
            Assert.AreEqual(eRoot1Asset.Id, entityDesignD1.Design.BasePartInstanceId);
            var testComponentD1 = entityDesignD1.Entity.Get<TestEntityComponent>();
            Assert.NotNull(testComponentD1);
            var entityB1 = asset.Hierarchy.Entities[asset.Hierarchy.RootEntities[0]].Entity.Transform.Children.Where(it => it.Entity.Name == "B").Select(it => it.Entity).First();
            Assert.AreEqual(entityB1, testComponentD1.EntityLink);

            var entityDesignD2 = asset.Hierarchy.Entities[asset.Hierarchy.Entities[asset.Hierarchy.RootEntities[1]].Entity.Transform.Children.Where(it => it.Entity.Name == "D").Select(it => it.Entity.Id).FirstOrDefault()];
            Assert.NotNull(entityDesignD2);
            Assert.AreEqual(eRoot2Asset.Id, entityDesignD2.Design.BasePartInstanceId);
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
            a1.Hierarchy.Entities.Add(ea);
            a1.Hierarchy.Entities.Add(eb);
            a1.Hierarchy.RootEntities.Add(ea.Id);
            a1.Hierarchy.RootEntities.Add(eb.Id);

            var a2 = new PrefabAsset();
            var aPartInstance1 = (PrefabAsset)a1.CreateChildAsset("a1");
            var aPartInstance2 = (PrefabAsset)a1.CreateChildAsset("a1");
            a2.AddPart(aPartInstance1);
            a2.AddPart(aPartInstance2);

            // Modify a1 to add entity ec
            var ec = new Entity("ec");
            a1.Hierarchy.Entities.Add(ec);
            a1.Hierarchy.RootEntities.Add(ec.Id);

            var a3 = (PrefabAsset)a2.CreateChildAsset("a2");

            // Merge a2
            var result2 = a2.Merge(null, null, new List<AssetBase>()
            {
                new AssetBase("a1", (Asset)AssetCloner.Clone(a1))
            });

            Assert.False(result2.HasErrors);
            Assert.AreEqual(6, a2.Hierarchy.RootEntities.Count);
            Assert.True(a2.Hierarchy.Entities.All(it => it.Design.BaseId.HasValue && it.Design.BasePartInstanceId.HasValue));

            // Merge a3
            var result3 = a3.Merge((Asset)AssetCloner.Clone(a3.Base.Asset), (Asset)AssetCloner.Clone(a2), null);

            Assert.False(result3.HasErrors);
            Assert.AreEqual(6, a3.Hierarchy.RootEntities.Count);
            Assert.True(a3.Hierarchy.Entities.All(it => !it.Design.BasePartInstanceId.HasValue));
            Assert.True(a3.Hierarchy.Entities.All(it => it.Design.BaseId.HasValue && a2.Hierarchy.Entities.ContainsKey(it.Design.BaseId.Value)));
        }

        [Test]
        public void TestPackageAssetTemplatingAnalysis()
        {
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
            a1.Hierarchy.Entities.Add(ea);
            a1.Hierarchy.Entities.Add(eb);
            a1.Hierarchy.RootEntities.Add(ea.Id);
            a1.Hierarchy.RootEntities.Add(eb.Id);

            assetItems.Add(new AssetItem("a1", a1));

            var a2 = new PrefabAsset();
            var aPartInstance1 = (PrefabAsset)a1.CreateChildAsset("a1");
            var aPartInstance2 = (PrefabAsset)a1.CreateChildAsset("a1");
            a2.AddPart(aPartInstance1);
            a2.AddPart(aPartInstance2);
            assetItems.Add(new AssetItem("a2", a2));

            // Modify a1 to add entity ec
            var ec = new Entity("ec");
            a1.Hierarchy.Entities.Add(ec);
            a1.Hierarchy.RootEntities.Add(ec.Id);

            var a3 = (PrefabAsset)a2.CreateChildAsset("a2");

            assetItems.Add(new AssetItem("a3", a3));

            // Create a session with this project
            using (var session = new PackageSession())
            {
                var logger = new LoggerResult();
                session.AddExistingPackage(package, logger);

                Assert.False(logger.HasErrors);

                Assert.AreEqual(6, a2.Hierarchy.RootEntities.Count);
                Assert.True(a2.Hierarchy.Entities.All(it => it.Design.BaseId.HasValue && it.Design.BasePartInstanceId.HasValue));

                Assert.AreEqual(6, a3.Hierarchy.RootEntities.Count);
                Assert.True(a3.Hierarchy.Entities.All(it => !it.Design.BasePartInstanceId.HasValue));
                Assert.True(a3.Hierarchy.Entities.All(it => it.Design.BaseId.HasValue && a2.Hierarchy.Entities.ContainsKey(it.Design.BaseId.Value)));
            }
        }
    }
}