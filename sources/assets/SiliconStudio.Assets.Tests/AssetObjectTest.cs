// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Tests
{
    [DataContract("!AssetObjectTest")]
    [AssetDescription(FileExtension)]
    public class AssetObjectTest : Asset, IEquatable<AssetObjectTest>
    {
        public const string FileExtension = ".xktest";

        public string Name { get; set; }

        [DefaultValue(null)]
        public AssetReference<AssetObjectTest> Reference { get; set; }

        [DefaultValue(null)]
        public UFile RawAsset { get; set; }

        public bool Equals(AssetObjectTest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(Reference, other.Reference) && Equals(RawAsset, other.RawAsset);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssetObjectTest)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Reference != null ? Reference.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RawAsset != null ? RawAsset.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(AssetObjectTest left, AssetObjectTest right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AssetObjectTest left, AssetObjectTest right)
        {
            return !Equals(left, right);
        }
    }

    [DataContract("!TestAssetWithParts")]
    [AssetDescription(FileExtension)]
    public class TestAssetWithParts : Asset, IAssetPartContainer
    {
        public const string FileExtension = ".xkpart";

        public TestAssetWithParts()
        {
            Parts = new List<AssetPart>();
        }

        public string Name { get; set; }

        public List<AssetPart> Parts { get; set; }

        public IEnumerable<AssetPart> CollectParts()
        {
            return Parts;
        }

        public bool ContainsPart(Guid id)
        {
            return Parts.Any(t => t.Id == id);
        }

        public override Asset CreateChildAsset(string location)
        {
            var asset = (TestAssetWithParts)base.CreateChildAsset(location);

            // Create asset with new base
            for (int i = 0; i < asset.Parts.Count; i++)
            {
                var part = asset.Parts[i];
                part.BaseId = part.Id;
                part.Id = Guid.NewGuid();
                asset.Parts[i] = part;
            }

            return asset;
        }
    }
    
    [DataContract("!AssetImportObjectTest")]
    [AssetDescription(".xkimptest")]
    public class AssetImportObjectTest : AssetImport
    {
        public AssetImportObjectTest()
        {
            References = new Dictionary<string, AssetReference<AssetObjectTestSub>>();
        }

        public string Name { get; set; }

        [DefaultValue(null)]
        public Dictionary<string, AssetReference<AssetObjectTestSub>> References { get; set; }
    }

    [DataContract("!AssetObjectTestSub")]
    [AssetDescription(".xktestsub")]
    public class AssetObjectTestSub : Asset
    {
        public int Value { get; set; }
    }
}