using NUnit.Framework;
using SiliconStudio.Assets.Tests.Helpers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [AssetPropertyGraph(typeof(Types.MyAssetWithRef2))]
    public class AssetWithRefPropertyGraph2 : MyAssetBasePropertyGraph
    {
        public AssetWithRefPropertyGraph2(AssetPropertyGraphContainer container, AssetItem assetItem, ILogger logger)
            : base(container, assetItem, logger)
        {
        }

        public override bool IsObjectReference(IGraphNode targetNode, Index index)
        {
            if ((targetNode as IMemberNode)?.Name == nameof(Types.MyAssetWithRef2.Reference))
                return true;
            if ((targetNode as IObjectNode)?.Retrieve() == ((Types.MyAssetWithRef2)Asset).References)
                return true;

            return false;
        }
    }

    [TestFixture]
    public class TestObjectReferenceGraph
    {
        [Test]
        public void TestSimpleObjectReferenceGraph()
        {
            var obj = new Types.MyReferenceable { Id = GuidGenerator.Get(2), Value = "MyInstance" };
            var asset = new Types.MyAssetWithRef2 { NonReference = obj, Reference = obj };
            var context = DeriveAssetTest<Types.MyAssetWithRef2>.DeriveAsset(asset);
            Assert.AreEqual(Types.MyAssetWithRef2.MemberCount, context.BaseGraph.RootNode.Members.Count);
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target.Retrieve());
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target.Retrieve());

            Assert.True(context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].IsReference);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)], context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target.BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)], context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target.BaseNode);
        }

        [Test]
        public void TestUpdateObjectReferenceGraph()
        {
            var obj = new Types.MyReferenceable { Id = GuidGenerator.Get(2), Value = "MyInstance" };
            var asset = new Types.MyAssetWithRef2 { NonReference = obj, Reference = obj };
            var context = DeriveAssetTest<Types.MyAssetWithRef2>.DeriveAsset(asset);
            Assert.AreEqual(Types.MyAssetWithRef2.MemberCount, context.BaseGraph.RootNode.Members.Count);
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target.Retrieve());
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target.Retrieve());

            Assert.True(context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].IsReference);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)], context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target.BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)], context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.Reference)].Target.BaseNode);
            // TODO
        }

        [Test]
        public void TestCollectionObjectReferenceGraph()
        {
            var obj = new Types.MyReferenceable { Id = GuidGenerator.Get(2), Value = "MyInstance" };
            var asset = new Types.MyAssetWithRef2 { NonReference = obj, References = { obj } };
            var context = DeriveAssetTest<Types.MyAssetWithRef2>.DeriveAsset(asset);
            Assert.AreEqual(Types.MyAssetWithRef2.MemberCount, context.BaseGraph.RootNode.Members.Count);
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target.Retrieve());
            Assert.True(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].IsReference);
            Assert.AreEqual(obj, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].Target.Retrieve(new Index(0)));
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target, context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].Target.IndexedTarget(new Index(0)));

            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.References)], context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].Target.BaseNode);
            Assert.AreEqual(context.BaseGraph.RootNode[nameof(Types.MyAssetWithRef2.NonReference)].Target, context.DerivedGraph.RootNode[nameof(Types.MyAssetWithRef2.References)].Target.IndexedTarget(new Index(0)).BaseNode);
        }
    }
}
