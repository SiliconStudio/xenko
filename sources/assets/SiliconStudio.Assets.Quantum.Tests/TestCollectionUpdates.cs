using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestCollectionUpdates
    {
        [Test]
        public void TestSimpleCollectionUpdate()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var asset = new Types.MyAsset2 { MyStrings = { "aaa", "bbb", "ccc" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem, null);
            var node = graph.RootNode[nameof(Types.MyAsset2.MyStrings)];
            //var ids = CollectionItemIdHelper.TryGetCollectionItemIds(asset.MyStrings, out itemIds);
        }
    }
}
