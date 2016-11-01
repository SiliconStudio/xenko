using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests
{
    [TestFixture]
    public class TestReconcileWithBase
    {
        [Test]
        public void TestPrimitiveMember()
        {
            const string primitiveMemberBaseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyString: MyBaseString
";
            const string primitiveMemberOverridenYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
Id: 30000000-0000-0000-0000-000000000000
Tags: []
MyString*: MyDerivedString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyString: String
";
            const string primitiveMemberToReconcileYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyString: MyDerivedString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset1,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyString: String
";
            var context = DeriveAssetTest<Types.MyAsset1>.LoadFromYaml(primitiveMemberBaseYaml, primitiveMemberOverridenYaml);
            Assert.AreEqual("MyBaseString", context.BaseAsset.MyString);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyString);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual("MyBaseString", context.BaseAsset.MyString);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyString);

            context = DeriveAssetTest<Types.MyAsset1>.LoadFromYaml(primitiveMemberBaseYaml, primitiveMemberToReconcileYaml);
            Assert.AreEqual("MyBaseString", context.BaseAsset.MyString);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyString);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual("MyBaseString", context.BaseAsset.MyString);
            Assert.AreEqual("MyBaseString", context.DerivedAsset.MyString);
        }

        [Test]
        public void TestCollectionMismatchItem()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: String2
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000*: MyDerivedString
    14000000140000001400000014000000: MyBaseString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            14000000140000001400000014000000: String2
";
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("MyBaseString", context.DerivedAsset.MyStrings[1]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.DerivedAsset.MyStrings[1]);
        }

        [Test]
        public void TestCollectionAddedItemInBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    15000000150000001500000015000000: String2.5
    14000000140000001400000014000000: String2
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: String2
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            15000000150000001500000015000000: String2.5
            14000000140000001400000014000000: String2
";
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(3, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String2.5", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual("String2", context.BaseAsset.MyStrings[2]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.DerivedAsset.MyStrings[1]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(3, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String2.5", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual("String2", context.BaseAsset.MyStrings[2]);
            Assert.AreEqual(3, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String2.5", context.DerivedAsset.MyStrings[1]);
            Assert.AreEqual("String2", context.DerivedAsset.MyStrings[2]);
        }

        [Test]
        public void TestCollectionRemovedItemFromBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    14000000140000001400000014000000: String3
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    24000000240000002400000024000000: String2
    14000000140000001400000014000000: String3
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            14000000140000001400000014000000: String3
";
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(3, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.DerivedAsset.MyStrings[1]);
            Assert.AreEqual("String3", context.DerivedAsset.MyStrings[2]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.DerivedAsset.MyStrings[1]);
            var ids = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            Assert.AreEqual(0, ids.DeletedCount);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            Assert.AreEqual(0, ids.DeletedCount);
        }

        [Test]
        public void TestCollectionRemovedDeletedItemFromBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    24000000240000002400000024000000: String3
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
Struct:
    MyStrings: {}
MyStrings:
    0a0000000a0000000a0000000a000000: String1
    24000000240000002400000024000000: String2
    14000000140000001400000014000000: ~(Deleted)
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset2,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        Struct:
            MyStrings: {}
        MyStrings:
            0a0000000a0000000a0000000a000000: String1
            14000000140000001400000014000000: String3
