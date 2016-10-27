using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var propertyNode = (AssetNode)graph.RootNode.GetChild(nameof(MyAsset1.MyString));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(1)));
            propertyNode.Content.Update("MyDerivedString", new Index(1));
            Assert.AreEqual(OverrideType.New, propertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, propertyNode.GetOverride(new Index(1)));
        }
    }

    [TestFixture]
    public class TestAssetPropertyNodeGraph
    {
        [DataContract]
        public class MyAsset1 : Asset
        {
            public string MyString { get; set; }
        }

        [Test]
        public void TestSimpleConstruction()
        {
            var nodeContainer = new AssetNodeContainer();
            var asset = new MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(nodeContainer, assetItem);
            Assert.IsAssignableFrom<AssetNode>(graph.RootNode);
        }
    }
}
