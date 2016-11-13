using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestArchetypes
    {
        [Test]
        public void TestSimplePropertyChange()
        {
            var asset = new Types.MyAsset1 { MyString = "String" };
            var context = DeriveAssetTest<Types.MyAsset1>.DeriveAsset(asset);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset1.MyString));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset1.MyString));

            // Initial checks
            Assert.AreEqual("String", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("String", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString");
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString");
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve());
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetContentOverride());
        }

        [Test]
        public void TestSimpleCollectionUpdate()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2" } };
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
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
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
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
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Add("String4");
            Assert.AreEqual(3, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(3)));
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
        }

        [Test]
        public void TestSimpleCollectionRemove()
        {
            var asset = new Types.MyAsset2 { MyStrings = { "String1", "String2", "String3", "String4" } };
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual(4, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String3", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(3)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(4, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
            Assert.AreEqual(baseIds[3], derivedIds[3]);

            // Update derived and check
            var derivedDeletedId = derivedIds[2];
            derivedPropertyNode.Content.Remove("String3", new Index(2));
            Assert.AreEqual(4, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String3", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(4, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(3, derivedIds.KeyCount);
            Assert.AreEqual(1, derivedIds.DeletedCount);        
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[3], derivedIds[2]);
            Assert.True(derivedIds.IsDeleted(derivedDeletedId));

            // Update base with propagation and check
            var baseDeletedId = baseIds[3];
            basePropertyNode.Content.Remove("String4", new Index(3));
            Assert.AreEqual(3, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String3", basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(3, baseIds.KeyCount);
            Assert.AreEqual(0, baseIds.DeletedCount);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(2, derivedIds.KeyCount);
            Assert.AreEqual(1, derivedIds.DeletedCount);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.True(derivedIds.IsDeleted(derivedDeletedId));
            Assert.True(!baseIds.IsDeleted(baseDeletedId));
        }

        [Test]
        public void TestCollectionInStructUpdate()
        {
            var asset = new Types.MyAsset2();
            asset.Struct.MyStrings.Add("String1");
            asset.Struct.MyStrings.Add("String2");
            var context = DeriveAssetTest<Types.MyAsset2>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.Struct.MyStrings);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.Struct.MyStrings);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.Struct)).TryGetChild(nameof(Types.MyAsset2.MyStrings));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset2.Struct)).TryGetChild(nameof(Types.MyAsset2.MyStrings));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.Struct.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.Struct.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index(1));
            Assert.AreEqual(2, context.BaseAsset.Struct.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.Struct.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index(0));
            Assert.AreEqual(2, context.BaseAsset.Struct.MyStrings.Count);
            Assert.AreEqual(2, context.DerivedAsset.Struct.MyStrings.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestSimpleDictionaryUpdate()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1"} , { "Key2", "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset3>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update base with propagation and check
            basePropertyNode.Content.Update("MyBaseString", new Index("Key2"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update derived and check
            derivedPropertyNode.Content.Update("MyDerivedString", new Index("Key1"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("MyDerivedString", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("MyBaseString", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
        }

        [Test]
        public void TestSimpleDictionaryAdd()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1" }, { "Key2", "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset3>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update derived and check
            derivedPropertyNode.Content.Add("String3", new Index("Key3"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);

            // Update base with propagation and check
            basePropertyNode.Content.Add("String4", new Index("Key4"));
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual("String4", derivedPropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key4")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key4")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
            Assert.AreEqual(baseIds["Key4"], derivedIds["Key4"]);
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyDictionary.Count);
        }

        [Test]
        public void TestSimpleDictionaryAddWithCollision()
        {
            var asset = new Types.MyAsset3 { MyDictionary = { { "Key1", "String1" }, { "Key2", "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset3>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset3.MyDictionary));

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update a key to derived and then the same key to the base
            derivedPropertyNode.Content.Add("String3", new Index("Key3"));
            basePropertyNode.Content.Add("String4", new Index("Key3"));

            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String4", basePropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual("String1", derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual("String2", derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String3", derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
            Assert.AreNotEqual(baseIds["Key3"], derivedIds["Key3"]);
            Assert.AreEqual(1, derivedIds.DeletedCount);
            Assert.AreEqual(baseIds["Key3"], derivedIds.DeletedItems.Single());
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
        }

        [Test]
        public void TestObjectCollectionUpdate()
        {
            var asset = new Types.MyAsset4 { MyObjects = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject { Value = "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset4>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyObjects);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyObjects);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset4.MyObjects));

            var objB0 = asset.MyObjects[0];
            var objB1 = asset.MyObjects[1];
            var objD0 = context.DerivedAsset.MyObjects[0];
            var objD1 = context.DerivedAsset.MyObjects[1];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check;
            var newObjB = new Types.SomeObject { Value = "MyBaseString" };
            basePropertyNode.Content.Update(newObjB, new Index(1));
            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            var newObjD = new Types.SomeObject { Value = "MyDerivedString" };
            derivedPropertyNode.Content.Update(newObjD, new Index(0));
            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("MyDerivedString", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestObjectCollectionAdd()
        {
            var asset = new Types.MyAsset4 { MyObjects = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject { Value = "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset4>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyObjects);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyObjects);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset4.MyObjects));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset4.MyObjects));

            var objB0 = asset.MyObjects[0];
            var objB1 = asset.MyObjects[1];
            var objD0 = context.DerivedAsset.MyObjects[0];
            var objD1 = context.DerivedAsset.MyObjects[1];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            var newObjD = new Types.SomeObject { Value = "String3" };
            derivedPropertyNode.Content.Add(newObjD);
            Assert.AreEqual(2, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String3", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            var newObjB = new Types.SomeObject { Value = "String4" };
            basePropertyNode.Content.Add(newObjB);
            Assert.AreEqual(3, context.BaseAsset.MyObjects.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyObjects.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreNotEqual(newObjB, derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String3", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(3))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(3)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
        }

        [Test]
        public void TestAbstractCollectionUpdate()
        {
            var asset = new Types.MyAsset5 { MyInterfaces = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject2 { Value = "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset5>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyInterfaces);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyInterfaces);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset5.MyInterfaces));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset5.MyInterfaces));

            var objB0 = asset.MyInterfaces[0];
            var objB1 = asset.MyInterfaces[1];
            var objD0 = context.DerivedAsset.MyInterfaces[0];
            var objD1 = context.DerivedAsset.MyInterfaces[1];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check;
            var newObjB = new Types.SomeObject { Value = "MyBaseString" };
            basePropertyNode.Content.Update(newObjB, new Index(1));
            Assert.AreEqual(2, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            var newObjD = new Types.SomeObject2 { Value = "MyDerivedString" };
            derivedPropertyNode.Content.Update(newObjD, new Index(0));
            Assert.AreEqual(2, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("MyDerivedString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
        }

        [Test]
        public void TestAbstractCollectionAdd()
        {
            var asset = new Types.MyAsset5 { MyInterfaces = { new Types.SomeObject { Value = "String1" }, new Types.SomeObject { Value = "String2" } } };
            var context = DeriveAssetTest<Types.MyAsset5>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyInterfaces);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyInterfaces);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset5.MyInterfaces));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset5.MyInterfaces));

            var objB0 = asset.MyInterfaces[0];
            var objB1 = asset.MyInterfaces[1];
            var objD0 = context.DerivedAsset.MyInterfaces[0];
            var objD1 = context.DerivedAsset.MyInterfaces[1];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update derived and check
            var newObjD = new Types.SomeObject { Value = "String3" };
            derivedPropertyNode.Content.Add(newObjD);
            Assert.AreEqual(2, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual("String1", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String1", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String3", ((Types.SomeObject)derivedPropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.SomeObject.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);

            // Update base with propagation and check
            var newObjB = new Types.SomeObject2 { Value = "String4" };
            basePropertyNode.Content.Add(newObjB);
            Assert.AreEqual(3, context.BaseAsset.MyInterfaces.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyInterfaces.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index(0)));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index(1)));
            Assert.AreNotEqual(newObjB, derivedPropertyNode.Content.Retrieve(new Index(2)));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index(3)));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(0))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(1))).Value);
            Assert.AreEqual("String4", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(2))).Value);
            Assert.AreEqual("String3", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index(3))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(0)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(1)));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index(2)));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index(3)));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(0)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(1)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(2)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index(3)].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds[0], derivedIds[0]);
            Assert.AreEqual(baseIds[1], derivedIds[1]);
            Assert.AreEqual(baseIds[2], derivedIds[2]);
        }

        [Test]
        public void TestAbstractDictionaryUpdate()
        {
            var asset = new Types.MyAsset6 { MyDictionary = { { "Key1", new Types.SomeObject { Value = "String1" } }, { "Key2", new Types.SomeObject2 { Value = "String2" } } } };
            var context = DeriveAssetTest<Types.MyAsset6>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset6.MyDictionary));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset6.MyDictionary));

            var objB0 = asset.MyDictionary["Key1"];
            var objB1 = asset.MyDictionary["Key2"];
            var objD0 = context.DerivedAsset.MyDictionary["Key1"];
            var objD1 = context.DerivedAsset.MyDictionary["Key2"];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update base with propagation and check;
            var newObjB = new Types.SomeObject { Value = "MyBaseString" };
            basePropertyNode.Content.Update(newObjB, new Index("Key2"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update derived and check
            var newObjD = new Types.SomeObject2 { Value = "MyDerivedString" };
            derivedPropertyNode.Content.Update(newObjD, new Index("Key1"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreNotEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("MyDerivedString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("MyBaseString", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
        }

        [Test]
        public void TestAbstractDictionaryAdd()
        {
            var asset = new Types.MyAsset6 { MyDictionary = { { "Key1", new Types.SomeObject { Value = "String1" } }, { "Key2", new Types.SomeObject2 { Value = "String2" } } } };
            var context = DeriveAssetTest<Types.MyAsset6>.DeriveAsset(asset);
            var baseIds = CollectionItemIdHelper.GetCollectionItemIds(asset.MyDictionary);
            var derivedIds = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            var basePropertyNode = (AssetNode)((IGraphNode)context.BaseGraph.RootNode).TryGetChild(nameof(Types.MyAsset6.MyDictionary));
            var derivedPropertyNode = (AssetNode)((IGraphNode)context.DerivedGraph.RootNode).TryGetChild(nameof(Types.MyAsset6.MyDictionary));

            var objB0 = asset.MyDictionary["Key1"];
            var objB1 = asset.MyDictionary["Key2"];
            var objD0 = context.DerivedAsset.MyDictionary["Key1"];
            var objD1 = context.DerivedAsset.MyDictionary["Key2"];

            // Initial checks
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(2, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update derived and check
            var newObjD = new Types.SomeObject { Value = "String3" };
            derivedPropertyNode.Content.Add(newObjD, new Index("Key3"));
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String3", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key3"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key3")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(2, baseIds.Count);
            Assert.AreEqual(3, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);

            // Update base with propagation and check
            var newObjB = new Types.SomeObject2 { Value = "String4" };
            basePropertyNode.Content.Add(newObjB, new Index("Key4"));
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual(4, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual(objB0, basePropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objB1, basePropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreEqual(newObjB, basePropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual(objD0, derivedPropertyNode.Content.Retrieve(new Index("Key1")));
            Assert.AreEqual(objD1, derivedPropertyNode.Content.Retrieve(new Index("Key2")));
            Assert.AreNotEqual(newObjB, derivedPropertyNode.Content.Retrieve(new Index("Key4")));
            Assert.AreEqual(newObjD, derivedPropertyNode.Content.Retrieve(new Index("Key3")));
            Assert.AreEqual("String1", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String4", ((Types.IMyInterface)basePropertyNode.Content.Retrieve(new Index("Key4"))).Value);
            Assert.AreEqual("String1", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key1"))).Value);
            Assert.AreEqual("String2", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key2"))).Value);
            Assert.AreEqual("String4", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key4"))).Value);
            Assert.AreEqual("String3", ((Types.IMyInterface)derivedPropertyNode.Content.Retrieve(new Index("Key3"))).Value);
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, basePropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)basePropertyNode.Content.Reference.AsEnumerable[new Index("Key4")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetContentOverride());
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key1")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key2")));
            Assert.AreEqual(OverrideType.New, derivedPropertyNode.GetItemOverride(new Index("Key3")));
            Assert.AreEqual(OverrideType.Base, derivedPropertyNode.GetItemOverride(new Index("Key4")));
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key1")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key2")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key3")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreEqual(OverrideType.Base, ((AssetNode)derivedPropertyNode.Content.Reference.AsEnumerable[new Index("Key4")].TargetNode.TryGetChild(nameof(Types.IMyInterface.Value))).GetContentOverride());
            Assert.AreNotSame(baseIds, derivedIds);
            Assert.AreEqual(3, baseIds.Count);
            Assert.AreEqual(4, derivedIds.Count);
            Assert.AreEqual(baseIds["Key1"], derivedIds["Key1"]);
            Assert.AreEqual(baseIds["Key2"], derivedIds["Key2"]);
            Assert.AreEqual(baseIds["Key4"], derivedIds["Key4"]);
        }
    }
}
