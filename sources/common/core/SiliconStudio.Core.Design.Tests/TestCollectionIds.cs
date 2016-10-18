using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Core.Design.Tests
{
    public static class GuidGenerator
    {
        public static Guid Get(int index)
        {
            var bytes = ToBytes(index);
            return new Guid(bytes);
        }

        public static bool Match(Guid guid, int index)
        {
            var bytes = ToBytes(index);
            var id = new Guid(bytes);
            return guid == id;
        }

        private static byte[] ToBytes(int index)
        {
            var bytes = new byte[16];
            for (int i = 0; i < 4; ++i)
            {
                bytes[4 * i] = (byte)(index);
                bytes[4 * i + 1] = (byte)(index >> 8);
                bytes[4 * i + 2] = (byte)(index >> 16);
                bytes[4 * i + 3] = (byte)(index >> 24);
            }
            return bytes;
        }
    }

    [TestFixture]
    public class TestCollectionIds
    {
        public class ContainerCollection
        {
            public ContainerCollection() { }
            public ContainerCollection(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
            public List<string> Strings { get; set; } = new List<string>();
            public List<ContainerCollection> Objects { get; set; } = new List<ContainerCollection>();
        }

        public class ContainerDictionary
        {
            public ContainerDictionary() { }
            public ContainerDictionary(string name)
            {
                Name = name;
            }
            public string Name { get; set; }
            public Dictionary<Guid, string> Strings { get; set; } = new Dictionary<Guid, string>();
            public Dictionary<string, ContainerCollection> Objects { get; set; } = new Dictionary<string, ContainerCollection>();
        }

        private const string YamlCollection = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerCollection,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    00000002-0002-0000-0200-000002000000: aaa
    00000001-0001-0000-0100-000001000000: bbb
Objects:
    00000003-0003-0000-0300-000003000000:
        Name: obj1
        Strings: {}
        Objects: {}
    00000004-0004-0000-0400-000004000000:
        Name: obj2
        Strings: {}
        Objects: {}
";

        private const string YamlDictionary = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerDictionary,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    00000002-0002-0000-0200-000002000000~000000c8-00c8-0000-c800-0000c8000000: aaa
    00000001-0001-0000-0100-000001000000~00000064-0064-0000-6400-000064000000: bbb
Objects:
    00000003-0003-0000-0300-000003000000~key3:
        Name: obj1
        Strings: {}
        Objects: {}
    00000004-0004-0000-0400-000004000000~key4:
        Name: obj2
        Strings: {}
        Objects: {}
";

        private const string YamlCollectionWithDeleted = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerCollection,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    00000008-0008-0000-0800-000008000000: aaa
    00000005-0005-0000-0500-000005000000: bbb
    00000001-0001-0000-0100-000001000000: ~(Deleted)
    00000003-0003-0000-0300-000003000000: ~(Deleted)
Objects:
    00000003-0003-0000-0300-000003000000:
        Name: obj1
        Strings: {}
        Objects: {}
    00000004-0004-0000-0400-000004000000:
        Name: obj2
        Strings: {}
        Objects: {}
    00000001-0001-0000-0100-000001000000: ~(Deleted)
    00000006-0006-0000-0600-000006000000: ~(Deleted)
";

        private const string YamlDictionaryWithDeleted = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerDictionary,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    00000008-0008-0000-0800-000008000000~000000c8-00c8-0000-c800-0000c8000000: aaa
    00000005-0005-0000-0500-000005000000~00000064-0064-0000-6400-000064000000: bbb
    00000001-0001-0000-0100-000001000000~: ~(Deleted)
    00000003-0003-0000-0300-000003000000~: ~(Deleted)
Objects:
    00000003-0003-0000-0300-000003000000~key3:
        Name: obj1
        Strings: {}
        Objects: {}
    00000004-0004-0000-0400-000004000000~key4:
        Name: obj2
        Strings: {}
        Objects: {}
    00000001-0001-0000-0100-000001000000~: ~(Deleted)
    00000006-0006-0000-0600-000006000000~: ~(Deleted)
";

        [Test]
        public void TestCollectionSerialization()
        {
            ShadowObject.Enable = true;
            var obj = new ContainerCollection("Root")
            {
                Strings = { "aaa", "bbb" },
                Objects = { new ContainerCollection("obj1"), new ContainerCollection("obj2") }
            };

            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            stringIds[0] = GuidGenerator.Get(2);
            stringIds[1] = GuidGenerator.Get(1);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            objectIds[0] = GuidGenerator.Get(3);
            objectIds[1] = GuidGenerator.Get(4);
            var yaml = YamlSerializer.Serialize(obj);
            Assert.AreEqual(YamlCollection, yaml);
        }

        [Test]
        public void TestCollectionDeserialization()
        {
            ShadowObject.Enable = true;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(YamlCollection);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerCollection), instance.GetType());
            var obj = (ContainerCollection)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[0]);
            Assert.AreEqual("bbb", obj.Strings[1]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects[0].Name);
            Assert.AreEqual("obj2", obj.Objects[1].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            Assert.AreEqual(GuidGenerator.Get(2), stringIds[0]);
            Assert.AreEqual(GuidGenerator.Get(1), stringIds[1]);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(GuidGenerator.Get(3), objectIds[0]);
            Assert.AreEqual(GuidGenerator.Get(4), objectIds[1]);
        }

        [Test]
        public void TestCollectionDeserializationOldWay()
        {
            ShadowObject.Enable = true;
            var yaml = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerCollection,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    - aaa
    - bbb
Objects:
    -   ~Id: 00000004-0004-0000-0400-000004000000
        Name: obj1
        Strings: {}
        Objects: {}
    -   ~Id: 00000003-0003-0000-0300-000003000000
        Name: obj2
        Strings: {}
        Objects: {}
";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(yaml);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerCollection), instance.GetType());
            var obj = (ContainerCollection)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[0]);
            Assert.AreEqual("bbb", obj.Strings[1]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects[0].Name);
            Assert.AreEqual("obj2", obj.Objects[1].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(2, stringIds.Count);
            Assert.IsTrue(stringIds.ContainsKey(0));
            Assert.IsTrue(stringIds.ContainsKey(1));
            Assert.AreEqual(2, objectIds.Count);
            Assert.IsTrue(objectIds.ContainsKey(0));
            Assert.IsTrue(objectIds.ContainsKey(1));
            Assert.AreEqual(GuidGenerator.Get(4), objectIds[0]);
            Assert.AreEqual(GuidGenerator.Get(3), objectIds[1]);
        }

        [Test]
        public void TestDictionarySerialization()
        {
            ShadowObject.Enable = true;
            var obj = new ContainerDictionary("Root")
            {
                Strings = { { GuidGenerator.Get(200), "aaa" }, { GuidGenerator.Get(100), "bbb" } },
                Objects = { { "key3", new ContainerCollection("obj1") }, { "key4", new ContainerCollection("obj2") } },
            };

            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            stringIds[GuidGenerator.Get(200)] = GuidGenerator.Get(2);
            stringIds[GuidGenerator.Get(100)] = GuidGenerator.Get(1);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            objectIds["key3"] = GuidGenerator.Get(3);
            objectIds["key4"] = GuidGenerator.Get(4);
            var yaml = YamlSerializer.Serialize(obj);
            Assert.AreEqual(YamlDictionary, yaml);
        }

        [Test]
        public void TestDictionaryDeserialization()
        {
            ShadowObject.Enable = true;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(YamlDictionary);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerDictionary), instance.GetType());
            var obj = (ContainerDictionary)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[GuidGenerator.Get(200)]);
            Assert.AreEqual("bbb", obj.Strings[GuidGenerator.Get(100)]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects["key3"].Name);
            Assert.AreEqual("obj2", obj.Objects["key4"].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            Assert.AreEqual(GuidGenerator.Get(2), stringIds[GuidGenerator.Get(200)]);
            Assert.AreEqual(GuidGenerator.Get(1), stringIds[GuidGenerator.Get(100)]);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(GuidGenerator.Get(3), objectIds["key3"]);
            Assert.AreEqual(GuidGenerator.Get(4), objectIds["key4"]);
        }

        [Test]
        public void TestDictionaryDeserializationOldWay()
        {
            ShadowObject.Enable = true;
            var yaml = @"!SiliconStudio.Core.Design.Tests.TestCollectionIds+ContainerDictionary,SiliconStudio.Core.Design.Tests
Name: Root
Strings:
    000000c8-00c8-0000-c800-0000c8000000: aaa
    00000064-0064-0000-6400-000064000000: bbb
Objects:
    key3:
        ~Id: 00000003-0003-0000-0300-000003000000
        Name: obj1
        Strings: {}
        Objects: {}
    key4:
        ~Id: 00000004-0004-0000-0400-000004000000
        Name: obj2
        Strings: {}
        Objects: {}
";

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(yaml);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerDictionary), instance.GetType());
            var obj = (ContainerDictionary)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[GuidGenerator.Get(200)]);
            Assert.AreEqual("bbb", obj.Strings[GuidGenerator.Get(100)]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects["key3"].Name);
            Assert.AreEqual("obj2", obj.Objects["key4"].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(2, stringIds.Count);
            Assert.IsTrue(stringIds.ContainsKey(GuidGenerator.Get(200)));
            Assert.IsTrue(stringIds.ContainsKey(GuidGenerator.Get(100)));
            Assert.AreEqual(2, objectIds.Count);
            Assert.IsTrue(objectIds.ContainsKey("key3"));
            Assert.IsTrue(objectIds.ContainsKey("key4"));
            Assert.AreEqual(GuidGenerator.Get(3), objectIds["key3"]);
            Assert.AreEqual(GuidGenerator.Get(4), objectIds["key4"]);
        }

        [Test]
        public void TestCollectionDeserializationWithDeleted()
        {
            ShadowObject.Enable = true;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(YamlCollectionWithDeleted);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerCollection), instance.GetType());
            var obj = (ContainerCollection)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[0]);
            Assert.AreEqual("bbb", obj.Strings[1]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects[0].Name);
            Assert.AreEqual("obj2", obj.Objects[1].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            Assert.AreEqual(GuidGenerator.Get(8), stringIds[0]);
            Assert.AreEqual(GuidGenerator.Get(5), stringIds[1]);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(GuidGenerator.Get(3), objectIds[0]);
            Assert.AreEqual(GuidGenerator.Get(4), objectIds[1]);
            var deletedItems = stringIds.DeletedItems.ToList();
            Assert.AreEqual(2, deletedItems.Count);
            Assert.AreEqual(GuidGenerator.Get(1), deletedItems[0]);
            Assert.AreEqual(GuidGenerator.Get(3), deletedItems[1]);
            deletedItems = objectIds.DeletedItems.ToList();
            Assert.AreEqual(2, deletedItems.Count);
            Assert.AreEqual(GuidGenerator.Get(1), deletedItems[0]);
            Assert.AreEqual(GuidGenerator.Get(6), deletedItems[1]);
        }

        [Test]
        public void TestCollectionSerializationWithDeleted()
        {
            ShadowObject.Enable = true;
            var obj = new ContainerCollection("Root")
            {
                Strings = { "aaa", "bbb" },
                Objects = { new ContainerCollection("obj1"), new ContainerCollection("obj2") }
            };

            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            stringIds[0] = GuidGenerator.Get(8);
            stringIds[1] = GuidGenerator.Get(5);
            stringIds.MarkAsDeleted(GuidGenerator.Get(3));
            stringIds.MarkAsDeleted(GuidGenerator.Get(1));
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            objectIds[0] = GuidGenerator.Get(3);
            objectIds[1] = GuidGenerator.Get(4);
            objectIds.MarkAsDeleted(GuidGenerator.Get(1));
            objectIds.MarkAsDeleted(GuidGenerator.Get(6));
            var yaml = YamlSerializer.Serialize(obj);
            Assert.AreEqual(YamlCollectionWithDeleted, yaml);
        }

        [Test]
        public void TestDictionaryDeserializationWithDeleted()
        {
            ShadowObject.Enable = true;
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(YamlDictionaryWithDeleted);
            writer.Flush();
            stream.Position = 0;
            var instance = YamlSerializer.Deserialize(stream);
            Assert.NotNull(instance);
            Assert.AreEqual(typeof(ContainerDictionary), instance.GetType());
            var obj = (ContainerDictionary)instance;
            Assert.AreEqual("Root", obj.Name);
            Assert.AreEqual(2, obj.Strings.Count);
            Assert.AreEqual("aaa", obj.Strings[GuidGenerator.Get(200)]);
            Assert.AreEqual("bbb", obj.Strings[GuidGenerator.Get(100)]);
            Assert.AreEqual(2, obj.Objects.Count);
            Assert.AreEqual("obj1", obj.Objects["key3"].Name);
            Assert.AreEqual("obj2", obj.Objects["key4"].Name);
            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            Assert.AreEqual(GuidGenerator.Get(8), stringIds[GuidGenerator.Get(200)]);
            Assert.AreEqual(GuidGenerator.Get(5), stringIds[GuidGenerator.Get(100)]);
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            Assert.AreEqual(GuidGenerator.Get(3), objectIds["key3"]);
            Assert.AreEqual(GuidGenerator.Get(4), objectIds["key4"]);
            var deletedItems = stringIds.DeletedItems.ToList();
            Assert.AreEqual(2, deletedItems.Count);
            Assert.AreEqual(GuidGenerator.Get(1), deletedItems[0]);
            Assert.AreEqual(GuidGenerator.Get(3), deletedItems[1]);
            deletedItems = objectIds.DeletedItems.ToList();
            Assert.AreEqual(2, deletedItems.Count);
            Assert.AreEqual(GuidGenerator.Get(1), deletedItems[0]);
            Assert.AreEqual(GuidGenerator.Get(6), deletedItems[1]);
        }

        [Test]
        public void TestDictionarySerializationWithDeleted()
        {
            ShadowObject.Enable = true;
            var obj = new ContainerDictionary("Root")
            {
                Strings = { { GuidGenerator.Get(200), "aaa" }, { GuidGenerator.Get(100), "bbb" } },
                Objects = { { "key3", new ContainerCollection("obj1") }, { "key4", new ContainerCollection("obj2") } },
            };

            var stringIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Strings);
            stringIds[GuidGenerator.Get(200)] = GuidGenerator.Get(8);
            stringIds[GuidGenerator.Get(100)] = GuidGenerator.Get(5);
            stringIds.MarkAsDeleted(GuidGenerator.Get(3));
            stringIds.MarkAsDeleted(GuidGenerator.Get(1));
            var objectIds = CollectionItemIdHelper.GetCollectionItemIds(obj.Objects);
            objectIds["key3"] = GuidGenerator.Get(3);
            objectIds["key4"] = GuidGenerator.Get(4);
            objectIds.MarkAsDeleted(GuidGenerator.Get(1));
            objectIds.MarkAsDeleted(GuidGenerator.Get(6));
            var yaml = YamlSerializer.Serialize(obj);
            Assert.AreEqual(YamlDictionaryWithDeleted, yaml);
        }
    }
}
