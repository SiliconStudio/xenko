// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

using YamlDotNet.Serialization;

namespace SiliconStudio.Xenko.Assets
{
    [DataContract("TestAsset")]
    [AssetAlias("AssetTest")]
    [AssetFileExtension(FileExtension)]
    [AssetFactory(typeof(TestAssetFactory))]
    [AssetDescription("Test asset", "A test assets, containing every supported types of property.")]
    public class TestAsset : Asset<TestAsset, AssetReference>
    {
        public enum TestEnum
        {
            DefaultValue,
            FirstValue,
            SecondValue, 
            ThirdValue
        }

        public const string FileExtension = ".xktest";

        public TestAsset()
        {
            StringList = new List<string>();
            IntList = new List<int>();
            UPathList = new List<UPath>();
            StringUPathDictionary = new Dictionary<string, UPath>();
            UPathIntDictionary = new Dictionary<UPath, int>();
            StringEnumDictionary = new Dictionary<string, TestEnum>();
        }

        [YamlMember(10)]
        public UPath UPathValue { get; set; }
        [YamlMember(20)]
        public float FloatValue { get; set; }
        [YamlMember(30)]
        public float? NullableFloatValue { get; set; }
        [YamlMember(40)]
        public bool BoolValue { get; set; }
        [YamlMember(50)]
        public bool? NullableBoolValue { get; set; }

        [YamlMember(60)]
        public int IntValue { get; set; }
        [YamlMember(70)]
        public UInt16 UShortValue { get; set; }

        [YamlMember(80)]
        public string StringValue { get; set; }

        [YamlMember(90)]
        public TestEnum EnumValue { get; set; }
        [YamlMember(100)]
        public TestEnum NullableEnumValue { get; set; }

        [YamlMember(110)]
        public List<string> StringList { get; set; }
        [YamlMember(120)]
        public List<int> IntList { get; set; }
        [YamlMember(130)]
        public List<UPath> UPathList { get; set; }
        [YamlMember(140)]
        public Dictionary<string, UPath> StringUPathDictionary { get; set; }
        [YamlMember(150)]
        public Dictionary<UPath, int> UPathIntDictionary { get; set; }
        [YamlMember(160)]
        public Dictionary<string, TestEnum> StringEnumDictionary { get; set; }

        public static TestAsset New()
        {
            var testAsset = new TestAsset
            {
                StringValue = Guid.NewGuid().ToString()
            };
            return testAsset;
        }

        private class TestAssetFactory : IAssetFactory
        {
            public IAsset New()
            {
                return TestAsset.New();
            }
        }

    }
}
