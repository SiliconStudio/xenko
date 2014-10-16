// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.BuildEngine.Tests.Commands;

namespace SiliconStudio.BuildEngine.Tests
{
    [TestFixture]
    public class TestMetadata
    {
        private readonly MetadataKey[] keys = new[]
                    {
                        new MetadataKey("key1", MetadataKey.DatabaseType.String),
                        new MetadataKey("key2", MetadataKey.DatabaseType.Char),
                        new MetadataKey("key3", MetadataKey.DatabaseType.Float),
                        new MetadataKey("key4", MetadataKey.DatabaseType.Short)
                    };

        private readonly string[] urls = new[] { "a/test/url", "another/url" };

        private readonly object[] values1 = new object[] { "string 1", 'A', 2.5f, 42 };

        private readonly object[] values2 = new object[] { "string 2", 'B', 3.1f, 69 };


        // ReSharper disable PossibleMultipleEnumeration
        [Test]
        public void TestCreation()
        {
            const string DBPath = "test1.db";
            if (File.Exists(DBPath))
                File.Delete(DBPath);

            using (var provider = new QueryMetadataProvider())
            {
                const string TestKey = "TestKey";
                const string TestUrl = "a/test/url";
                const string TestValue = "test value";

                // Create database
                bool result = provider.Create(DBPath);
                Assert.True(result);
                // Add a key and retrieve it
                result = provider.AddKey(new MetadataKey(TestKey, MetadataKey.DatabaseType.String));
                Assert.True(result);
                IEnumerable<MetadataKey> fetchedKeys = provider.FetchAllKeys();
                Assert.NotNull(fetchedKeys);
                Assert.AreEqual(fetchedKeys.Count(), 1);
                MetadataKey key = fetchedKeys.First();
                Assert.NotNull(key);
                Assert.AreEqual(key.Name, TestKey);
                Assert.AreEqual(key.Type, MetadataKey.DatabaseType.String);

                // Add a metadata and retrieve it
                var stringMetadata = new ObjectMetadata<string>(TestUrl, key, TestValue);
                result = provider.Write(stringMetadata);
                Assert.True(result);

                IEnumerable<string> fetchedUrls = provider.FetchAllObjectUrls();
                Assert.NotNull(fetchedUrls);
                Assert.AreEqual(fetchedUrls.Count(), 1);
                string url = fetchedUrls.First();
                Assert.AreEqual(url, TestUrl);

                IEnumerable<IObjectMetadata> metadata = provider.Fetch(url);
                Assert.NotNull(metadata);
                Assert.AreEqual(metadata.Count(), 1);
                IObjectMetadata metadatum = metadata.First();
                Assert.AreEqual(metadatum.Key, key);
                Assert.AreEqual(metadatum.ObjectUrl, TestUrl);
                Assert.AreEqual(metadatum.Value, TestValue);

                provider.Close();
            }
        }

