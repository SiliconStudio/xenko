// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpYaml;
using SharpYaml.Events;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetUpgrade : TestBase
    {
        [DataContract("MyUpgradedAsset")]
        [AssetFileExtension(".pdxobj")]
        [AssetFormatVersion(AssetFormatVersion, typeof(AssetUpgrader))]
        public class MyUpgradedAsset : Asset
        {
            public const int AssetFormatVersion = 2;

            public MyUpgradedAsset()
            {
                SerializedVersion = 1;
            }

            public int Test { get; set; }

            public List<int> Test2 { get; set; }
            public List<int> Test3 { get; set; }

            class AssetUpgrader : IAssetUpgrader
            {
                public void Upgrade(ILogger log, YamlMappingNode yamlAssetNode)
                {
                    dynamic asset = new DynamicYamlMapping(yamlAssetNode);

                    asset.SerializedVersion = AssetFormatVersion;
                    
                    // Move Test2 to Test3
                    asset.Test3 = asset.Test2;
                    asset.Test2 = DynamicYamlEmpty.Default;
                }
            }
        }

        [Test]
        public void Simple()
        {
            var asset = new MyUpgradedAsset { Test = 32, Test2 = new List<int> { 32, 64 } };
            var outputFilePath = Path.Combine(DirectoryTestBase, @"TestUpgrade\Asset1.pdxobj");
            AssetSerializer.Save(outputFilePath, asset);

            var logger = new LoggerResult();
            Assert.That(AssetMigration.MigrateAssetIfNeeded(logger, outputFilePath));

            Console.WriteLine(File.ReadAllText(outputFilePath).Trim());

            var upgradedAsset = AssetSerializer.Load<MyUpgradedAsset>(outputFilePath);
            Assert.That(upgradedAsset.SerializedVersion, Is.EqualTo(2));
            Assert.That(upgradedAsset.Test2, Is.Null);
            Assert.That(upgradedAsset.Test3, Is.Not.Null);
        }
    }
}