// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Tests
{
    [DataContract("!AssetObjectTest")]
    [AssetDescription(FileExtension)]
    public class AssetObjectTest : Asset, IEquatable<AssetObjectTest>, IAssetComposer
    {
        public const string FileExtension = ".pdxtest";

        public string Name { get; set; }

        [DefaultValue(null)]
        public AssetReference<AssetObjectTest> Reference { get; set; }

        [DefaultValue(null)]
        public List<AssetReference<AssetObjectTest>> CompositionBases = new List<AssetReference<AssetObjectTest>>();

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

        public IEnumerable<IContentReference> GetCompositionBases()
        {
            return CompositionBases;
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

    [DataContract("!AssetImportObjectTest")]
    [AssetDescription(".pdximptest")]
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

    [DataContract("!AssetObjectTestRaw")]
    [AssetDescription(".pdxraw")]
    public class AssetObjectTestRaw : SourceCodeAsset
    {
    }

    [DataContract("!AssetObjectTestSub")]
    [AssetDescription(".pdxtestsub")]
    public class AssetObjectTestSub : Asset
    {
        public int Value { get; set; }
    }
}