using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchyBases
    {
        [Test]
        public void TestSimplePropertyChangeInBase()
        {
            var baseAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, null, x => x.Parts[x.RootPartIds.Single()].Part.Name = "BaseName");
            var derivedAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, baseAsset.Container);
            var instances = baseAsset.Asset.CreatePartInstances();
            var baseRootId = baseAsset.Asset.Hierarchy.RootPartIds.Single();
            var derivedRootId = instances.RootPartIds.Single();
            derivedAsset.Graph.AddPartToAsset(instances.Parts, instances.Parts[derivedRootId], null, 1);
            derivedAsset.Graph.RefreshBase();
            Assert.AreEqual(2, derivedAsset.Asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseRootId, derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Base?.BasePartId);
            Assert.AreEqual("BaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            var baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            var derivedRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part);
            baseRootPartNode[nameof(Types.MyPart.Name)].Update("NewBaseName");
            Assert.AreEqual("NewBaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            baseRootPartNode[nameof(Types.MyPart.Name)].Update("NewBaseName2");
            Assert.AreEqual("NewBaseName2", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            derivedRootPartNode[nameof(Types.MyPart.Name)].Update("NewDerivedName");
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.MyPart.Name)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            baseRootPartNode[nameof(Types.MyPart.Name)].Update("NewBaseName3");
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.MyPart.Name)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
        }

        [Test]
        public void TestSimpleNestedPropertyChangeInBase()
        {
            var baseAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, null, x => x.Parts[x.RootPartIds.Single()].Part.Object = new Types.SomeObject { Value = "BaseName" });
            var derivedAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, baseAsset.Container);
            var instances = baseAsset.Asset.CreatePartInstances();
            var baseRootId = baseAsset.Asset.Hierarchy.RootPartIds.Single();
            var derivedRootId = instances.RootPartIds.Single();
            derivedAsset.Graph.AddPartToAsset(instances.Parts, instances.Parts[derivedRootId], null, 1);
            derivedAsset.Graph.RefreshBase();
            Assert.AreEqual(2, derivedAsset.Asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseRootId, derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Base?.BasePartId);
            Assert.AreEqual("BaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            var baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part.Object);
            var derivedRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object);
            baseRootPartNode[nameof(Types.SomeObject.Value)].Update("NewBaseName");
            Assert.AreEqual("NewBaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            baseRootPartNode[nameof(Types.SomeObject.Value)].Update("NewBaseName2");
            Assert.AreEqual("NewBaseName2", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            derivedRootPartNode[nameof(Types.SomeObject.Value)].Update("NewDerivedName");
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.SomeObject.Value)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            baseRootPartNode[nameof(Types.SomeObject.Value)].Update("NewBaseName3");
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.SomeObject.Value)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
        }

        [Test, Ignore("Overriding an object does not override its member currently, so this test cannot pass")]
        public void TestObjectPropertyChangeInBase()
        {
            var baseAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, null, x => x.Parts[x.RootPartIds.Single()].Part.Object = new Types.SomeObject { Value = "BaseName" });
            var derivedAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, baseAsset.Container);
            var instances = baseAsset.Asset.CreatePartInstances();
            var baseRootId = baseAsset.Asset.Hierarchy.RootPartIds.Single();
            var derivedRootId = instances.RootPartIds.Single();
            derivedAsset.Graph.AddPartToAsset(instances.Parts, instances.Parts[derivedRootId], null, 1);
            derivedAsset.Graph.RefreshBase();
            Assert.AreEqual(2, derivedAsset.Asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseRootId, derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Base?.BasePartId);
            Assert.AreEqual("BaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            var baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            baseRootPartNode[nameof(Types.MyPart.Object)].Update(new Types.SomeObject { Value = "NewBaseName" });
            Assert.AreEqual("NewBaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            baseRootPartNode[nameof(Types.MyPart.Object)].Update(new Types.SomeObject { Value = "NewBaseName2" });
            Assert.AreEqual("NewBaseName2", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            var derivedRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part);
            derivedRootPartNode[nameof(Types.MyPart.Object)].Update(new Types.SomeObject { Value = "NewDerivedName" });
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.MyPart.Object)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            derivedRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part);
            baseRootPartNode[nameof(Types.MyPart.Object)].Update(new Types.SomeObject { Value = "NewBaseName3" });
            Assert.AreEqual(true, derivedRootPartNode[nameof(Types.MyPart.Object)].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
        }

        [Test]
        public void TestMultiplePropertyChangesInBase()
        {
            var baseAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, null, x => x.Parts[x.RootPartIds.Single()].Part.Name = "BaseName");
            var derivedAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, baseAsset.Container);
            var instances = baseAsset.Asset.CreatePartInstances();
            var baseRootId = baseAsset.Asset.Hierarchy.RootPartIds.Single();
            var derivedRootId = instances.RootPartIds.Single();
            derivedAsset.Graph.AddPartToAsset(instances.Parts, instances.Parts[derivedRootId], null, 1);
            derivedAsset.Graph.RefreshBase();
            Assert.AreEqual(2, derivedAsset.Asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseRootId, derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Base?.BasePartId);
            Assert.AreEqual("BaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            var baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            baseRootPartNode[nameof(Types.MyPart.Object)].Update(new Types.SomeObject { Value = "NewBaseValue" });
            Assert.NotNull(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object);
            Assert.AreEqual("NewBaseValue", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Object.Value);
            baseRootPartNode[nameof(Types.MyPart.Name)].Update("NewBaseName");
            Assert.AreEqual("NewBaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
        }

    }
}
