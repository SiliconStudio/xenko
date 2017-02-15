using System.IO;
using NUnit.Framework;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public class DeriveAssetTestBase
    {
        protected DeriveAssetTestBase(Asset baseAsset, Asset derivedAsset)
        {
            Container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            BaseAssetItem = new AssetItem("MyAsset", baseAsset);
            DerivedAssetItem = new AssetItem("MyDerivedAsset", derivedAsset);
        }

        protected void BuildGraph()
        {
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, BaseAssetItem, null);
            Assert.IsInstanceOf<MyAssetBasePropertyGraph>(baseGraph);
            BaseGraph = (MyAssetBasePropertyGraph)baseGraph;
            var derivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, DerivedAssetItem, null);
            Assert.IsInstanceOf<MyAssetBasePropertyGraph>(baseGraph);
            DerivedGraph = (MyAssetBasePropertyGraph)derivedGraph;
            DerivedGraph.RefreshBase(BaseGraph);
        }

        public static Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public AssetPropertyGraphContainer Container { get; }
        public AssetItem BaseAssetItem { get; }
        public AssetItem DerivedAssetItem { get; }
        public MyAssetBasePropertyGraph BaseGraph { get; private set; }
        public MyAssetBasePropertyGraph DerivedGraph { get; private set; }
    }
    public class DeriveAssetTest<T> : DeriveAssetTestBase where T : Asset
    {
        private DeriveAssetTest(T baseAsset, T derivedAsset)
            : base(baseAsset, derivedAsset)
        {
        }

        public static DeriveAssetTest<T> DeriveAsset(T baseAsset)
        {
            var derivedAsset = (T)baseAsset.CreateDerivedAsset("MyAsset");
            var result = new DeriveAssetTest<T>(baseAsset, derivedAsset);
            result.BuildGraph();
            return result;
        }

        public static DeriveAssetTest<T> LoadFromYaml(string baseYaml, string derivedYaml)
        {
            var baseAsset = AssetFileSerializer.Load<T>(ToStream(baseYaml), $"MyAsset{Types.FileExtension}");
            var derivedAsset = AssetFileSerializer.Load<T>(ToStream(derivedYaml), $"MyDerivedAsset{Types.FileExtension}");
            var result = new DeriveAssetTest<T>(baseAsset.Asset, derivedAsset.Asset)
            {
                BaseAssetItem = { Overrides = baseAsset.Overrides },
                DerivedAssetItem = { Overrides = derivedAsset.Overrides }
            };
            result.BuildGraph();
            return result;
        }

        public T BaseAsset => (T)BaseAssetItem.Asset;
        public T DerivedAsset => (T)DerivedAssetItem.Asset;
    }
}
