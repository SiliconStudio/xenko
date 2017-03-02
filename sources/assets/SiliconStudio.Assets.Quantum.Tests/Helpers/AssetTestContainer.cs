using System.IO;
using NUnit.Framework;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public class AssetTestContainer
    {
        private readonly LoggerResult logger = new LoggerResult();

        public AssetTestContainer(AssetPropertyGraphContainer container, Asset asset)
        {
            Container = container;
            AssetItem = new AssetItem("MyAsset", asset);
        }

        public AssetPropertyGraphContainer Container { get; }

        public AssetItem AssetItem { get; }

        public MyAssetBasePropertyGraph Graph { get; private set; }

        public void BuildGraph()
        {
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, AssetItem, logger);
            Container.RegisterGraph(baseGraph);
            Assert.IsInstanceOf<MyAssetBasePropertyGraph>(baseGraph);
            Graph = (MyAssetBasePropertyGraph)baseGraph;
        }

        [NotNull]
        public static Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }

    public class AssetTestContainer<T> : AssetTestContainer where T : Asset
    {
        public AssetTestContainer(AssetPropertyGraphContainer container, T asset)
            : base(container, asset)
        {
        }

        public AssetTestContainer(T asset)
            : base(new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } }), asset)
        {
        }

        public T Asset => (T)AssetItem.Asset;

        public AssetTestContainer<T> DeriveAsset()
        {
            var derivedAsset = (T)Asset.CreateDerivedAsset("MyAsset");
            var result = new AssetTestContainer<T>(Container, derivedAsset);
            result.BuildGraph();
            return result;
        }

        public static AssetTestContainer<T> LoadFromYaml(string yaml)
        {
            var asset = AssetFileSerializer.Load<T>(ToStream(yaml), $"MyAsset{Types.FileExtension}");
            var graphContainer = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer { NodeBuilder = { ContentFactory = new AssetNodeFactory() } });
            var assetContainer = new AssetTestContainer<T>(graphContainer, asset.Asset);
            asset.YamlMetadata.CopyInto(assetContainer.AssetItem.YamlMetadata);
            assetContainer.BuildGraph();
            return assetContainer;
        }
    }
}
