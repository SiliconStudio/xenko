using System.Linq;
using NUnit.Framework;
using SiliconStudio.Assets.Quantum.Tests.Helpers;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestAssetCompositeHierarchyBases
    {
        public void TestSimplePropertyChangeInBase()
        {
            var baseAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1, null, x => x.Parts[x.RootPartIds.Single()].Part.Name = "BaseName");
            var derivedAsset = AssetHierarchyHelper.BuildAssetContainer(1, 2, 1);
            var instances = baseAsset.Asset.CreatePartInstances();
            var baseRootId = baseAsset.Asset.Hierarchy.RootPartIds.Single();
            var derivedRootId = instances.RootPartIds.Single();
            derivedAsset.Graph.AddPartToAsset(instances.Parts, instances.Parts[derivedRootId], null, 1);
            Assert.AreEqual(2, derivedAsset.Asset.Hierarchy.RootPartIds.Count);
            Assert.AreEqual(baseRootId, derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Base?.BasePartId);
            Assert.AreEqual("BaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            var baseRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId].Part);
            var derivedRootPartNode = (IAssetObjectNode)baseAsset.Graph.Container.NodeContainer.GetNode(derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part);
            //var rootPartNode = baseAsset.Graph.Container.NodeContainer.GetNode(baseAsset.Asset.Hierarchy.Parts[baseRootId]);
            baseRootPartNode["Name"].Update("NewBaseName");
            Assert.AreEqual("NewBaseName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            baseRootPartNode["Name"].Update("NewBaseName2");
            Assert.AreEqual("NewBaseName2", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            derivedRootPartNode["Name"].Update("NewDerivedName");
            Assert.AreEqual(true, derivedRootPartNode["Name"].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
            baseRootPartNode["Name"].Update("NewBaseName3");
            Assert.AreEqual(true, derivedRootPartNode["Name"].IsContentOverridden());
            Assert.AreEqual("NewDerivedName", derivedAsset.Asset.Hierarchy.Parts[derivedRootId].Part.Name);
        }
    }
}
