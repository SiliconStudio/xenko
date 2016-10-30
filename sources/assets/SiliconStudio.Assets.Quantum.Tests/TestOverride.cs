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
        public void TestSimplePropertyInArchetype()
        {
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            var derivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
            var basePropertyNode = (AssetNode)baseGraph.RootNode.GetChild(nameof(MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)derivedGraph.RootNode.GetChild(nameof(MyAsset1.MyString));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            basePropertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve());
        }

        [Test]
        public void TestSimplePropertyInDerived()
        {
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            var derivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
            var basePropertyNode = (AssetNode)baseGraph.RootNode.GetChild(nameof(MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)derivedGraph.RootNode.GetChild(nameof(MyAsset1.MyString));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            derivedPropertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(Index.Empty));
        }

        [Test]
        public void TestSimpleCollection()
        {
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
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
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset3 { MyDictionary = { { "String1", new SomeObject { Value = "aaa" } }, { "String2", new SomeObject { Value = "bbb" } } } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
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
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset2();
            asset.Struct.MyStrings.Add("String1");
            asset.Struct.MyStrings.Add("String2");        
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
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

        [Test]
        public void TestSimpleCollectionAddInArchetype()
        {
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = (MyAsset2)asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            var derivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
            var basePropertyNode = (AssetNode)baseGraph.RootNode.GetChild(nameof(MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)derivedGraph.RootNode.GetChild(nameof(MyAsset2.MyStrings));
            basePropertyNode.Content.Add("String3");
            Assert.AreEqual(3, derivedAsset.MyStrings.Count);
            Assert.AreEqual("String3", derivedAsset.MyStrings[2]);
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(2)));
        }
    }
}
