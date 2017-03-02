using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetPropertyGraph
    {
        [Test]
        public void TestSimpleConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = new Types.MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            Assert.IsAssignableFrom<AssetObjectNode>(graph.RootNode);
        }

        [Test]
        public void TestCollectionConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            Assert.IsAssignableFrom<AssetObjectNode>(graph.RootNode);
            CollectionItemIdentifiers ids;
            Assert.True(CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyStrings, out ids));
            Assert.AreEqual(3, ids.KeyCount);
            Assert.AreEqual(0, ids.DeletedCount);
            Assert.True(ids.ContainsKey(0));
            Assert.True(ids.ContainsKey(1));
            Assert.True(ids.ContainsKey(2));
        }

        [Test]
        public void TestNestedCollectionConstruction()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = new Types.MyAsset7 { MyAsset2 = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } } };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            Assert.IsAssignableFrom<AssetObjectNode>(graph.RootNode);
            CollectionItemIdentifiers ids;
            Assert.True(CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyAsset2.MyStrings, out ids));
            Assert.AreEqual(3, ids.KeyCount);
            Assert.AreEqual(0, ids.DeletedCount);
            Assert.True(ids.ContainsKey(0));
            Assert.True(ids.ContainsKey(1));
            Assert.True(ids.ContainsKey(2));
        }

        [Test]
        public void TestCollectionItemIdentifierWithDuplicates()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } };
            var ids = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            ids.Add(0, IdentifierGenerator.Get(100));
            ids.Add(1, IdentifierGenerator.Get(200));
            ids.Add(2, IdentifierGenerator.Get(100));
            var assetItem = new AssetItem("MyAsset", asset);
            Assert.AreEqual(IdentifierGenerator.Get(100), ids[0]);
            Assert.AreEqual(IdentifierGenerator.Get(200), ids[1]);
            Assert.AreEqual(IdentifierGenerator.Get(100), ids[2]);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            Assert.IsAssignableFrom<AssetObjectNode>(graph.RootNode);
            Assert.True(CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyStrings, out ids));
            Assert.AreEqual(3, ids.KeyCount);
            Assert.AreEqual(0, ids.DeletedCount);
            Assert.AreEqual(IdentifierGenerator.Get(100), ids[0]);
            Assert.AreEqual(IdentifierGenerator.Get(200), ids[1]);
            Assert.AreNotEqual(IdentifierGenerator.Get(100), ids[2]);
            Assert.AreNotEqual(IdentifierGenerator.Get(200), ids[2]);
        }
    }
}
