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
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(Index.Empty));
            Assert.True(propertyNode.IsInherited(Index.Empty));
            Assert.False(propertyNode.IsOverridden(Index.Empty));
            propertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual(MemberFlags.Default, propertyNode.GetMemberFlags(Index.Empty));
            Assert.False(propertyNode.IsInherited(Index.Empty));
            Assert.True(propertyNode.IsOverridden(Index.Empty));
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
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(Index.Empty));
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(new Index(0)));
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(new Index(1)));
            Assert.True(propertyNode.IsInherited(Index.Empty));
            Assert.False(propertyNode.IsOverridden(Index.Empty));
            Assert.True(propertyNode.IsInherited(new Index(0)));
            Assert.False(propertyNode.IsOverridden(new Index(0)));
            Assert.True(propertyNode.IsInherited(new Index(1)));
            Assert.False(propertyNode.IsOverridden(new Index(1)));
            propertyNode.Content.Update("MyDerivedString", new Index(1));
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(Index.Empty));
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(new Index(0)));
            Assert.AreEqual(MemberFlags.Inherited, propertyNode.GetMemberFlags(new Index(1)));
            Assert.True(propertyNode.IsInherited(Index.Empty));
            Assert.False(propertyNode.IsOverridden(Index.Empty));
            Assert.True(propertyNode.IsInherited(new Index(0)));
            Assert.False(propertyNode.IsOverridden(new Index(0)));
            Assert.False(propertyNode.IsInherited(new Index(1)));
            Assert.True(propertyNode.IsOverridden(new Index(1)));
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
