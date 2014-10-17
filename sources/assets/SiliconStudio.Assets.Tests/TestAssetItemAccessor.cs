// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Tests
{
    [TestFixture]
    public class TestAssetItemAccessor
    {
        [Test]
        public void TestCreateChildAsset()
        {
            // -------------------------
            // Init
            // -------------------------
            var localComponentDescriptor = TypeDescriptorFactory.Default.Find(typeof(LocalComponent));

            var baseAsset = new LocalAsset { Name = "base", Value = 1 };
            baseAsset.Parameters.Set(LocalAsset.StringKey, "string");

            var component = new LocalComponent() { Name = "comp1", Position = Vector4.UnitX };
            component.SetOverride(localComponentDescriptor["Name"], OverrideType.Sealed);
            baseAsset.Parameters.Set(LocalAsset.ComponentKey, component);

            var baseAssetItem = new AssetItem("base1", baseAsset);

            // Create a child asset
            var newAsset = (LocalAsset)baseAssetItem.CreateChildAsset();

            // -------------------------
            // Check base asset
            // -------------------------
            Assert.NotNull(newAsset.Base);
            Assert.IsFalse(newAsset.Base.IsRootImport);
            Assert.AreEqual(baseAsset.Id, newAsset.Base.Id);

            // Check override on member "Name"
            var newComponent = newAsset.Parameters[LocalAsset.ComponentKey];
            var overrideType = newComponent.GetOverride(localComponentDescriptor["Name"]);
            Assert.AreEqual(OverrideType.Sealed, overrideType);
        }

        [Test]
        public void TestAccessor()
        {
            // -------------------------
            // Init
            // -------------------------
            var localAssetDescriptor = TypeDescriptorFactory.Default.Find(typeof(LocalAsset));
            var localComponentDescriptor = TypeDescriptorFactory.Default.Find(typeof(LocalComponent));
            var collectionDescriptor = (DictionaryDescriptor)TypeDescriptorFactory.Default.Find(typeof(CustomParameterCollection));

            var baseAsset = new LocalAsset { Name = "base", Value = 1 };
            baseAsset.Parameters.Set(LocalAsset.StringKey, "string");

            var component = new LocalComponent() { Name = "comp1", Position = Vector4.UnitX };
            component.SetOverride(localComponentDescriptor["Name"], OverrideType.Sealed);
            baseAsset.Parameters.Set(LocalAsset.ComponentKey, component);

            var baseAssetItem = new AssetItem("base1", baseAsset);

            // Create a child asset
            var newAsset = (LocalAsset)baseAssetItem.CreateChildAsset();
            var newAssetItem = new AssetItem("new1", newAsset);

            // -------------------------
            // Setup project + assets
            // -------------------------
            var project = new Package();
            project.Assets.Add(baseAssetItem);
            project.Assets.Add(newAssetItem);
            var session = new PackageSession(project);

            // Create accessor on new item
            var accessor = new AssetItemAccessor(newAssetItem);

            var memberPath = new MemberPath();
            memberPath.Push(localAssetDescriptor["Parameters"]);
            memberPath.Push(collectionDescriptor, LocalAsset.ComponentKey);
            memberPath.Push(localComponentDescriptor["Name"]);

            // Get value for member path
            var memberValue = accessor.TryGetMemberValue(memberPath);

            Assert.IsTrue(memberValue.IsValid);
            Assert.AreEqual(OverrideType.Sealed, memberValue.Override);
            Assert.NotNull(memberValue.OverriderItem);
            Assert.AreEqual(baseAsset.Id, memberValue.OverriderItem.Id);

            memberPath.Pop();
            memberPath.Push(localComponentDescriptor["Position"]);

            // Get value for member path
            memberValue = accessor.TryGetMemberValue(memberPath);

            Assert.IsTrue(memberValue.IsValid);
            Assert.AreEqual(OverrideType.Base, memberValue.Override);
            Assert.NotNull(memberValue.OverriderItem);
            Assert.AreEqual(baseAsset.Id, memberValue.OverriderItem.Id);

            // Set Position as a new value 
            newAsset.Parameters[LocalAsset.ComponentKey].SetOverride(localComponentDescriptor["Position"], OverrideType.New);
            memberValue = accessor.TryGetMemberValue(memberPath);

            Assert.IsTrue(memberValue.IsValid);
            Assert.AreEqual(OverrideType.New, memberValue.Override);
            Assert.IsNull(memberValue.OverriderItem);
        }

        [DataContract("TestAssetItemAccessor-LocalAsset")]
        [AssetFileExtension(".pdxlocalasset")]
        public class LocalAsset : Asset
        {
            public static readonly PropertyKey<string> StringKey = new PropertyKey<string>("StringKey", typeof(LocalAsset));
            public static readonly PropertyKey<LocalComponent> ComponentKey = new PropertyKey<LocalComponent>("ComponentKey", typeof(LocalAsset));
            public static readonly PropertyKey<LocalComponent> ComponentKey1 = new PropertyKey<LocalComponent>("ComponentKey1", typeof(LocalAsset));

            public LocalAsset()
            {
                Parameters = new CustomParameterCollection();
                Components = new List<LocalComponent>();
            }

            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public int Value { get; set; }

            [DataMember(2)]
            public CustomParameterCollection Parameters { get; set; }

            public List<LocalComponent> Components { get; set; }

            public AssetReference<LocalAsset> AssetReference { get; set; }
        }

        [DataContract("TestAssetItemAccessor-LocalComponent")]
        public class LocalComponent : IEquatable<LocalComponent>
        {
            [DataMember(0)]
            public string Name { get; set; }

            [DataMember(1)]
            public Vector4 Position { get; set; }

            public bool Equals(LocalComponent other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name) && Position.Equals(other.Position);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LocalComponent)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ Position.GetHashCode();
                }
            }

            public static bool operator ==(LocalComponent left, LocalComponent right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LocalComponent left, LocalComponent right)
            {
                return !Equals(left, right);
            }
        }




    }
}