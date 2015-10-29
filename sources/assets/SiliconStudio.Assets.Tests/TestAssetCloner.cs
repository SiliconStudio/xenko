// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests
{
    [DataContract("TestAssetClonerObject")]
    public class TestAssetClonerObject
    {
        public string Name { get; set; }

        public TestAssetClonerObject SubObject { get; set; }
    }

    [TestFixture]
    public class TestAssetCloner
    {
        /// <summary>
        /// Test basic clone and shadow objects copy
        /// </summary>
        [Test]
        public void TestCloneAndShadowObject()
        {
            var obj = new TestAssetClonerObject
            {
                Name = "Test1",
                SubObject = new TestAssetClonerObject() { Name = "Test2" }
            };

            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestAssetClonerObject));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");
            obj.SetOverride(memberDesc, OverrideType.New);
            obj.SubObject.SetOverride(memberDesc, OverrideType.Sealed);

            var newInstance = (TestAssetClonerObject)AssetCloner.Clone(obj);

            // Check that we are getting shadow objects
            Assert.AreEqual(OverrideType.New, newInstance.GetOverride(memberDesc));
            Assert.AreEqual(OverrideType.Sealed, newInstance.SubObject.GetOverride(memberDesc));

            // Change original object to default, but check that we are working on a shadow object copy on the cloned object
            obj.SetOverride(memberDesc, OverrideType.Base);
            Assert.AreEqual(OverrideType.New, newInstance.GetOverride(memberDesc));
        }


        /// <summary>
        /// Test basic clone with remove overrides option
        /// </summary>
        [Test]
        public void TestCloneAssetWithRemoveOverrides()
        {
            var obj = new TestAssetClonerObject
            {
                Name = "Test1",
                SubObject = new TestAssetClonerObject() { Name = "Test2" }
            };

            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestAssetClonerObject));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");
            obj.SetOverride(memberDesc, OverrideType.New);
            obj.SubObject.SetOverride(memberDesc, OverrideType.Sealed);

            var newInstance = (TestAssetClonerObject)AssetCloner.Clone(obj, AssetClonerFlags.RemoveOverrides);

            // Check that we are not overriding anything
            Assert.AreEqual(OverrideType.Base, newInstance.GetOverride(memberDesc));
            Assert.AreEqual(OverrideType.Base, newInstance.SubObject.GetOverride(memberDesc));
        }
    }
}