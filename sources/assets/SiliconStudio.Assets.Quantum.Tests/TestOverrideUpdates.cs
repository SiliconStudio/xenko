using NUnit.Framework;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    public class DeriveAssetTest<T> where T : Asset
    {
        public DeriveAssetTest(AssetItem baseAssetItem)
        {
            Container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            BaseAssetItem = baseAssetItem;
            var derivedAsset = BaseAssetItem.Asset.CreateDerivedAsset(BaseAssetItem.Location);
            DerivedAssetItem = new AssetItem("MyDerivedAsset", derivedAsset);
            BaseGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, BaseAssetItem);
            DerivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(Container, DerivedAssetItem);
            DerivedGraph.RefreshBase(BaseGraph);
        }

        public AssetPropertyGraphContainer Container { get; }
        public T BaseAsset => (T)BaseAssetItem.Asset;
        public T DerivedAsset => (T)DerivedAssetItem.Asset;
        public AssetItem BaseAssetItem { get; }
        public AssetItem DerivedAssetItem { get; }
        public AssetPropertyGraph BaseGraph { get; private set; }
        public AssetPropertyGraph DerivedGraph { get; private set; }
    }

    [TestFixture]
    public class TestArchetypes
    {
        /* test TODO:
         * Non-abstract class (test result recursively) : simple prop + in collection
         * Abstract (interface) override with different type
         * class prop set to null
         */

        [Test]
        public void TestSimplePropertyChange()
        {
            var asset = new Types.MyAsset1 { MyString = "String" };
            var assetItem = new AssetItem("MyAsset", asset);
            var context = new DeriveAssetTest<Types.MyAsset1>(assetItem);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset1.MyString));

            // Initial checks
            Assert.AreEqual("String", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("String", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString");
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(Index.Empty));
        }

        [Test]
        public void TestSimpleCollectionUpdate()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var context = new DeriveAssetTest<Types.MyAsset2>(assetItem);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotEqual(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotEqual(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotEqual(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestSimpleDictionaryUpdate()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1"} , { "Key2", "String2" } } };
            var assetItem = new AssetItem("MyAsset", asset);
            var context = new DeriveAssetTest<Types.MyAsset3>(assetItem);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset3.MyDictionary));

            // Initial checks
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index("Key2"));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index("Key1"));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
        }

        [Test]
        public void TestCollectionInStructUpdate()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            asset.Struct.MyStrings.Add("String1");
            asset.Struct.MyStrings.Add("String2");
            var assetItem = new AssetItem("MyAsset", asset);
            var context = new DeriveAssetTest<Types.MyAsset2>(assetItem);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.Struct)).GetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestSimpleCollectionAdd()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var context = new DeriveAssetTest<Types.MyAsset2>(assetItem);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)context.BaseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)context.DerivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            derivedPropertyNode.Content.Add("String3");
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(2)));
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Add("String4");
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetOverride(new Index(3)));
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[3]);
        }
    }

    [TestFixture]
    public class TestOverrideUpdates
    {
        [Test]
        public void TestSimpleCollectionAddInArchetype()
        {
            var container = new AssetPropertyGraphContainer(new PackageSession(), new AssetNodeContainer());
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var assetItem = new AssetItem("MyAsset", asset);
            var derivedAsset = (Types.MyAsset2)asset.CreateDerivedAsset(assetItem.Location);
            var derivedItem = new AssetItem("MyDerivedAsset", derivedAsset);
            var baseGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, assetItem);
            var derivedGraph = AssetQuantumRegistry.ConstructPropertyGraph(container, derivedItem);
            derivedGraph.RefreshBase(baseGraph);

            var basePropertyNode = (AssetNode)baseGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)derivedGraph.RootNode.GetChild(nameof(Types.MyAsset2.MyStrings));
            basePropertyNode.Content.Add("String3");
            Assert.AreEqual(3, derivedAsset.MyStrings.Count);
            Assert.AreEqual("String3", derivedAsset.MyStrings[2]);
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(Index.Empty));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetOverride(new Index(2)));
        }
    }
}
