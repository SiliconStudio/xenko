// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetInheritance
    {
        [Test]
        public void TestWithOverrides()
        {
            // Create a derivative asset and check overrides
            var assets = new List<TestAssetWithParts>();
            var assetItems = new List<AssetItem>();

            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestAssetWithParts));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");

            assets.Add(new TestAssetWithParts()
            {
                Parts =
                {
                        new AssetPartTestItem(Guid.NewGuid()),
                        new AssetPartTestItem(Guid.NewGuid())
                }
            });
            assetItems.Add(new AssetItem("asset-0", assets[0]));

            // Set a sealed on Name property
            assets[0].SetOverride(memberDesc, OverrideType.Sealed);

            var childAsset = (TestAssetWithParts)assetItems[0].CreateChildAsset();

            // Check that child asset has a base
            Assert.NotNull(childAsset.Base);

            // Check that derived asset is using base
            Assert.AreEqual(OverrideType.Base, childAsset.GetOverride(memberDesc));

            // Check that property Name on base asset is sealed
            Assert.AreEqual(OverrideType.Sealed, childAsset.Base.Asset.GetOverride(memberDesc));
        }

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