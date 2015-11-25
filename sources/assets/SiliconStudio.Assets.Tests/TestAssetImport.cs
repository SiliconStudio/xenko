// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetImport : TestBase
    {
        [TestFixtureSetUp]
        public void Initialize()
        {
        }

        [Test]
        public void CheckImporter()
        {
            var importer = AssetRegistry.FindImporterForFile("test.tmp").FirstOrDefault();
            Assert.NotNull(importer);
        }

        [Test]
        public void TestImportSessionSimple()
        {
            var name = "TestAssetImport";
            var file = Path.Combine(Path.GetTempPath(), name + ".tmp");
            const string fileContent = "This is the file content";
            File.WriteAllText(file, fileContent);
            var fileHash = ObjectId.FromBytes(Encoding.UTF8.GetBytes(fileContent));

            // Create a project with an asset reference a raw file
            var project = new Package();
            using (var session = new PackageSession(project))
            {
                var importSession = new AssetImportSession(session);

                // ------------------------------------------------------------------
                // Step 1: Add files to session
                // ------------------------------------------------------------------
                importSession.AddFile(file, project, UDirectory.Empty);

                // ------------------------------------------------------------------
                // Step 2: Stage assets
                // ------------------------------------------------------------------
                var stageResult = importSession.Stage();
                Assert.IsTrue(stageResult);
                Assert.AreEqual(0, project.Assets.Count);

                // ------------------------------------------------------------------
                // Step 3: Import asset directly (we don't try to merge)
                // ------------------------------------------------------------------
                importSession.Import();
                Assert.AreEqual(3, project.Assets.Count);
                var assetItem = project.Assets.FirstOrDefault(item => item.Asset is AssetImport);
                Assert.NotNull(assetItem);
                var importedAsset = (AssetImport)assetItem.Asset;

                Assert.IsInstanceOf<AssetImport>(importedAsset.Base.Asset);
                Assert.AreEqual((string)AssetBase.DefaultImportBase, importedAsset.Base.Location);

                // ------------------------------------------------------------------
                // Reset the import session
                // ------------------------------------------------------------------
                importSession.Reset();

                // Get the list of asset already in the project
                var ids = project.Assets.Select(item => item.Id).ToList();
                ids.Sort();

                // ------------------------------------------------------------------
                // Step 1: Add same file to the session
                // ------------------------------------------------------------------
                importSession.AddFile(file, project, UDirectory.Empty);

                // ------------------------------------------------------------------
                // Step 2: Stage assets
                // ------------------------------------------------------------------
                stageResult = importSession.Stage();

                // Select the previous item so that we will perform a merge
                foreach (var fileToImport in importSession.Imports)
                {
                    foreach (var import in fileToImport.ByImporters)
                    {
                        foreach (var assetImportMergeGroup in import.Items)
                        {
                            Assert.AreEqual(1, assetImportMergeGroup.Merges.Count);

                            // Check that we are matching correctly a previously imported asset
                            var previousItem = assetImportMergeGroup.Merges[0].PreviousItem;
                            Assert.IsTrue(project.Assets.ContainsById(previousItem.Id));

                            // Select the previous asset
                            assetImportMergeGroup.SelectedItem = previousItem;
                        }
                    }
                }

                // ------------------------------------------------------------------
                // Step 3: Merge the asset specified by the previous step
                // ------------------------------------------------------------------
                importSession.Merge();

                Assert.IsTrue(stageResult);
                Assert.AreEqual(3, project.Assets.Count);

                // ------------------------------------------------------------------
                // Step 4: Import merged asset
                // ------------------------------------------------------------------
                importSession.Import();
                Assert.AreEqual(3, project.Assets.Count);

                // Get the list of asset already in the project
                var newIds = project.Assets.Select(item => item.Id).ToList();
                newIds.Sort();

                // Check that we have exactly the same number of assets
                Assert.AreEqual(ids, newIds);

                // Check that new AssetObjectTestRaw.Value is setup to the new value (1, was previously 0)
                var assetRaw = project.Assets.Select(item => item.Asset).OfType<AssetObjectTestSub>().FirstOrDefault();
                Assert.IsNotNull(assetRaw);
                Assert.IsNotNull(assetRaw.Base);
                Assert.IsInstanceOf<AssetObjectTestSub>(assetRaw.Base.Asset);

                var assetRawBase = (AssetObjectTestSub)assetRaw.Base.Asset;
                Assert.AreEqual(1, assetRaw.Value);
                Assert.AreEqual(1, assetRawBase.Value);
            }
        }

        [Test]
        public void TestImportWithDuplicateWithinSameSession()
        {
            var name = "TestAssetImport";
            var file = Path.Combine(Path.GetTempPath(), name + ".tmp");
            const string fileContent = "This is the file content";
            File.WriteAllText(file, fileContent);

            var fileHash = ObjectId.FromBytes(Encoding.UTF8.GetBytes(fileContent));

            // Create a project with an asset reference a raw file
            var project = new Package();
            using (var session = new PackageSession(project))
            {
                var importSession = new AssetImportSession(session);

                // ------------------------------------------------------------------
                // Step 1: Add files to session
                // ------------------------------------------------------------------
                importSession.AddFile(file, project, UDirectory.Empty);
                // Simulate using another importer to duplicate the assets
                importSession.AddFile(file, new CustomImporter(), project, UDirectory.Empty);
                Assert.AreEqual(3, importSession.Imports[0].ByImporters.Count);

                // ------------------------------------------------------------------
                // Step 2: Stage assets
                // ------------------------------------------------------------------
                var stageResult = importSession.Stage();

                //importSession.Imports[0].ByImporters[1].Items[0].SelectedItem = importSession.Imports[0].ByImporters[1].Items[0].Merges[0].PreviousItem;
                // Merge 0 into 1
                importSession.Imports[0].ByImporters[0].Items[0].SelectedItem = importSession.Imports[0].ByImporters[0].Items[0].Merges[0].PreviousItem;

                // ------------------------------------------------------------------
                // Step 4: Merge the asset specified by the previous step
                // ------------------------------------------------------------------
                importSession.Merge();

                Assert.IsTrue(stageResult);

                // ------------------------------------------------------------------
                // Step 3: Import merged asset
                // ------------------------------------------------------------------
                var result = importSession.Import();
                Assert.AreEqual(4, project.Assets.Count);

                // TODO Add more tests
            }
        }
    }

    public class CustomImporter : AssetImporterBase
    {
        // Supported file extensions for this importer
        public const string FileExtensions = ".tmp";
        private int value = 0;

        private static readonly Guid uid = new Guid("9bf31c9a-270f-427e-b76c-b16efd5c9004");
        public override Guid Id
        {
            get
            {
                return uid;
            }
        }

        public override string Description
        {
            get
            {
                return "CustomImporter";
            }
        }

        public override string SupportedFileExtensions
        {
            get
            {
                return FileExtensions;
            }
        }

        public override AssetImporterParameters GetDefaultParameters(bool isForReImport)
        {
            return new AssetImporterParameters(typeof(AssetImportObjectTest), typeof(AssetObjectTestSub));
        }

        public override IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters)
        {
            // Creates the url to the texture
            var asset = new AssetImportObjectTest
                {
                    Source = rawAssetPath, 
                    Name = rawAssetPath.GetFileName() + "Name"
                };

            var assetUrl = new UFile(rawAssetPath.GetFileName(), null);

            // Emulate a change in a sub-asset
            var subAsset = new AssetObjectTestSub() { Value = value };
            value++;

            var subAssetItem = new AssetItem(rawAssetPath.GetFileName() + "_SubAsset", subAsset);

            asset.References.Add("Test", new AssetReference(subAsset.Id, subAssetItem.Location));

            var list = new List<AssetItem>
                {
                    new AssetItem(assetUrl, asset),
                    subAssetItem
                };

            return list;
        }
    }
}