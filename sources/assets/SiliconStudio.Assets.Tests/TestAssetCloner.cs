// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets.Tests
{
    [DataContract("TestAssetClonerObject")]
    public class TestAssetClonerObject
    {
        public string Name { get; set; }

        public TestAssetClonerObject SubObject { get; set; }

        public TestObjectReference ObjectWithAttachedReference { get; set; }
    }

    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<TestObjectReference>))]
    [DataSerializerGlobal(typeof(ReferenceSerializer<TestObjectReference>), Profile = "Asset")]
    public class TestObjectReference
    {        
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

        [Test]
        public void TestHash()
        {
            var obj1 = new TestAssetClonerObject
            {
                Name = "Test1",
                SubObject = new TestAssetClonerObject() { Name = "Test2" },
                ObjectWithAttachedReference = new TestObjectReference()
            };

            // Create a fake reference to make sure that the attached reference will not be serialized
            var attachedReference = AttachedReferenceManager.GetOrCreateAttachedReference(obj1.ObjectWithAttachedReference);
            attachedReference.Url = "just_for_test";
            attachedReference.Id = Guid.NewGuid();

            // Setup some proper id on objects so serialization is stable
            IdentifiableHelper.SetId(obj1, new Guid("EC86143E-896F-45C5-9A4D-627317D22955"));
            IdentifiableHelper.SetId(obj1.SubObject, new Guid("34E160CD-1D94-468E-8BFD-F82FF96013FC"));

            var obj2 = (TestAssetClonerObject)AssetCloner.Clone(obj1);

            var hash1 = AssetHash.Compute(obj1);
            var hash2 = AssetHash.Compute(obj2);
            Assert.AreEqual(hash1, hash2);

            obj1.Name = "Yes";
            var hash11 = AssetHash.Compute(obj1);
            Assert.AreNotEqual(hash11, hash2);
            obj1.Name = "Test1";

            var hash12 = AssetHash.Compute(obj1);
            Assert.AreEqual(hash12, hash2);

            // Test the same with overrides
            var objDesc = TypeDescriptorFactory.Default.Find(typeof(TestAssetClonerObject));
            var memberDesc = objDesc.Members.First(t => t.Name == "Name");
            obj1.SetOverride(memberDesc, OverrideType.New);
            obj1.SubObject.SetOverride(memberDesc, OverrideType.Sealed);

            obj2 = (TestAssetClonerObject)AssetCloner.Clone(obj1);

            var hash1WithOverrides = AssetHash.Compute(obj1);
            var hash2WithOverrides = AssetHash.Compute(obj2);
            Assert.AreNotEqual(hash1, hash1WithOverrides);
            Assert.AreNotEqual(hash2, hash2WithOverrides);
            Assert.AreEqual(hash1WithOverrides, hash2WithOverrides);
        }
    }
}