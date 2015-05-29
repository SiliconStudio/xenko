// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetUpgrade : TestBase
    {
        [DataContract("MyUpgradedAsset")]
        [AssetDescription(".pdxobj")]
        [AssetFormatVersion(5, 1)]
        [AssetUpgrader(1, 2, typeof(AssetUpgrader1))]
        [AssetUpgrader(2, 3, 4, typeof(AssetUpgrader2))]
        [AssetUpgrader(4, 5, typeof(AssetUpgrader3))]
        public class MyUpgradedAsset : Asset
        {
            public MyUpgradedAsset(int version)
            {
                SerializedVersion = version;
            }

            public MyUpgradedAsset()
            {
            }

            public Vector3 Vector { get; set; }
            public List<int> Test1 { get; set; }
            public List<int> Test2 { get; set; }
            public List<int> Test3 { get; set; }
            public List<int> Test4 { get; set; }
            public List<int> Test5 { get; set; }

            class AssetUpgrader1 : IAssetUpgrader
            {
                public void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode)
                {
                    dynamic asset = new DynamicYamlMapping(yamlAssetNode);

                    asset.SerializedVersion = AssetRegistry.GetCurrentFormatVersion(typeof(MyUpgradedAsset));

                    // Move Test1 to Test2
                    asset.Test2 = asset.Test1;
                    asset.Test1 = DynamicYamlEmpty.Default;
                }
            }

            class AssetUpgrader2 : IAssetUpgrader
            {
                public void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode)
                {
                    dynamic asset = new DynamicYamlMapping(yamlAssetNode);

                    asset.SerializedVersion = targetVersion;

                    // Move Test2 to Test4
                    if (currentVersion == 2)
                    {
                        asset.Test4 = asset.Test2;
                        asset.Test2 = DynamicYamlEmpty.Default;
                    }
                    // Move Test3 to Test4
                    else if (currentVersion == 3)
                    {
                        asset.Test4 = asset.Test3;
                        asset.Test3 = DynamicYamlEmpty.Default;
                    }
                }
            }

            class AssetUpgrader3 : IAssetUpgrader
            {
                public void Upgrade(int currentVersion, int targetVersion, ILogger log, YamlMappingNode yamlAssetNode)
                {
                    dynamic asset = new DynamicYamlMapping(yamlAssetNode);

                    asset.SerializedVersion = targetVersion;

                    // Move Test4 to Test5
                    asset.Test5 = asset.Test4;
                    asset.Test4 = DynamicYamlEmpty.Default;
                }
            }
        }

        [Test]
        public void Version1()
        {
            var asset = new MyUpgradedAsset(1) { Vector = new Vector3(12.0f, 15.0f, 17.0f), Test1 = new List<int> { 32, 64 } };
            TestUpgrade(asset, true);
        }

        [Test]
        public void Version2()
        {
            var asset = new MyUpgradedAsset(2) { Vector = new Vector3(12.0f, 15.0f, 17.0f), Test2 = new List<int> { 32, 64 } };
            TestUpgrade(asset, true);
        }

        [Test]
        public void Version3()
        {
            var asset = new MyUpgradedAsset(3) { Vector = new Vector3(12.0f, 15.0f, 17.0f), Test3 = new List<int> { 32, 64 } };
            TestUpgrade(asset, true);
        }

        [Test]
        public void Version4()
        {
            var asset = new MyUpgradedAsset(4) { Vector = new Vector3(12.0f, 15.0f, 17.0f), Test4 = new List<int> { 32, 64 } };
            TestUpgrade(asset, true);
        }

        [Test]
        public void Version5()
        {
            var asset = new MyUpgradedAsset(5) { Vector = new Vector3(12.0f, 15.0f, 17.0f), Test5 = new List<int> { 32, 64 } };
            TestUpgrade(asset, false);
        }

        public void TestUpgrade(MyUpgradedAsset asset, bool needMigration)
        {
            var outputFilePath = Path.Combine(DirectoryTestBase, @"TestUpgrade\Asset1.pdxobj");
            AssetSerializer.Save(outputFilePath, asset);

            var logger = new LoggerResult();
            Assert.AreEqual(AssetMigration.MigrateAssetIfNeeded(logger, outputFilePath), needMigration);

            Console.WriteLine(File.ReadAllText(outputFilePath).Trim());

            var upgradedAsset = AssetSerializer.Load<MyUpgradedAsset>(outputFilePath);
            AssertUpgrade(upgradedAsset);
        }

        private static void AssertUpgrade(MyUpgradedAsset asset)
        {
            Assert.That(asset.SerializedVersion, Is.EqualTo(5));
            Assert.That(asset.Test1, Is.Null);
            Assert.That(asset.Test2, Is.Null);
            Assert.That(asset.Test3, Is.Null);
            Assert.That(asset.Test4, Is.Null);
            Assert.That(asset.Test5, Is.Not.Null);
        }
    }
}