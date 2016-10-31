using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public static class Types
    {
        [DataContract]
        public class MyAsset1 : Asset
        {
            public string MyString { get; set; }
        }

        [DataContract]
        public class MyAsset2 : Asset
        {
            public List<string> MyStrings { get; set; } = new List<string>();
            public StructWithList Struct = new StructWithList { MyStrings = new List<string>() };
        }

        [DataContract]
        public class MyAsset3 : Asset
        {
            public Dictionary<string, string> MyDictionary { get; set; } = new Dictionary<string, string>();
        }

        [DataContract]
        public class MyAsset4 : Asset
        {
            public List<string> MyStrings { get; set; } = new List<string>();
        }

        [DataContract]
        public struct StructWithList
        {
            public List<string> MyStrings { get; set; }
        }

        [DataContract]
        public class SomeObject
        {
            public string Value { get; set; }
        }
    }
}
