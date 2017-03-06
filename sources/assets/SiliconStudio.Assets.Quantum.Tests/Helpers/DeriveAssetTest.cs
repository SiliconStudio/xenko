namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public class DeriveAssetTest<TAsset, TAssetPropertyGraph> where TAsset : Asset where TAssetPropertyGraph : AssetPropertyGraph
    {
        private DeriveAssetTest(AssetTestContainer<TAsset, TAssetPropertyGraph> baseAsset, AssetTestContainer<TAsset, TAssetPropertyGraph> derivedAsset)
        {
            Base = baseAsset;
            Derived = derivedAsset;
        }

        public TAsset BaseAsset => (TAsset)BaseAssetItem.Asset;
        public TAsset DerivedAsset => (TAsset)DerivedAssetItem.Asset;
        public AssetItem BaseAssetItem => Base.AssetItem;
        public AssetItem DerivedAssetItem => Derived.AssetItem;
        public TAssetPropertyGraph BaseGraph => Base.Graph;
        public TAssetPropertyGraph DerivedGraph => Derived.Graph;

        public AssetTestContainer<TAsset, TAssetPropertyGraph> Base { get; }
        public AssetTestContainer<TAsset, TAssetPropertyGraph> Derived { get; }

        public static DeriveAssetTest<TAsset, TAssetPropertyGraph> DeriveAsset(TAsset baseAsset)
        {
            var container = new AssetPropertyGraphContainer(new AssetNodeContainer { NodeBuilder = { NodeFactory = new AssetNodeFactory() } });
            var baseContainer = new AssetTestContainer<TAsset, TAssetPropertyGraph>(container, baseAsset);
            baseContainer.BuildGraph();
            var derivedAsset = (TAsset)baseContainer.Asset.CreateDerivedAsset("MyAsset");
            var derivedContainer = new AssetTestContainer<TAsset, TAssetPropertyGraph>(baseContainer.Container, derivedAsset);
            var result = new DeriveAssetTest<TAsset, TAssetPropertyGraph>(baseContainer, derivedContainer);
            derivedContainer.BuildGraph();
            derivedContainer.Graph.RefreshBase();
            return result;
        }

        public static DeriveAssetTest<TAsset, TAssetPropertyGraph> LoadFromYaml(string baseYaml, string derivedYaml)
        {
            var container = new AssetPropertyGraphContainer(new AssetNodeContainer { NodeBuilder = { NodeFactory = new AssetNodeFactory() } });
            var baseAsset = AssetFileSerializer.Load<TAsset>(AssetTestContainer.ToStream(baseYaml), $"MyAsset{Types.FileExtension}");
            var derivedAsset = AssetFileSerializer.Load<TAsset>(AssetTestContainer.ToStream(derivedYaml), $"MyDerivedAsset{Types.FileExtension}");
            var baseContainer = new AssetTestContainer<TAsset, TAssetPropertyGraph>(container, baseAsset.Asset);
            var derivedContainer = new AssetTestContainer<TAsset, TAssetPropertyGraph>(container, derivedAsset.Asset);
            baseAsset.YamlMetadata.CopyInto(baseContainer.AssetItem.YamlMetadata);
            derivedAsset.YamlMetadata.CopyInto(derivedContainer.AssetItem.YamlMetadata);
            baseContainer.BuildGraph();
            derivedContainer.BuildGraph();
            var result = new DeriveAssetTest<TAsset, TAssetPropertyGraph>(baseContainer, derivedContainer);
            derivedContainer.Graph.RefreshBase();
            return result;
        }
    }
}
