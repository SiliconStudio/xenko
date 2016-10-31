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
            public List<SomeObject> MyObjects { get; set; } = new List<SomeObject>();
        }

        [DataContract]
        public class MyAsset5 : Asset
        {
            public List<IMyInterface> MyInterfaces { get; set; } = new List<IMyInterface>();
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
