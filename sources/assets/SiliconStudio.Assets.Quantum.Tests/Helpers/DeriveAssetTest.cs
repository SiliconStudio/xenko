namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public class DeriveAssetTest<T> where T : Asset
    {
        private DeriveAssetTest(AssetTestContainer<T> baseAsset, AssetTestContainer<T> derivedAsset)
        {
            Base = baseAsset;
            Derived = derivedAsset;
        }

        public T BaseAsset => (T)BaseAssetItem.Asset;
        public T DerivedAsset => (T)DerivedAssetItem.Asset;
        public AssetItem BaseAssetItem => Base.AssetItem;
        public AssetItem DerivedAssetItem => Derived.AssetItem;
        public MyAssetBasePropertyGraph BaseGraph => Base.Graph;
        public MyAssetBasePropertyGraph DerivedGraph => Derived.Graph;

        public AssetTestContainer<T> Base { get; }
        public AssetTestContainer<T> Derived { get; }

        public static DeriveAssetTest<T> DeriveAsset(T baseAsset)
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var baseContainer = new AssetTestContainer<T>(container, baseAsset);
            baseContainer.BuildGraph();
            var derivedAsset = (T)baseContainer.Asset.CreateDerivedAsset("MyAsset");
            var derivedContainer = new AssetTestContainer<T>(baseContainer.Container, derivedAsset);
            var result = new DeriveAssetTest<T>(baseContainer, derivedContainer);
            derivedContainer.BuildGraph();
            derivedContainer.Graph.RefreshBase();
            return result;
        }

        public static DeriveAssetTest<T> LoadFromYaml(string baseYaml, string derivedYaml)
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var baseAsset = AssetFileSerializer.Load<T>(AssetTestContainer.ToStream(baseYaml), $"MyAsset{Types.FileExtension}");
            var derivedAsset = AssetFileSerializer.Load<T>(AssetTestContainer.ToStream(derivedYaml), $"MyDerivedAsset{Types.FileExtension}");
            var baseContainer = new AssetTestContainer<T>(container, baseAsset.Asset);
            var derivedContainer = new AssetTestContainer<T>(container, derivedAsset.Asset);
            baseAsset.YamlMetadata.CopyInto(baseContainer.AssetItem.YamlMetadata);
            derivedAsset.YamlMetadata.CopyInto(derivedContainer.AssetItem.YamlMetadata);
            baseContainer.BuildGraph();
            derivedContainer.BuildGraph();
            var result = new DeriveAssetTest<T>(baseContainer, derivedContainer);
            derivedContainer.Graph.RefreshBase();
            return result;
        }
    }
}
