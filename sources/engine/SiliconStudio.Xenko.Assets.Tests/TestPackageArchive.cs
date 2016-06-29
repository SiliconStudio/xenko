// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Tasks;

namespace SiliconStudio.Xenko.Assets.Tests
{
    [TestFixture]
    public class TestPackageArchive
    {

        [Test, Ignore("Need to check why it was disabled")]
        public void TestBasicPackageCreateSaveLoad()
        {
            var defaultPackage = PackageStore.Instance.DefaultPackage;

            PackageArchive.Build(defaultPackage);
        }
    }
}