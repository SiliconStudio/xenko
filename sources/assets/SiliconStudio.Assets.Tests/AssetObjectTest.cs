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
    public class TestAssetWithParts : AssetComposite
    {
        public const string FileExtension = ".xkpart";

        public TestAssetWithParts()
        {
            Parts = new List<AssetPartTestItem>();
        }

        public string Name { get; set; }

        public List<AssetPartTestItem> Parts { get; set; }

        public override IEnumerable<AssetPart> CollectParts()
        {
            return Parts.Select(it => new AssetPart(it.Id, it.BaseId, it.BasePartInstanceId));
        }

        public override void SetPart(Guid id, Guid baseId, Guid basePartInstanceId)
        {
            throw new NotImplementedException();
        }

        public override bool ContainsPart(Guid id)
        {
            return Parts.Any(t => t.Id == id);
        }

        public override void FixupPartReferences()
        {
            throw new NotImplementedException();
        }

        public override Asset CreateChildAsset(string location)
        {
            var asset = (TestAssetWithParts)base.CreateChildAsset(location);

            // Create asset with new base
            for (int i = 0; i < asset.Parts.Count; i++)
            {
                var part = asset.Parts[i];
                asset.Parts[i] = new AssetPartTestItem(Guid.NewGuid(), part.Id);
            }

            return asset;
        }

        public void AddPart(TestAssetWithParts assetBaseWithParts)
        {
            if (assetBaseWithParts == null) throw new ArgumentNullException(nameof(assetBaseWithParts));

            // The assetPartBase must be a plain child asset
            if (assetBaseWithParts.Base == null) throw new InvalidOperationException($"Expecting a Base for {nameof(assetBaseWithParts)}");
            if (assetBaseWithParts.BaseParts != null) throw new InvalidOperationException($"Expecting a null BaseParts for {nameof(assetBaseWithParts)}");

            // Check that the assetPartBase contains only entities from its base (no new entity, must be a plain ChildAsset)
            if (assetBaseWithParts.CollectParts().Any(it => !it.BaseId.HasValue))
            {
                throw new InvalidOperationException("An asset part base must contain only base assets");
            }

            AddBasePart(assetBaseWithParts.Base);

            for (int i = 0; i < assetBaseWithParts.Parts.Count; i++)
            {
                var part = assetBaseWithParts.Parts[i];
                Parts.Add(new AssetPartTestItem(part.Id, part.BaseId, assetBaseWithParts.Id));
            }
        }
    }

    [DataContract("AssetPartTestItem")]
    public class AssetPartTestItem
    {
        public AssetPartTestItem()
        {
        }

        public AssetPartTestItem(Guid id, Guid? baseId = null, Guid? basePartInstanceId = null)
        {
            Id = id;
            BaseId = baseId;
            BasePartInstanceId = basePartInstanceId;
        }

        public Guid Id;

        public Guid? BaseId;

        public Guid? BasePartInstanceId;
    }

    [DataContract("!AssetImportObjectTest")]
    [AssetDescription(".xkimptest")]
    public class AssetImportObjectTest : AssetWithSource
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
