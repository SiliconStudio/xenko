// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core.IO;

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

            var asset3 = assetItem1.CreateChildAsset();
            var assetItem3 = new AssetItem("asset-3", asset3);

            asset1.Reference = new AssetReference<AssetObjectTest>(assetItem2.Id, assetItem2.Location);

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
                var copyBase = asset3.Base;
                asset3.Base = null;
                assetItem3.IsDirty = true;
                {
                    var assets = dependencyManager.FindAssetsInheritingFrom(asset1.Id);
                    Assert.AreEqual(0, assets.Count);
                }

                // Add back the inheritance
                asset3.Base = copyBase;
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
            asset1.Reference = new AssetReference<AssetObjectTest>(assetItem2.Id, assetItem2.Location);
            asset2.Reference = new AssetReference<AssetObjectTest>(assetItem3.Id, assetItem3.Location);
            asset3.Reference = new AssetReference<AssetObjectTest>(assetItem4.Id, assetItem4.Location);
            asset4.Reference = new AssetReference<AssetObjectTest>(assetItem1.Id, assetItem1.Location);

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
                var dependenciesFirst = dependencyManager.Find(assetItem1);
                Assert.AreEqual(1, dependenciesFirst.Parents.Count);
                var copyItem = dependenciesFirst.Parents.FirstOrDefault();
                Assert.NotNull(copyItem);
                Assert.AreEqual(assetItem4.Id, copyItem.Id);

                // Check direct output references
                Assert.AreEqual(1, dependenciesFirst.Count);
                copyItem = dependenciesFirst.FirstOrDefault();
                Assert.NotNull(copyItem);
                Assert.AreEqual(assetItem2.Id, copyItem.Id);

                // Calculate full recursive references
                var dependencies = dependencyManager.ComputeDependencies(assetItem1);

                // Check all input references (recursive)
                var asset1RecursiveInputs = dependencies.Parents.OrderBy(item => item.Location).ToList();
                Assert.AreEqual(4, dependencies.Count);
                Assert.AreEqual(assetItem1.Id, asset1RecursiveInputs[0].Id);
                Assert.AreEqual(assetItem2.Id, asset1RecursiveInputs[1].Id);
                Assert.AreEqual(assetItem3.Id, asset1RecursiveInputs[2].Id);
                Assert.AreEqual(assetItem4.Id, asset1RecursiveInputs[3].Id);

                // Check all output references (recursive)
                var asset1RecursiveOutputs = dependencies.OrderBy(item => item.Location).ToList();
                Assert.AreEqual(4, asset1RecursiveOutputs.Count);
                Assert.AreEqual(assetItem1.Id, asset1RecursiveInputs[0].Id);
                Assert.AreEqual(assetItem2.Id, asset1RecursiveInputs[1].Id);
                Assert.AreEqual(assetItem3.Id, asset1RecursiveInputs[2].Id);
                Assert.AreEqual(assetItem4.Id, asset1RecursiveInputs[3].Id);
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
            asset1.Reference = new AssetReference<AssetObjectTest>(assetItem2.Id, assetItem2.Location);
            asset3.Reference = new AssetReference<AssetObjectTest>(assetItem4.Id, assetItem4.Location);

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
                    var missingReferences = dependencyManager.FindMissingReferences(assetItem3).ToList();
                    Assert.AreEqual(1, missingReferences.Count);
                    Assert.AreEqual(asset4.Id, missingReferences[0].Id);

                    // Check references for: asset1 => asset2
                    var referencesFromAsset1 = dependencyManager.ComputeDependencies(assetItem1);
                    Assert.AreEqual(1, referencesFromAsset1.Count);
                    var copyItem = referencesFromAsset1.FirstOrDefault();
                    Assert.NotNull(copyItem);
                    Assert.AreEqual(assetItem2.Id, copyItem.Id);
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
                Assert.AreEqual(2, dependencyManager.Dependencies.Count); // asset1, asset2, asset3, asset4
                Assert.AreEqual(0, dependencyManager.AssetsWithMissingReferences.Count);
                Assert.AreEqual(0, dependencyManager.MissingReferencesToParent.Count);

                asset1.Reference = new AssetReference<AssetObjectTest>(assetItem2.Id, assetItem2.Location);

                // Mark the asset dirty
                assetItem1.IsDirty = true;

                var dependencies1 = dependencyManager.FindDependencySet(asset1.Id);
                var copyItem = dependencies1.FirstOrDefault();
                Assert.NotNull(copyItem);
                Assert.AreEqual(assetItem2.Id, copyItem.Id);

                var dependencies2 = dependencyManager.FindDependencySet(asset2.Id);
                copyItem = dependencies2.Parents.FirstOrDefault();
                Assert.NotNull(copyItem);
                Assert.AreEqual(assetItem1.Id, copyItem.Id);
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

            asset1.Reference = new AssetReference<AssetObjectTest>(assetItem2.Id, assetItem2.Location);
            asset3.Reference = new AssetReference<AssetObjectTest>(assetItem1.Id, assetItem1.Location);

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
                    var dependencySetAsset1 = dependencyManager.FindDependencySet(asset1.Id);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(0, dependencySetAsset1.Count);
                    Assert.IsTrue(dependencySetAsset1.HasMissingReferences);
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.MissingReferences.First().Id);
                }

                // Add asset2
                project.Assets.Add(assetItem2);
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(0, assets.Count);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.FindDependencySet(asset1.Id);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.Count);
                    Assert.AreEqual(0, dependencySetAsset1.Parents.Count);
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.First().Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.FindDependencySet(asset2.Id);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.Count);
                    Assert.AreEqual(1, dependencySetAsset2.Parents.Count);
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.Parents.First().Id);
                }

                // Add asset3
                project.Assets.Add(assetItem3);
                Action checkAllOk = () =>
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(0, assets.Count);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.FindDependencySet(asset1.Id);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.Count);
                    Assert.AreEqual(1, dependencySetAsset1.Parents.Count);
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.First().Id);
                    Assert.AreEqual(asset3.Id, dependencySetAsset1.Parents.First().Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.FindDependencySet(asset2.Id);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.Count);
                    Assert.AreEqual(1, dependencySetAsset2.Parents.Count);
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.Parents.First().Id);

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.FindDependencySet(asset3.Id);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(1, dependencySetAsset3.Count);
                    Assert.AreEqual(0, dependencySetAsset3.Parents.Count);
                    Assert.AreEqual(asset1.Id, dependencySetAsset3.First().Id);
                };
                checkAllOk();

                // Remove asset1
                project.Assets.Remove(assetItem1);
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0]);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.FindDependencySet(asset2.Id);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.Count);
                    Assert.AreEqual(0, dependencySetAsset2.Parents.Count);

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.FindDependencySet(asset3.Id);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(0, dependencySetAsset3.Count);
                    Assert.AreEqual(0, dependencySetAsset3.Parents.Count);
                    Assert.IsTrue(dependencySetAsset3.HasMissingReferences);
                    Assert.AreEqual(asset1.Id, dependencySetAsset3.MissingReferences.First().Id);
                }

                // Add asset1
                project.Assets.Add(assetItem1);
                checkAllOk();

                // Modify reference asset3 to asset1 with fake asset
                var previousAsset3ToAsset1Reference = asset3.Reference;
                asset3.Reference = new AssetReference<AssetObjectTest>(Guid.NewGuid(), "fake");
                assetItem3.IsDirty = true;
                {
                    var assets = dependencyManager.FindAssetsWithMissingReferences().ToList();
                    Assert.AreEqual(1, assets.Count);
                    Assert.AreEqual(asset3.Id, assets[0]);

                    // Check dependencies on asset1
                    var dependencySetAsset1 = dependencyManager.FindDependencySet(asset1.Id);
                    Assert.NotNull(dependencySetAsset1);

                    Assert.AreEqual(1, dependencySetAsset1.Count);
                    Assert.AreEqual(0, dependencySetAsset1.Parents.Count);
                    Assert.AreEqual(asset2.Id, dependencySetAsset1.First().Id);

                    // Check dependencies on asset2
                    var dependencySetAsset2 = dependencyManager.FindDependencySet(asset2.Id);
                    Assert.NotNull(dependencySetAsset2);

                    Assert.AreEqual(0, dependencySetAsset2.Count);
                    Assert.AreEqual(1, dependencySetAsset2.Parents.Count);
                    Assert.AreEqual(asset1.Id, dependencySetAsset2.Parents.First().Id);

                    // Check dependencies on asset3
                    var dependencySetAsset3 = dependencyManager.FindDependencySet(asset3.Id);
                    Assert.NotNull(dependencySetAsset3);

                    Assert.AreEqual(0, dependencySetAsset3.Count);
                    Assert.AreEqual(0, dependencySetAsset3.Parents.Count);
                    Assert.IsTrue(dependencySetAsset3.HasMissingReferences);
                    Assert.AreEqual(asset3.Reference.Id, dependencySetAsset3.MissingReferences.First().Id);
                }

                // Revert back reference from asset3 to asset1
                asset3.Reference = previousAsset3ToAsset1Reference;
                assetItem3.IsDirty = true;
                checkAllOk();
            }
        }

        [Test, Ignore]
        public void TestTrackingPackageWithAssetsAndSave()
        {
            var dirPath = Path.Combine(Environment.CurrentDirectory, DirectoryTestBase + @"TestTracking");
            TryDeleteDirectory(dirPath);

            string testGenerated1 = Path.Combine(dirPath, "TestTracking.pdxpkg");

            var project = new Package { FullPath = testGenerated1 };
            project.Profiles.Add(new PackageProfile("Shared", new AssetFolder(".")));
            var asset1 = new AssetObjectTest();
            var assetItem1 = new AssetItem("asset-1", asset1);
            project.Assets.Add(assetItem1);

            using (var session = new PackageSession(project))
            {

                var dependencies = session.DependencyManager;
                dependencies.TrackingSleepTime = 10;
                dependencies.EnableTracking = true;

                // Save the session
                {
                    var result = session.Save();
                    Assert.IsFalse(result.HasErrors);

                    // Wait enough time for events
                    Thread.Sleep(100);

                    // Make sure that save is not generating events
                    var events = dependencies.FindAssetFileChangedEvents().ToList();
                    Assert.AreEqual(0, events.Count);

                    // Check tracked directories
                    var directoriesTracked = dependencies.DirectoryWatcher.GetTrackedDirectories();
                    Assert.AreEqual(1, directoriesTracked.Count);
                    Assert.AreEqual(dirPath.ToLowerInvariant(), directoriesTracked[0].ToLowerInvariant());

                    // Simulate multiple change an asset on the disk
                    File.SetLastWriteTime(assetItem1.FullPath, DateTime.Now);
                    Thread.Sleep(100);

                    // Check that we are capturing this event
                    events = dependencies.FindAssetFileChangedEvents().ToList();
                    Assert.AreEqual(1, events.Count);
                    Assert.AreEqual(assetItem1.Location, events[0].AssetLocation);
                    Assert.AreEqual(AssetFileChangedType.Updated, events[0].ChangeType);
                }

                // Save the project to another location
                {
                    var dirPath2 = Path.Combine(Environment.CurrentDirectory, DirectoryTestBase + @"TestTracking2");
                    TryDeleteDirectory(dirPath2);
                    string testGenerated2 = Path.Combine(dirPath2, "TestTracking.pdxpkg");

                    project.FullPath = testGenerated2;
                    var result = session.Save();
                    Assert.IsFalse(result.HasErrors);

                    // Wait enough time for events
                    Thread.Sleep(200);

                    // Make sure that save is not generating events
                    var events = dependencies.FindAssetFileChangedEvents().ToList();
                    Assert.AreEqual(0, events.Count);

                    // Check tracked directories
                    var directoriesTracked = dependencies.DirectoryWatcher.GetTrackedDirectories();
                    Assert.AreEqual(1, directoriesTracked.Count);
                    Assert.AreEqual(dirPath2.ToLowerInvariant(), directoriesTracked[0].ToLowerInvariant());
                }

                // Copy file to simulate a new file on the disk (we will not try to load it as it has the same guid 
                {
                    var fullPath = assetItem1.FullPath;
                    var newPath = Path.Combine(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + "2" + Path.GetExtension(fullPath));
                    File.Copy(fullPath, newPath);

                    // Wait enough time for events
                    Thread.Sleep(200);

                    // Make sure that save is not generating events
                    var events = dependencies.FindAssetFileChangedEvents().ToList();
                    Assert.AreEqual(1, events.Count);
                    Assert.IsTrue((events[0].ChangeType & AssetFileChangedType.Added) != 0);
                }
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                try
                {
                    // bug with Directory.Delete not always working
                    // Trying our best to make sure the directory will be deleted, so
                    // we delete all files manually before
                    foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    Directory.Delete(path, true);
                }
                catch (Exception)
                {
                    Console.WriteLine("Warning, unable to delete directory [{0}]", path);
                }
            }
        }
    }
}