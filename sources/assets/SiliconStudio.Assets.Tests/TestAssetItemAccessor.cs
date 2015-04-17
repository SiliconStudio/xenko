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
        
        [DataContract("TestAssetItemAccessor-LocalAsset")]
        [AssetDescription(".pdxlocalasset")]
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