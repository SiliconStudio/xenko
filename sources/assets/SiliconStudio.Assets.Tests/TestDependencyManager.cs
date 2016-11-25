// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Analysis;

namespace SiliconStudio.Assets.Tests
{
    /// <summary>
    /// Test class for <see cref="AssetDependencyManager"/>.
    /// </summary>
    [TestFixture]
    public class TestDependencyManager : TestBase
    {
        [Test]
        public void TestInheritance()
        {
            // -----------------------------------------------------------
            // Tests inheritance
            // -----------------------------------------------------------
            // 4 assets
            // [asset1] is referencing [asset2]
            // [asset2]
            // [asset3] is inheriting  [asset1]
            // We create a [project1] with [asset1, asset2, asset3]
            // Check direct inherit dependencies for [asset3]: [asset1]
            // -----------------------------------------------------------

            var asset1 = new AssetObjectTest();
            var asset2 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            var assetItem2 = new AssetItem("asset-2", asset2);

            var asset3 = assetItem1.CreateDerivedAsset();
            var assetItem3 = new AssetItem("asset-3", asset3);

            asset1.Reference = new AssetReference(assetItem2.Id, assetItem2.Location);

            var project = new Package();
            project.Assets.Add(assetItem1);
            project.Assets.Add(assetItem2);
            project.Assets.Add(assetItem3);

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                // Verify inheritance
                {
                    var assets = dependencyManager.FindAssetsInheritingFrom(asset1.Id);
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0].Id);
                }

                // Remove the inheritance
                var copyBase = asset3.Archetype;
                asset3.Archetype = null;
                assetItem3.IsDirty = true;
                {
                    var assets = dependencyManager.FindAssetsInheritingFrom(asset1.Id);
                    Assert.AreEqual(0, assets.Count);
                }