        [Test]
        public void TestFetch()
        {
            const string DBPath = "test2.db";
            if (File.Exists(DBPath))
                File.Delete(DBPath);

            using (var provider = new QueryMetadataProvider())
            {
                // Create database and fill it
                provider.Create(DBPath);

                foreach (MetadataKey key in keys)
                    provider.AddKey(key);

                for (int i = 0; i < 4; ++i)
                {
                    provider.Write(new ObjectMetadata(urls[0], keys[i], values1[i]));
                    provider.Write(new ObjectMetadata(urls[1], keys[i], values2[i]));
                }

                // Fetch by url
                IEnumerable<IObjectMetadata> result = provider.Fetch(urls[0]);
                Assert.NotNull(result);
                Assert.AreEqual(result.Count(), 4);
                for (int i = 0; i < 4; ++i)
                {
                    IObjectMetadata obj = result.Skip(i).First();
                    Assert.AreEqual(obj.ObjectUrl, urls[0]);
                    Assert.AreEqual(obj.Key, keys[i]);
                    Assert.AreEqual(obj.Value, values1[i]);
                }
                // Fetch by key
                for (int j = 0; j < 4; ++j)
                {
                    result = provider.Fetch(keys[j]);
                    Assert.NotNull(result);
                    Assert.AreEqual(result.Count(), 2);
                    for (int i = 0; i < 2; ++i)
                    {
                        IObjectMetadata obj = result.Skip(i).First();
                        Assert.AreEqual(obj.ObjectUrl, urls[i]);
                        Assert.AreEqual(obj.Key, keys[j]);
                        Assert.AreEqual(obj.Value, i == 0 ? values1[j] : values2[j]);
                    }
                }
                // Fetch by url and key
                for (int j = 0; j < 4; ++j)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        IObjectMetadata obj = provider.Fetch(urls[i], keys[j]);
                        Assert.NotNull(obj);
                        Assert.AreEqual(obj.ObjectUrl, urls[i]);
                        Assert.AreEqual(obj.Key, keys[j]);
                        Assert.AreEqual(obj.Value, i == 0 ? values1[j] : values2[j]);
                    }
                }
                provider.Close();
            }
        }

        [Test]
        public void TestDelete()
        {
            const string DBPath = "test3.db";
            if (File.Exists(DBPath))
                File.Delete(DBPath);

            using (var provider = new QueryMetadataProvider())
            {
                // Create database and fill it
                provider.Create(DBPath);

                foreach (MetadataKey key in keys)
                    provider.AddKey(key);

                for (int i = 0; i < 4; ++i)
                {
                    provider.Write(new ObjectMetadata(urls[0], keys[i], values1[i]));
                    provider.Write(new ObjectMetadata(urls[1], keys[i], values2[i]));
                }

                for (int i = 0; i < 4; ++i)
                {
                    bool result = provider.Delete(new ObjectMetadata(urls[0], keys[i], values1[i]));
                    Assert.True(result);
                    if (i != 3)
                    {
                        IObjectMetadata data = provider.Fetch(urls[1], keys[i]);
                        result = provider.Delete(data);
                        Assert.True(result);
                    }
                }

                IEnumerable<IObjectMetadata> metadata = provider.FetchAll();
                Assert.NotNull(metadata);
                Assert.AreEqual(metadata.Count(), 1);
                IObjectMetadata fetchedData = metadata.First();
                Assert.NotNull(fetchedData);
                Assert.AreEqual(fetchedData.Key, keys[3]);
                Assert.AreEqual(fetchedData.Value, values2[3]);
            }

            using (var provider = new QueryMetadataProvider())
            {
                provider.Open(DBPath, false);

                IObjectMetadata data = new ObjectMetadata(urls[1], keys[3]);
                bool result = provider.Delete(data);
                Assert.True(result);
                IEnumerable<IObjectMetadata> metadata = provider.FetchAll();
                Assert.NotNull(metadata);
                Assert.AreEqual(metadata.Count(), 0);
            }
        }

        [Test]
        public void TestInvalidOperations()
        {
            // TODO/Benlitz
        }

        [Test]
        public void TestBuilder()
        {
            Utils.CleanContext();
            if (File.Exists(QueryMetadataProvider.DefaultDatabaseFilename))
                File.Delete(QueryMetadataProvider.DefaultDatabaseFilename);

            using (var provider = new QueryMetadataProvider())
            {
                const string TestStringKeyName = "TestString";
                const string TestFloatKeyName = "TestFloat";
                var testStringKey = new MetadataKey(TestStringKeyName, MetadataKey.DatabaseType.String);
                var testFloatKey = new MetadataKey(TestFloatKeyName, MetadataKey.DatabaseType.Float);

                // Create database
                bool result = provider.Create(QueryMetadataProvider.DefaultDatabaseFilename);
                Assert.True(result);

                // Add a key and retrieve it
                provider.AddKey(testStringKey);
                provider.AddKey(testFloatKey);

                provider.Write(new ObjectMetadata("/assets/${PathVariable}/url1", testStringKey, "Test1"));
                provider.Write(new ObjectMetadata("/assets/${PathVariable}/url2", testStringKey, "Test2"));
                provider.Write(new ObjectMetadata("/assets/${PathVariable}/url3", testFloatKey, 42.69f));
            }

            var builder = Utils.CreateBuilder();
            var commands = new List<Command>();
            builder.MetadataDatabaseDirectory = "./";
            builder.InitialVariables.Add("PathVariable".ToUpperInvariant(), "path/via/variable");
            commands.Add(new EchoCommand("/assets/${PathVariable}/url1", "Metadata TestString: (${Metadata:TestString})"));
            commands.Add(new EchoCommand("/assets/${PathVariable}/url2", "Metadata TestString: (${Metadata:TestString})"));
            commands.Add(new EchoCommand("/assets/${PathVariable}/url3", "Metadata TestFloat: (${Metadata:TestFloat})"));
            commands.Add(new EchoCommand("/assets/${PathVariable}/url4", "Non existing metadata: (${Metadata:NonExisting})"));

            builder.Root.Add(commands);
            builder.Run(Builder.Mode.Build);

            Assert.That(((EchoCommand)commands[0]).Echo, Is.EqualTo("Metadata TestString: (Test1)"));
            Assert.That(((EchoCommand)commands[1]).Echo, Is.EqualTo("Metadata TestString: (Test2)"));
            Assert.That(((EchoCommand)commands[2]).Echo, Is.EqualTo("Metadata TestFloat: (" + 42.69f + ")"));
            Assert.That(((EchoCommand)commands[3]).Echo, Is.EqualTo("Non existing metadata: ()"));
        }
        // ReSharper restore PossibleMultipleEnumeration
    }
}
