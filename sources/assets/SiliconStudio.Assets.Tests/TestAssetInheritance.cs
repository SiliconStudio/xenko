// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetInheritance
    {
        [Test]
        public void TestWithParts()
        {
            // Create a derivative asset with asset parts
            var project = new Package();
            var assets = new List<TestAssetWithParts>();
            var assetItems = new List<AssetItem>();

            assets.Add(new TestAssetWithParts()
            {
                Parts =
                {
                        new AssetPartTestItem(Guid.NewGuid()),
                        new AssetPartTestItem(Guid.NewGuid())
                }
            });
            assetItems.Add(new AssetItem("asset-0", assets[0]));
            project.Assets.Add(assetItems[0]);

            var childAsset = (TestAssetWithParts)assetItems[0].CreateDerivedAsset();

            // Check that child asset has a base
            Assert.NotNull(childAsset.Archetype);

            // Check base asset
            Assert.AreEqual(assets[0].Id, childAsset.Archetype.Id);

            // Check that base is correctly setup for the part
            var i = 0;
            var instanceId = Guid.Empty;
            foreach (var part in childAsset.Parts)
            {
                Assert.AreEqual(assets[0].Id, part.Base.BasePartAsset.Id);
                Assert.AreEqual(assets[0].Parts[i].Id, part.Base.BasePartId);
                if (instanceId == Guid.Empty)
                    instanceId = part.Base.InstanceId;
                Assert.AreNotEqual(Guid.Empty, instanceId);
                Assert.AreEqual(instanceId, part.Base.InstanceId);
                ++i;
            }
        }
    }
}
