using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestOverride
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
            public Dictionary<string, SomeObject> MyDictionary { get; set; } = new Dictionary<string, SomeObject>();
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

        [Test]
        public void TestSimpleProperty()
        {
            var nodeContainer = new AssetNodeContainer();
            var asset = new MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateChildAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, derivedItem);
            var propertyNode = (AssetNode)graph.RootNode.GetChild(nameof(MyAsset1.MyString));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            propertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual(OverrideType.New, propertyNode.GetOverride(Index.Empty));
        }

        [Test]
        public void TestSimpleCollection()
        {
            var nodeContainer = new AssetNodeContainer();
            var asset = new MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateChildAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, derivedItem);
            var propertyNode = (AssetNode)graph.RootNode.GetChild(nameof(MyAsset2.MyStrings));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(1)));
            propertyNode.Content.Update("MyDerivedString", new Index(1));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.New, propertyNode.GetOverride(new Index(1)));
        }

        [Test]
        public void TestSimpleDictionary()
        {
            var nodeContainer = new AssetNodeContainer();
            var asset = new MyAsset3 { MyDictionary = { { "String1", new SomeObject { Value = "aaa" } }, { "String2", new SomeObject { Value = "bbb" } } } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateChildAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, derivedItem);
            var propertyNode = (AssetNode)graph.RootNode.GetChild(nameof(MyAsset3.MyDictionary));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index("String1")));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index("String2")));
            propertyNode.Content.Update(new SomeObject { Value = "bbb" }, new Index("String2"));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index("String1")));
            Assert.AreEqual(OverrideType.New, propertyNode.GetOverride(new Index("String2")));
        }

        [Test]
        public void TestCollectionInStruct()
        {
            var nodeContainer = new AssetNodeContainer();
            var asset = new MyAsset2();
            asset.Struct.MyStrings.Add("String1");
            asset.Struct.MyStrings.Add("String2");        
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateChildAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, derivedItem);
            var structNode = (AssetNode)graph.RootNode.GetChild(nameof(MyAsset2.Struct));
            var propertyNode = (AssetNode)structNode.GetChild(nameof(StructWithList.MyStrings));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(1)));
            propertyNode.Content.Update("MyDerivedString", new Index(1));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.New, propertyNode.GetOverride(new Index(1)));
        }
    }
}
