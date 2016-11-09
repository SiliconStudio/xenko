using System.IO;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public class DeriveAssetTest<T> where T : Asset
    {
        private DeriveAssetTest(T baseAsset, T derivedAsset)
        {
            Container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            BaseAssetItem = new AssetItem("MyAsset", baseAsset);
            DerivedAssetItem = new AssetItem("MyDerivedAsset", derivedAsset);
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
            var baseAsset = AssetSerializer.Load<T>(ToStream(baseYaml), $"MyAsset{Types.FileExtension}", null);
            var derivedAsset = AssetSerializer.Load<T>(ToStream(derivedYaml), $"MyDerivedAsset{Types.FileExtension}", null);
            var result = new DeriveAssetTest<T>(baseAsset.Asset, derivedAsset.Asset)
            {
                BaseAssetItem = { Overrides = baseAsset.Overrides },
                DerivedAssetItem = { Overrides = derivedAsset.Overrides }
            };
            result.BuildGraph();
            return result;
        }

        private void BuildGraph()
        {
            BaseGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, BaseAssetItem, null);
            DerivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, DerivedAssetItem, null);
            DerivedGraph.RefreshBase(BaseGraph);
        }

        private static Stream ToStream(string str)
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
        public T BaseAsset => (T)BaseAssetItem.Asset;
        public T DerivedAsset => (T)DerivedAssetItem.Asset;
        public AssetPropertyGraph BaseGraph { get; private set; }
        public AssetPropertyGraph DerivedGraph { get; private set; }
    }
}
