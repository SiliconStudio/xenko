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
        public void TestCreateChildAssetWithParts()
        {
            var project = new Package();
            var assets = new List<TestAssetWithParts>();
            var assetItems = new List<AssetItem>();

            assets.Add(new TestAssetWithParts()
            {
                Parts =
                {
                        new AssetPart(Guid.NewGuid()),
                        new AssetPart(Guid.NewGuid())
                }
            });
            assetItems.Add(new AssetItem("asset-0", assets[0]));
            project.Assets.Add(assetItems[0]);

            var childAsset = (TestAssetWithParts)assetItems[0].CreateChildAsset();

            // Check that child asset has a base
            Assert.NotNull(childAsset.Base);

            // Check base asset
            Assert.AreEqual(assets[0].Id, childAsset.Base.Id);

            // Check that base is correctly setup for the part
            Assert.AreEqual(assets[0].Parts[0].Id, childAsset.Parts[0].BaseId);
        }
    }
}