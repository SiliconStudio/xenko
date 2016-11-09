using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public static class Types
    {
        public const string FileExtension = ".xktest";

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset1 : Asset
        {
            public string MyString { get; set; }
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset2 : Asset
        {
            public List<string> MyStrings { get; set; } = new List<string>();
            public StructWithList Struct = new StructWithList { MyStrings = new List<string>() };
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset3 : Asset
        {
            public Dictionary<string, string> MyDictionary { get; set; } = new Dictionary<string, string>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset4 : Asset
        {
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset5 : Asset
        {
            public List<IMyInterface> MyInterfaces { get; set; } = new List<IMyInterface>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset6 : Asset
        {
            public Dictionary<string, IMyInterface> MyDictionary { get; set; } = new Dictionary<string, IMyInterface>();
        }

        [DataContract]
        [AssetDescription(FileExtension)]
        public class MyAsset7 : Asset
        {
            public MyAsset2 MyAsset2 { get; set; }
            public MyAsset3 MyAsset3 { get; set; }
            public MyAsset4 MyAsset4 { get; set; }
        }

        [DataContract]
        public struct StructWithList
        {
            public List<string> MyStrings { get; set; }
        }

        public interface IMyInterface
        {
            string Value { get; set; }
        }

        [DataContract]
        public class SomeObject : IMyInterface
        {
            public string Value { get; set; }
        }

        [DataContract]
        public class SomeObject2 : IMyInterface
        {
            public string Value { get; set; }
            public int Number { get; set; }
        }
    }
}
