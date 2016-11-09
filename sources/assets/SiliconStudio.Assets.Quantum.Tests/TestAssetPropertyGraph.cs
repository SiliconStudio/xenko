using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetPropertyGraph
    {
        [Test]
        public void TestSimpleConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new Types.MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            Assert.IsAssignableFrom<AssetNode>(graph.RootNode);
        }

        [Test]
        public void TestCollectionConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            Assert.IsAssignableFrom<AssetNode>(graph.RootNode);
            CollectionItemIdentifiers ids;
            Assert.True(CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyStrings, out ids));
            Assert.AreEqual(3, ids.Count);
            Assert.True(ids.ContainsKey(0));
            Assert.True(ids.ContainsKey(1));
            Assert.True(ids.ContainsKey(2));
        }

        [Test]
        public void TestNestedCollectionConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new Types.MyAsset7 { MyAsset2 = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } } };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            Assert.IsAssignableFrom<AssetNode>(graph.RootNode);
            CollectionItemIdentifiers ids;
            Assert.True(CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyAsset2.MyStrings, out ids));
            Assert.AreEqual(3, ids.Count);
            Assert.True(ids.ContainsKey(0));
            Assert.True(ids.ContainsKey(1));
            Assert.True(ids.ContainsKey(2));
        }
    }
}