                // Add back the inheritance
                asset3.Archetype = copyBase;
                assetItem3.IsDirty = true;
                {
                    var assets = dependencyManager.FindAssetsInheritingFrom(asset1.Id);
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0].Id);
                }
            }
        }

        [Test]
        public void TestCircularAndRecursiveDependencies()
        {
            // -----------------------------------------------------------
            // Tests circular references and dependencies query
            // -----------------------------------------------------------
            // 4 assets
            // [asset1] is referencing [asset2]
            // [asset2] is referencing [asset3]
            // [asset3] is referencing [asset4]
            // [asset4] is referencing [asset1]
            // We create a [project1] with [asset1, asset2, asset3, asset4]
            // Check direct input dependencies for [asset1]: [asset4]
            // Check all all input dependencies for [asset1]: [asset4, asset3, asset2, asset1]
            // Check direct output dependencies for [asset1]: [asset2]
            // Check all all output dependencies for [asset1]: [asset2, asset3, asset4, asset1]
            // -----------------------------------------------------------

            var asset1 = new AssetObjectTest();
            var asset2 = new AssetObjectTest();
            var asset3 = new AssetObjectTest();
            var asset4 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            var assetItem2 = new AssetItem("asset-2", asset2);
            var assetItem3 = new AssetItem("asset-3", asset3);
            var assetItem4 = new AssetItem("asset-4", asset4);
            asset1.Reference = new AssetReference(assetItem2.Id, assetItem2.Location);
            asset2.Reference = new AssetReference(assetItem3.Id, assetItem3.Location);
            asset3.Reference = new AssetReference(assetItem4.Id, assetItem4.Location);
            asset4.Reference = new AssetReference(assetItem1.Id, assetItem1.Location);

            var project = new Package();
            project.Assets.Add(assetItem1);
            project.Assets.Add(assetItem2);
            project.Assets.Add(assetItem3);
            project.Assets.Add(assetItem4);

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                // Check internal states
                Assert.AreEqual(1, dependencyManager.Packages.Count); // only one project
                Assert.AreEqual(4, dependencyManager.Dependencies.Count); // asset1, asset2, asset3, asset4
                Assert.AreEqual(0, dependencyManager.AssetsWithMissingReferences.Count);
                Assert.AreEqual(0, dependencyManager.MissingReferencesToParent.Count);

                // Check direct input references
                var dependenciesFirst = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                Assert.AreEqual(1, dependenciesFirst.LinksIn.Count());
                var copyItem = dependenciesFirst.LinksIn.FirstOrDefault();
                Assert.NotNull(copyItem.Element);
                Assert.AreEqual(assetItem4.Id, copyItem.Item.Id);

                // Check direct output references
                Assert.AreEqual(1, dependenciesFirst.LinksOut.Count());
                copyItem = dependenciesFirst.LinksOut.FirstOrDefault();
                Assert.NotNull(copyItem.Element);
                Assert.AreEqual(assetItem2.Id, copyItem.Item.Id);

                // Calculate full recursive references
                var dependencies = dependencyManager.ComputeDependencies(assetItem1.Id);

                // Check all input references (recursive)
                var asset1RecursiveInputs = dependencies.LinksIn.OrderBy(item => item.Element.Location).ToList();
                Assert.AreEqual(4, dependencies.LinksOut.Count());
                Assert.AreEqual(assetItem1.Id, asset1RecursiveInputs[0].Item.Id);
                Assert.AreEqual(assetItem2.Id, asset1RecursiveInputs[1].Item.Id);
                Assert.AreEqual(assetItem3.Id, asset1RecursiveInputs[2].Item.Id);
                Assert.AreEqual(assetItem4.Id, asset1RecursiveInputs[3].Item.Id);

                // Check all output references (recursive)
                var asset1RecursiveOutputs = dependencies.LinksOut.OrderBy(item => item.Element.Location).ToList();
                Assert.AreEqual(4, asset1RecursiveOutputs.Count);
                Assert.AreEqual(assetItem1.Id, asset1RecursiveInputs[0].Element.Id);
                Assert.AreEqual(assetItem2.Id, asset1RecursiveInputs[1].Element.Id);
                Assert.AreEqual(assetItem3.Id, asset1RecursiveInputs[2].Element.Id);
                Assert.AreEqual(assetItem4.Id, asset1RecursiveInputs[3].Element.Id);
            }
        }

        [Test]
        public void TestFullSession()
        {
            // -----------------------------------------------------------
            // This is a more complex test mixing several different cases:
            // -----------------------------------------------------------
            // 4 assets
            // [asset1] is referencing [asset2]
            // [asset3] is referencing [asset4]
            // We create a [project1] with [asset1, asset2, asset3]
            // Start to evaluate the dependencies 
            // Check the dependencies for this project, [asset4] is missing
            // We create a [project2] and add it to the session
            // We add [asset4] to the [project2]
            // All depedencies should be fine
            // Remove [project2] from session
            // Check the dependencies for this project, [asset4] is missing
            // -----------------------------------------------------------

            var asset1 = new AssetObjectTest();
            var asset2 = new AssetObjectTest();
            var asset3 = new AssetObjectTest();
            var asset4 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            var assetItem2 = new AssetItem("asset-2", asset2);
            var assetItem3 = new AssetItem("asset-3", asset3);
            var assetItem4 = new AssetItem("asset-4", asset4);
            asset1.Reference = new AssetReference(assetItem2.Id, assetItem2.Location);
            asset3.Reference = new AssetReference(assetItem4.Id, assetItem4.Location);

            var project = new Package();
            project.Assets.Add(assetItem1);
            project.Assets.Add(assetItem2);
            project.Assets.Add(assetItem3);

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                // Check internal states
                Action checkState1 = () =>
                {
                    Assert.AreEqual(1, dependencyManager.Packages.Count); // only one project
                    Assert.AreEqual(3, dependencyManager.Dependencies.Count); // asset1, asset2, asset3
                    Assert.AreEqual(1, dependencyManager.AssetsWithMissingReferences.Count); // asset3 => asset4
                    Assert.AreEqual(1, dependencyManager.MissingReferencesToParent.Count); // asset4 => [asset3]

                    // Check missing references for asset3 => X asset4
                    var assetItemWithMissingReferences = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assetItemWithMissingReferences.Count);
                    Assert.AreEqual(assetItem3.Id, assetItemWithMissingReferences[0]);

                    // Check missing reference
                    var missingReferences = dependencyManager.FindMissingReferences(assetItem3.Id).ToList();
                    Assert.AreEqual(1, missingReferences.Count);
                    Assert.AreEqual(asset4.Id, missingReferences[0].Id);

                    // Check references for: asset1 => asset2
                    var referencesFromAsset1 = dependencyManager.ComputeDependencies(assetItem1.Id);
                    Assert.AreEqual(1, referencesFromAsset1.LinksOut.Count());
                    var copyItem = referencesFromAsset1.LinksOut.FirstOrDefault();
                    Assert.NotNull(copyItem.Element);
                    Assert.AreEqual(assetItem2.Id, copyItem.Element.Id);
                };
                checkState1();

                {
                    // Add new project (must be tracked by the dependency manager)
                    var project2 = new Package();
                    session.Packages.Add(project2);

                    // Check internal states
                    Assert.AreEqual(2, dependencyManager.Packages.Count);

                    // Add missing asset4
                    project2.Assets.Add(assetItem4);
                    var assetItemWithMissingReferences = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(0, assetItemWithMissingReferences.Count);

                    // Check internal states
                    Assert.AreEqual(4, dependencyManager.Dependencies.Count); // asset1, asset2, asset3, asse4
                    Assert.AreEqual(0, dependencyManager.AssetsWithMissingReferences.Count);
                    Assert.AreEqual(0, dependencyManager.MissingReferencesToParent.Count);

                    // Try to remove the project and double check
                    session.Packages.Remove(project2);

                    checkState1();
                }
            }
        }

        [Test]
        public void TestAssetChanged()
        {
            // -----------------------------------------------------------
            // Case where an asset is changing is referencing
            // -----------------------------------------------------------
            // 2 assets [asset1, asset2]
            // Change [asset1] referencing [asset2]
            // Notify the session to mark asset1 dirty
            // 
            // -----------------------------------------------------------

            var asset1 = new AssetObjectTest();
            var asset2 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            var assetItem2 = new AssetItem("asset-2", asset2);

            var project = new Package();
            project.Assets.Add(assetItem1);
            project.Assets.Add(assetItem2);

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                // Check internal states
                Assert.AreEqual(1, dependencyManager.Packages.Count); // only one project
                Assert.AreEqual(2, dependencyManager.Dependencies.Count); // asset1, asset2
                Assert.AreEqual(0, dependencyManager.AssetsWithMissingReferences.Count);
                Assert.AreEqual(0, dependencyManager.MissingReferencesToParent.Count);

                asset1.Reference = new AssetReference(assetItem2.Id, assetItem2.Location);

                // Mark the asset dirty
                assetItem1.IsDirty = true;

                var dependencies1 = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                var copyItem = dependencies1.LinksOut.FirstOrDefault();
                Assert.NotNull(copyItem.Element);
                Assert.AreEqual(assetItem2.Id, copyItem.Element.Id);

                var dependencies2 = dependencyManager.ComputeDependencies(assetItem2.Id, AssetDependencySearchOptions.InOut);
                copyItem = dependencies2.LinksIn.FirstOrDefault();
                Assert.NotNull(copyItem.Element);
                Assert.AreEqual(assetItem1.Id, copyItem.Element.Id);
            }
        }

        [Test]
        public void TestMissingReferences()
        {
            // -----------------------------------------------------------
            // Tests missing references
            // -----------------------------------------------------------
            // 3 assets
            // [asset1] is referencing [asset2]
            // [asset3] is referencing [asset1]
            // Add asset1. Check dependencies
            // Add asset2. Check dependencies
            // Add asset3. Check dependencies
            // Remove asset1. Check dependencies
            // Add asset1. Check dependencies.
            // Modify reference asset3 to asset1 with fake asset. Check dependencies
            // Revert reference asset3 to asset1. Check dependencies
            // -----------------------------------------------------------

            var asset1 = new AssetObjectTest();
            var asset2 = new AssetObjectTest();
            var asset3 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            var assetItem2 = new AssetItem("asset-2", asset2);
            var assetItem3 = new AssetItem("asset-3", asset3);

            asset1.Reference = new AssetReference(assetItem2.Id, assetItem2.Location);
            asset3.Reference = new AssetReference(assetItem1.Id, assetItem1.Location);

            var project = new Package();

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                // Add asset1
                project.Assets.Add(assetItem1);
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset1.Id, assets[0]);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(0, dependencySetAsset1.LinksOut.Count());
                    Assert.IsTrue(dependencySetAsset1.HasMissingDependencies);
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.BrokenLinksOut.First().Element.Id);
                }

                // Add asset2
                project.Assets.Add(assetItem2);
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(0, assets.Count);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset1.LinksIn.Count());
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.LinksOut.First().Element.Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.ComputeDependencies(assetItem2.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.LinksOut.Count());
                    Assert.AreEqual(1, dependencySetAsset2.LinksIn.Count());
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.LinksIn.First().Element.Id);
                }

                // Add asset3
                project.Assets.Add(assetItem3);
                Action checkAllOk = () =>
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(0, assets.Count);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.LinksOut.Count());
                    Assert.AreEqual(1, dependencySetAsset1.LinksIn.Count());
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.LinksOut.First().Element.Id);
                    Assert.AreEqual(asset3.Id, dependencySetAsset1.LinksIn.First().Element.Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.ComputeDependencies(assetItem2.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.LinksOut.Count());
                    Assert.AreEqual(1, dependencySetAsset2.LinksIn.Count());
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.LinksIn.First().Element.Id);

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.ComputeDependencies(assetItem3.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(1, dependencySetAsset3.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset3.LinksIn.Count());
                    Assert.AreEqual(asset1.Id, dependencySetAsset3.LinksOut.First().Element.Id);
                };
                checkAllOk();

                // Remove asset1
                project.Assets.Remove(assetItem1);
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0]);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.ComputeDependencies(assetItem2.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset2.LinksIn.Count());

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.ComputeDependencies(assetItem3.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(0, dependencySetAsset3.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset3.LinksIn.Count());
                    Assert.IsTrue(dependencySetAsset3.HasMissingDependencies);
                    Assert.AreEqual(asset1.Id, dependencySetAsset3.BrokenLinksOut.First().Element.Id);
                }

                // Add asset1
                project.Assets.Add(assetItem1);
                checkAllOk();

                // Modify reference asset3 to asset1 with fake asset
                var previousAsset3ToAsset1Reference = asset3.Reference;
                asset3.Reference = new AssetReference(AssetId.New(), "fake");
                assetItem3.IsDirty = true;
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0]);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.ComputeDependencies(assetItem1.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset1.LinksIn.Count());
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.LinksOut.First().Element.Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.ComputeDependencies(assetItem2.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.LinksOut.Count());
                    Assert.AreEqual(1, dependencySetAsset2.LinksIn.Count());
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.LinksIn.First().Element.Id);

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.ComputeDependencies(assetItem3.Id, AssetDependencySearchOptions.InOut);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(0, dependencySetAsset3.LinksOut.Count());
                    Assert.AreEqual(0, dependencySetAsset3.LinksIn.Count());
                    Assert.IsTrue(dependencySetAsset3.HasMissingDependencies);
                    Assert.AreEqual(asset3.Reference.Id, dependencySetAsset3.BrokenLinksOut.First().Element.Id);
                }

                // Revert back reference from asset3 to asset1
                asset3.Reference = previousAsset3ToAsset1Reference;
                assetItem3.IsDirty = true;
                checkAllOk();
            }
        }

        /// <summary>
        /// Tests the types of the links between elements.
        /// </summary>
        [Test]
        public void TestLinkType()
        {
            // -----------------------------------------------------------
            // Add dependencies of several types and check the links
            // -----------------------------------------------------------
            // 7 assets
            // A1 -- inherit from --> A0
            // A2 -- inherit from --> A1
            // A3 -- compose --> A1
            // A1 -- compose --> A4
            // A5 -- reference --> A1
            // A1 -- reference --> A6
            // 
            // Expected links on A1:
            // - In: A2(Inheritance), A3(Composition), A5(Reference)
            // - Out: A0(Inheritance), A4(Composition), A6(Reference)
            // - BrokenOut: 
            //
            // ------------------------------------
            // Remove all items except A1 and check missing reference types
            // -----------------------------------------------------------
            //
            // Expected broken out links
            // - BrokenOut: A0(Inheritance), A4(Composition), A6(Reference)
            //
            // ---------------------------------------------------------

            var project = new Package();
            var assets = new List<AssetObjectTest>();
            var assetItems = new List<AssetItem>();
            for (int i = 0; i < 7; ++i)
            {
                assets.Add(new AssetObjectTest { Parts = { new AssetPartTestItem { Id = Guid.NewGuid() } } });
                assetItems.Add(new AssetItem("asset-" + i, assets[i]));
                project.Assets.Add(assetItems[i]);
            }

            assets[1].Archetype = new AssetReference(assetItems[0].Id, assetItems[0].Location);
            assets[2].Archetype = new AssetReference(assetItems[1].Id, assetItems[1].Location);
            assets[3].Parts[0].Base = new BasePart(new AssetReference(assetItems[1].Id, assetItems[1].Location), assets[1].Parts[0].Id, Guid.NewGuid());
            assets[1].Parts[0].Base = new BasePart(new AssetReference(assetItems[4].Id, assetItems[4].Location), assets[4].Parts[0].Id, Guid.NewGuid());
            assets[5].Reference = CreateAssetReference(assetItems[1]);
            assets[1].Reference = CreateAssetReference(assetItems[6]);

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                var dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);

                Assert.AreEqual(3, dependencies.LinksIn.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkIn(assetItems[3]).Type);
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkIn(assetItems[5]).Type);

                Assert.AreEqual(3, dependencies.LinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkOut(assetItems[0]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkOut(assetItems[4]).Type);
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkOut(assetItems[6]).Type);
                
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());

                var count = assets.Count;
                for (int i = 0; i < count; i++)
                {
                    if (i != 1)
                        project.Assets.Remove(assetItems[i]);
                }

                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);

                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(0, dependencies.LinksOut.Count());

                Assert.AreEqual(3, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetBrokenLinkOut(assetItems[0].Id).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetBrokenLinkOut(assetItems[4].Id).Type);
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetBrokenLinkOut(assetItems[6].Id).Type);
            }
        }

        /// <summary>
        /// Tests the types of the links between elements during progressive additions.
        /// </summary>
        [Test]
        public void TestLinkTypeProgressive()
        {
            // -----------------------------------------------------------
            // Progressively add dependencies between elements and check the link types
            // -----------------------------------------------------------
            //
            // 3 assets:
            // A1 -- inherit from --> A0
            // A2 -- inherit from --> A1
            // A1 -- compose --> A0
            // A2 -- compose --> A1
            // A1 -- reference --> A0
            // A2 -- reference --> A1
            // 
            // ---------------------------------------------------------

            var project = new Package();
            var assets = new List<AssetObjectTest>();
            var assetItems = new List<AssetItem>();
            for (int i = 0; i < 3; ++i)
            {
                assets.Add(new AssetObjectTest { Parts = { new AssetPartTestItem { Id = Guid.NewGuid() } } });
                assetItems.Add(new AssetItem("asset-" + i, assets[i]));
                project.Assets.Add(assetItems[i]);
            }

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                var dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(0, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                
                assets[1].Reference = CreateAssetReference(assetItems[0]);
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Reference = CreateAssetReference(assetItems[1]);
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[1].Archetype = new AssetReference(assetItems[0].Id, assetItems[0].Location);
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Archetype = new AssetReference(assetItems[1].Id, assetItems[1].Location);
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[1].Parts[0].Base = new BasePart(new AssetReference(assetItems[0].Id, assetItems[0].Location), assets[0].Parts[0].Id, Guid.NewGuid());
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Parts[0].Base = new BasePart(new AssetReference(assetItems[1].Id, assetItems[1].Location), assets[1].Parts[0].Id, Guid.NewGuid());
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkOut(assetItems[0]).Type);

                project.Assets.Remove(assetItems[0]);
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(0, dependencies.LinksOut.Count());
                Assert.AreEqual(1, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.All, dependencies.GetBrokenLinkOut(assetItems[0].Id).Type);

                project.Assets.Remove(assetItems[2]);
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(0, dependencies.LinksOut.Count());
                Assert.AreEqual(1, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.All, dependencies.GetBrokenLinkOut(assetItems[0].Id).Type);

                project.Assets.Add(assetItems[0]);
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkOut(assetItems[0]).Type);

                project.Assets.Add(assetItems[2]);
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Archetype = null;
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.All, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[1].Archetype = null;
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Parts[0].Base = null;
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[1].Parts[0].Base = null;
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(1, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkIn(assetItems[2]).Type);
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[2].Reference = null;
                assetItems[2].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(1, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
                Assert.AreEqual(ContentLinkType.Reference, dependencies.GetLinkOut(assetItems[0]).Type);

                assets[1].Reference = null;
                assetItems[1].IsDirty = true;
                dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                Assert.AreEqual(0, dependencies.LinksIn.Count());
                Assert.AreEqual(0, dependencies.LinksOut.Count());
                Assert.AreEqual(0, dependencies.BrokenLinksOut.Count());
            }
        }

        private AssetReference CreateAssetReference(AssetItem item)
        {
            return new AssetReference(item.Id, item.Location);
        }

        /// <summary>
        /// Tests the <see cref="AssetDependencyManager.FindAssetsInheritingFrom"/> functions.
        /// </summary>
        [Test]
        //[Ignore("This test is currently broken and we do not have any scenario of inheritance + composition, nor decided if we actually want to support it.")]
        public void TestInheritFrom()
        {
            // -----------------------------------------------------------
            // Add dependencies of several types and check the InheritingFrom result
            // -----------------------------------------------------------
            // 8 assets
            // A1 -- inherit from --> A0
            // A2 -- inherit from --> A1
            // A3 -- inherit from --> A2
            // A8 -- inherit from --> A1
            // A1 -- reference --> A5
            // A4 -- reference --> A1
            // A1 -- compose --> A7
            // A2 -- compose --> A1
            // A6 -- compose --> A1
            // 
            // Results expected on A1:
            // - Inherit: A2, A8
            // - Composition: A2, A6
            // - All: A2, A6, A8
            // -----------------------------------------------------------

            var project = new Package();
            var assets = new List<AssetObjectTest>();
            var assetItems = new List<AssetItem>();
            for (int i = 0; i < 9; ++i)
            {
                assets.Add(new AssetObjectTest { Parts = { new AssetPartTestItem { Id = Guid.NewGuid() } } });
                assetItems.Add(new AssetItem("asset-" + i, assets[i]));
                project.Assets.Add(assetItems[i]);
            }

            assets[1].Archetype = new AssetReference(assetItems[0].Id, assetItems[0].Location);
            assets[2].Archetype = new AssetReference(assetItems[1].Id, assetItems[1].Location);
            assets[3].Archetype = new AssetReference(assetItems[2].Id, assetItems[2].Location);
            assets[8].Archetype = new AssetReference(assetItems[1].Id, assetItems[1].Location);
            assets[1].Reference = CreateAssetReference(assetItems[5]);
            assets[4].Reference = CreateAssetReference(assetItems[1]);
            assets[1].Parts[0].Base = new BasePart(new AssetReference(assetItems[7].Id, assetItems[7].Location), assets[7].Parts[0].Id, Guid.NewGuid());
            assets[2].Parts[0].Base = new BasePart(new AssetReference(assetItems[1].Id, assetItems[1].Location), assets[1].Parts[0].Id, Guid.NewGuid());
            assets[6].Parts[0].Base = new BasePart(new AssetReference(assetItems[1].Id, assetItems[1].Location), assets[1].Parts[0].Id, Guid.NewGuid());

            // Create a session with this project
            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                var children = dependencyManager.FindAssetsInheritingFrom(assets[1].Id, AssetInheritanceSearchOptions.Base);
                Assert.AreEqual(2, children.Count);
                Assert.IsTrue(children.Any(x=>x.Id == assets[2].Id));
                Assert.IsTrue(children.Any(x => x.Id == assets[8].Id));

                var compositionChildren = dependencyManager.FindAssetsInheritingFrom(assets[1].Id, AssetInheritanceSearchOptions.Composition);
                Assert.AreEqual(2, compositionChildren.Count);
                Assert.IsTrue(compositionChildren.Any(x => x.Id == assets[2].Id));
                Assert.IsTrue(compositionChildren.Any(x => x.Id == assets[6].Id));

                var all = dependencyManager.FindAssetsInheritingFrom(assets[1].Id);
                Assert.AreEqual(3, all.Count);
                Assert.IsTrue(all.Any(x => x.Id == assets[2].Id));
                Assert.IsTrue(all.Any(x => x.Id == assets[6].Id));
                Assert.IsTrue(all.Any(x => x.Id == assets[8].Id));
            }
        }

        /// <summary>
        /// Tests the links used for <see cref="ContentLinkType.CompositionInheritance"/>.
        /// </summary>
        [Test]
        public void TestCompositionsInAndOut()
        {
            // -----------------------------------------------------------
            // 3 assets
            // a1 : two parts
            // a2 (baseParts: a1, 2 instances -> 4 parts)
            // a3 (base: a2)
            // -----------------------------------------------------------

            var package = new Package();

            var assetItems = package.Assets;

            var a1 = new TestAssetWithParts();
            a1.Parts.Add(new AssetPartTestItem(Guid.NewGuid()));
            a1.Parts.Add(new AssetPartTestItem(Guid.NewGuid()));
            var a1Item = new AssetItem("a1", a1);
            assetItems.Add(a1Item);

            var a2 = new TestAssetWithParts();
            var aPartInstance1 = (TestAssetWithParts)a1.CreateDerivedAsset("a1");
            var aPartInstance2 = (TestAssetWithParts)a1.CreateDerivedAsset("a1");
            a2.AddParts(aPartInstance1);
            a2.AddParts(aPartInstance2);
            var a2Item = new AssetItem("a2", a2);
            assetItems.Add(a2Item);

            var a3 = a2.CreateDerivedAsset("a2");
            var a3Item = new AssetItem("a3", a3);
            assetItems.Add(a3Item);

            // Create a session with this project
            using (var session = new PackageSession(package))
            {
                var dependencyManager = session.DependencyManager;

                //var deps = dependencyManager.FindDependencySet(aPartInstance1.Parts[0].Id);
                var deps = dependencyManager.ComputeDependencies(a2Item.Id, AssetDependencySearchOptions.InOut);
                Assert.NotNull(deps);

                // The dependencies is the same as the a2 dependencies
                Assert.AreEqual(a2.Id, deps.Id);

                Assert.False(deps.HasMissingDependencies);

                Assert.AreEqual(1, deps.LinksIn.Count()); // a3 inherits from a2
                Assert.AreEqual(1, deps.LinksOut.Count()); // a2 use composition inheritance from a1

                var linkIn = deps.LinksIn.FirstOrDefault();
                Assert.AreEqual(a3.Id, linkIn.Item.Id);
                // a3 has a2 as archetype (Inheritance) and its parts are referencing a2 parts (CompositionInheritance)
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.Inheritance | ContentLinkType.CompositionInheritance, linkIn.Type);

                var linkOut = deps.LinksOut.FirstOrDefault();
                Assert.AreEqual(a1.Id, linkOut.Item.Id);
                Assert.AreEqual(ContentLinkType.Reference | ContentLinkType.CompositionInheritance, linkOut.Type);
            }
        }

        /// <summary>
        /// Tests that the asset cached in the session's dependency manager are correctly updated when IsDirty is set to true.
        /// </summary>
        [Test]
        public void TestCachedAssetUpdate()
        {
            // -----------------------------------------------------------
            // Change a property of A0 and see if the version of A0 returned by dependency computation from A1 is valid.
            // -----------------------------------------------------------
            // 2 assets
            // A1 -- inherit from --> A0
            // 
            // -----------------------------------------------------------
            
            var project = new Package();
            var assets = new List<AssetObjectTest>();
            var assetItems = new List<AssetItem>();
            for (int i = 0; i < 2; ++i)
            {
                assets.Add(new AssetObjectTest());
                assetItems.Add(new AssetItem("asset-" + i, assets[i]));
                project.Assets.Add(assetItems[i]);
            }

            assets[1].Archetype = new AssetReference(assetItems[0].Id, assetItems[0].Location);

            using (var session = new PackageSession(project))
            {
                var dependencyManager = session.DependencyManager;

                assets[0].RawAsset = "tutu";
                assetItems[0].IsDirty = true;

                var dependencies = dependencyManager.ComputeDependencies(assetItems[1].Id);
                var asset0 = dependencies.GetLinkOut(assetItems[0]);
                Assert.AreEqual(assets[0].RawAsset, ((AssetObjectTest)asset0.Item.Asset).RawAsset);
            }
        }

        //[Test, Ignore("Need check")]
        //public void TestTrackingPackageWithAssetsAndSave()
        //{
        //    var dirPath = Path.Combine(Environment.CurrentDirectory, DirectoryTestBase + @"TestTracking");
        //    TestHelper.TryDeleteDirectory(dirPath);

        //    string testGenerated1 = Path.Combine(dirPath, "TestTracking.xkpkg");

        //    var project = new Package { FullPath = testGenerated1 };
        //    project.Profiles.Add(new PackageProfile("Shared", new AssetFolder(".")));
        //    var asset1 = new AssetObjectTest();
        //    var assetItem1 = new AssetItem("asset-1", asset1);
        //    project.Assets.Add(assetItem1);

        //    using (var session = new PackageSession(project))
        //    {

        //        var dependencies = session.DependencyManager;
        //        dependencies.TrackingSleepTime = 10;
        //        dependencies.EnableTracking = true;

        //        // Save the session
        //        {
        //            var result = session.Save();
        //            Assert.IsFalse(result.HasErrors);

        //            // Wait enough time for events
        //            Thread.Sleep(100);

        //            // Make sure that save is not generating events
        //            var events = dependencies.FindAssetFileChangedEvents().ToList();
        //            Assert.AreEqual(0, events.Count);

        //            // Check tracked directories
        //            var directoriesTracked = dependencies.DirectoryWatcher.GetTrackedDirectories();
        //            Assert.AreEqual(1, directoriesTracked.Count);
        //            Assert.AreEqual(dirPath.ToLowerInvariant(), directoriesTracked[0].ToLowerInvariant());

        //            // Simulate multiple change an asset on the disk
        //            File.SetLastWriteTime(assetItem1.FullPath, DateTime.Now);
        //            Thread.Sleep(100);

        //            // Check that we are capturing this event
        //            events = dependencies.FindAssetFileChangedEvents().ToList();
        //            Assert.AreEqual(1, events.Count);
        //            Assert.AreEqual(assetItem1.Location, events[0].AssetLocation);
        //            Assert.AreEqual(AssetFileChangedType.Updated, events[0].ChangeType);
        //        }

        //        // Save the project to another location
        //        {
        //            var dirPath2 = Path.Combine(Environment.CurrentDirectory, DirectoryTestBase + @"TestTracking2");
        //            TestHelper.TryDeleteDirectory(dirPath2);
        //            string testGenerated2 = Path.Combine(dirPath2, "TestTracking.xkpkg");

        //            project.FullPath = testGenerated2;
        //            var result = session.Save();
        //            Assert.IsFalse(result.HasErrors);

        //            // Wait enough time for events
        //            Thread.Sleep(200);

        //            // Make sure that save is not generating events
        //            var events = dependencies.FindAssetFileChangedEvents().ToList();
        //            Assert.AreEqual(0, events.Count);

        //            // Check tracked directories
        //            var directoriesTracked = dependencies.DirectoryWatcher.GetTrackedDirectories();
        //            Assert.AreEqual(1, directoriesTracked.Count);
        //            Assert.AreEqual(dirPath2.ToLowerInvariant(), directoriesTracked[0].ToLowerInvariant());
        //        }

        //        // Copy file to simulate a new file on the disk (we will not try to load it as it has the same guid 
        //        {
        //            var fullPath = assetItem1.FullPath;
        //            var newPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "2" + Path.GetExtension(fullPath));
        //            File.Copy(fullPath, newPath);

        //            // Wait enough time for events
        //            Thread.Sleep(200);

        //            // Make sure that save is not generating events
        //            var events = dependencies.FindAssetFileChangedEvents().ToList();
        //            Assert.AreEqual(1, events.Count);
        //            Assert.IsTrue((events[0].ChangeType & AssetFileChangedType.Added) != 0);
        //        }
        //    }
        //}
    }
}