using System;
using System.IO;
using NUnit.Framework;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;

// ReSharper disable ConvertToLambdaExpression

namespace SiliconStudio.Assets.Quantum.Tests
{
    [AssetPropertyGraph(typeof(Types.MyAssetWithRef))]
    public class AssetWithRefPropertyGraph : MyAssetBasePropertyGraph
    {
        public AssetWithRefPropertyGraph(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public Func<IGraphNode, Index, object, bool> IsObjectReferenceFunc { get; set; }

        public override bool IsObjectReference(IGraphNode targetNode, Index index, object value)
        {
            return IsObjectReferenceFunc?.Invoke(targetNode, index, value) ?? base.IsObjectReference(targetNode, index, value);
        }
    }

    [TestFixture]
    public class TestObjectReferenceSerialization
    {
        private static readonly AssetId BaseId = (AssetId)GuidGenerator.Get(1);
        private static readonly AssetId DerivedId = (AssetId)GuidGenerator.Get(2);

        private static void SerializeAndCompare(AssetItem assetItem, AssetPropertyGraph graph, string expectedYaml, bool isDerived)
        {
            assetItem.Asset.Id = isDerived ? DerivedId : BaseId;
            Assert.AreEqual(isDerived, assetItem.Asset.Archetype != null);
            if (isDerived)
                assetItem.Asset.Archetype = new AssetReference(BaseId, assetItem.Asset.Archetype?.Location);
            graph.PrepareForSave(null, assetItem);
            var stream = new MemoryStream();
            AssetFileSerializer.Save(stream, assetItem.Asset, assetItem.YamlMetadata, null);
            stream.Position = 0;
            var streamReader = new StreamReader(stream);
            var yaml = streamReader.ReadToEnd();
            Assert.AreEqual(expectedYaml, yaml);
        }

        private const string SimpleReferenceYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAssetWithRef,SiliconStudio.Assets.Quantum.Tests
Id: 00000001-0001-0000-0100-000001000000
Tags: []
MyObject1:
    Value: MyInstance
    Id: 00000002-0002-0000-0200-000002000000
MyObject2: ref!! 00000002-0002-0000-0200-000002000000
MyObjects: {}
MyNonIdObjects: []
";

        [Test]
        public void TestSimpleReference()
        {
            var obj = new Types.MyReferenceable { Id = GuidGenerator.Get(2), Value = "MyInstance" };
            var asset = new Types.MyAssetWithRef { MyObject1 = obj, MyObject2 = obj };
            var context = DeriveAssetTest<Types.MyAssetWithRef>.DeriveAsset(asset);
            ((AssetWithRefPropertyGraph)context.BaseGraph).IsObjectReferenceFunc = (targetNode, index, value) =>
            {
                return (targetNode as IMemberNode)?.Name == nameof(Types.MyAssetWithRef.MyObject2);
            };
            SerializeAndCompare(context.BaseAssetItem, context.BaseGraph, SimpleReferenceYaml, false);

            context = DeriveAssetTest<Types.MyAssetWithRef>.LoadFromYaml(SimpleReferenceYaml, SimpleReferenceYaml);
            Assert.AreEqual(context.BaseAsset.MyObject1, context.BaseAsset.MyObject2);
            Assert.AreEqual(GuidGenerator.Get(2), context.BaseAsset.MyObject1.Id);
        }
    }
}
