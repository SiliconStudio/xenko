// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using NUnit.Framework;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestPackageArchive
    {

        [Test, Ignore]
        public void TestBasicPackageCreateSaveLoad()
        {
            var defaultPackage = PackageStore.Instance.DefaultPackage;

            PackageArchive.Build(defaultPackage);
        }
    }
}