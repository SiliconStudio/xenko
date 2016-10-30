using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum.Tests
{
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
            var container = new AssetPropertyNodeGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var graph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            Assert.IsAssignableFrom<AssetNode>(graph.RootNode);
        }
    }
}