";
            var context = DeriveAssetTest<Types.MyAsset2>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String2", context.DerivedAsset.MyStrings[1]);
            var ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            Assert.AreEqual(1, ids.DeletedCount);
            Assert.True(ids.DeletedItems.Contains(IdentifierGenerator.Get(20)));
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.BaseAsset.MyStrings[1]);
            Assert.AreEqual(2, context.DerivedAsset.MyStrings.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyStrings[0]);
            Assert.AreEqual("String3", context.DerivedAsset.MyStrings[1]);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyStrings);
            Assert.AreEqual(0, ids.DeletedCount);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyStrings);
            Assert.AreEqual(0, ids.DeletedCount);
        }

        [Test]
        public void TestDictionaryMismatchValue()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key2: String2
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1*: MyDerivedString
    14000000140000001400000014000000~Key2: MyBaseString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("MyBaseString", context.DerivedAsset.MyDictionary["Key2"]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2"]);
        }

        [Test]
        public void TestDictionaryAddedKeyInBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    15000000150000001500000015000000~Key2.5: String2.5
    14000000140000001400000014000000~Key2: String2
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key2: String2
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2.5", context.BaseAsset.MyDictionary["Key2.5"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2"]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2.5", context.BaseAsset.MyDictionary["Key2.5"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2"]);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2.5", context.DerivedAsset.MyDictionary["Key2.5"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2"]);
        }

        [Test]
        public void TestDictionaryRemovedItemFromBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    14000000140000001400000014000000~Key3: String3
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    24000000240000002400000024000000~Key2: String2
    14000000140000001400000014000000~Key3: String3
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3"]);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2"]);
            Assert.AreEqual("String3", context.DerivedAsset.MyDictionary["Key3"]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.DerivedAsset.MyDictionary["Key3"]);
            var ids = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
        }

        [Test]
        public void TestDictionaryRemovedDeletedItemFromBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    24000000240000002400000024000000~Key3: String3
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    24000000240000002400000024000000~Key3: String2
    14000000140000001400000014000000~Key2: ~(Deleted)
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key3"]);
            var ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            Assert.AreEqual(1, ids.DeletedCount);
            Assert.True(ids.DeletedItems.Contains(IdentifierGenerator.Get(20)));
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(2, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3"]);
            Assert.AreEqual(2, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String3", context.DerivedAsset.MyDictionary["Key3"]);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
        }

        [Test]
        public void TestDictionaryRenameItemFromBase()
        {
            const string baseYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 10000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    24000000240000002400000024000000~Key2Renamed: String2
    14000000140000001400000014000000~Key3Renamed: String3
";
            const string derivedYaml = @"!SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
Id: 20000000-0000-0000-0000-000000000000
Tags: []
MyDictionary:
    0a0000000a0000000a0000000a000000~Key1: String1
    24000000240000002400000024000000~Key2: String2
    14000000140000001400000014000000~Key3*: MyDerivedString
~Base:
    Location: MyAsset
    Asset: !SiliconStudio.Assets.Quantum.Tests.Types+MyAsset3,SiliconStudio.Assets.Quantum.Tests
        Id: 10000000-0000-0000-0000-000000000000
        Tags: []
        MyDictionary:
            0a0000000a0000000a0000000a000000~Key1: String1
            14000000140000001400000014000000~Key2: String2
";
            var context = DeriveAssetTest<Types.MyAsset3>.LoadFromYaml(baseYaml, derivedYaml);
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2Renamed"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3Renamed"]);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2"]);
            Assert.AreEqual("MyDerivedString", context.DerivedAsset.MyDictionary["Key3"]);
            context.DerivedGraph.ReconcileWithBase();
            Assert.AreEqual(3, context.BaseAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.BaseAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.BaseAsset.MyDictionary["Key2Renamed"]);
            Assert.AreEqual("String3", context.BaseAsset.MyDictionary["Key3Renamed"]);
            Assert.AreEqual(3, context.DerivedAsset.MyDictionary.Count);
            Assert.AreEqual("String1", context.DerivedAsset.MyDictionary["Key1"]);
            Assert.AreEqual("String2", context.DerivedAsset.MyDictionary["Key2Renamed"]);
            Assert.AreEqual("String3", context.DerivedAsset.MyDictionary["Key3"]);
            var ids = CollectionItemIdHelper.GetCollectionItemIds(context.BaseAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
            ids = CollectionItemIdHelper.GetCollectionItemIds(context.DerivedAsset.MyDictionary);
            Assert.AreEqual(0, ids.DeletedCount);
        }
    }
}
