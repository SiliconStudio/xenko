using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;

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
    }
}
