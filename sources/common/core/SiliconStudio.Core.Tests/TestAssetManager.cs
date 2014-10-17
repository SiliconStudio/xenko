// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Core.Serialization.Serializers;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Tests
{
    [TestFixture]
    public class TestAssetManager
    {
        [ContentSerializer(typeof(DataContentConverterSerializer<A>))]
        public class A
        {
            public int I;
        }

        [ContentSerializer(typeof(DataContentConverterSerializer<B>))]
        public class B
        {
            public A A;
        }

        [ContentSerializer(typeof(DataContentSerializer<C>))]
        [DataContract]
        public class C : ComponentBase, IContentData
        {
            public int I { get; set; }

            public ContentReference<C> Child { get; set; }

            public ContentReference<D> Child2 { get; set; }

            public string Url { get; set; }
        }

        [ContentSerializer(typeof(D.Serializer))]
        public class D : ComponentBase
        {
            public D(int a)
            {
            }

            class Serializer : ContentSerializerBase<D>
            {
                public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref D obj)
                {
                    if (context.Mode == ArchiveMode.Deserialize)
                    {
                        obj = new D(12);
                    }
                }
            }
        }

        [ContentSerializer(typeof(DataContentSerializer<AData>))]
        [DataContract]
        public class AData
        {
            public int I;
        }

        [ContentSerializer(typeof(DataContentSerializer<BData>))]
        [DataContract]
        public class BData
        {
            public ContentReference<AData> A;
        }

        public class ADataConverter : DataConverter<AData, A>
        {
            public override void ConvertFromData(ConverterContext converterContext, AData data, ref A obj)
            {
                obj = new A { I = data.I };
            }

            public override void ConvertToData(ConverterContext converterContext, ref AData data, A obj)
            {
                data = new AData { I = obj.I };
            }
        }

        public class BDataConverter : DataConverter<BData, B>
        {
            public override void ConvertFromData(ConverterContext converterContext, BData data, ref B obj)
            {
                obj = new B();
                converterContext.ConvertFromData(data.A, ref obj.A);
            }

            public override void ConvertToData(ConverterContext converterContext, ref BData data, B obj)
            {
                data = new BData();
                converterContext.ConvertToData(ref data.A, obj.A);
            }
        }

        [TestFixtureSetUp]
        public void SetupDatabase()
        {
            VirtualFileSystem.CreateDirectory("/data");
            VirtualFileSystem.CreateDirectory("/data/db");
            var databaseFileProvider = new DatabaseFileProvider(AssetIndexMap.NewTool(), new ObjectDatabase("/data/db"));
            AssetManager.GetFileProvider = () => databaseFileProvider;
        }

        [Test]
        public void Simple()
        {
            var a1 = new A { I = 18 };

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("test", a1);

            // Use same asset manager
            var a2 = assetManager1.Load<A>("test");

            Assert.That(a2, Is.EqualTo(a1));

            // Use new asset manager
            var a3 = assetManager2.Load<A>("test");

            Assert.That(a3, Is.Not.EqualTo(a1));
            Assert.That(a3.I, Is.EqualTo(a1.I));
        }

        [Test]
        public void SimpleWithContentReference()
        {
            var b1 = new B();
            b1.A = new A { I = 18 };

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("test", b1);

            // Use new asset manager
            var b2 = assetManager2.Load<B>("test");

            Assert.That(b2, Is.Not.EqualTo(b1));
            Assert.That(b2.A.I, Is.EqualTo(b1.A.I));
        }

        [Test]
        public void SimpleWithContentReferenceShared()
        {
            var b1 = new B();
            b1.A = new A { I = 18 };
            var b2 = new B { A = b1.A };

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("b1", b1);
            assetManager1.Save("b2", b2);

            // Use new asset manager
            var b1Loaded = assetManager2.Load<B>("b1");
            var b2Loaded = assetManager2.Load<B>("b2");

            Assert.That(b2Loaded, Is.Not.EqualTo(b1Loaded));
            Assert.That(b2Loaded.A, Is.EqualTo(b1Loaded.A));
        }

        [Test]
        public void SimpleLoadData()
        {
            var b1 = new B();
            b1.A = new A { I = 18 };

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();
            var assetManager3 = new AssetManager();

            assetManager1.Save("test", b1);

            // Use new asset manager
            var b2 = assetManager2.Load<BData>("test");

            Assert.That(b2, Is.Not.EqualTo(b1));
            Assert.That(b2.A.Value.I, Is.EqualTo(b1.A.I));

            // Try to load without references
            var b3 = assetManager3.Load<BData>("test", new AssetManagerLoaderSettings { LoadContentReferences = false });

            Assert.That(b3, Is.Not.EqualTo(b1));
            Assert.That(b3.A.Value, Is.Null);
            Assert.That(b3.A.Location, Is.Not.Null);
        }

        [Test]
        public void SimpleSaveData()
        {
            var b1 = new BData();
            b1.A = new AData { I = 18 };

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("test", b1);

            Assert.That(b1.A.Location, Is.Not.Null);

            var b2 = new BData();
            b2.A = new ContentReference<AData>(Guid.Empty, b1.A.Location);
            assetManager1.Save("test2", b2);

            var b3 = assetManager2.Load<BData>("test2");
            Assert.That(b3.A.Value.I, Is.EqualTo(b1.A.Value.I));
        }

        [Test]
        public void LifetimeShared()
        {
            var c1 = new C { I = 16 };
            var c2 = new C { I = 18 };
            c1.Child = new C { I = 32 };
            c2.Child = c1.Child;

            c1.Child.Location = "cchild";

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("c1", c1);
            assetManager1.Save("c2", c2);

            var c1Copy = assetManager2.Load<C>("c1");
            var c2Copy = assetManager2.Load<C>("c2");
            var c1ChildCopy = assetManager2.Load<C>("cchild");

            assetManager2.Unload(c1Copy);

            // Check that everything is properly unloaded
            Assert.That(((IReferencable)c1Copy).ReferenceCount, Is.EqualTo(0));
            Assert.That(((IReferencable)c2Copy).ReferenceCount, Is.EqualTo(1));
            Assert.That(((IReferencable)c1ChildCopy).ReferenceCount, Is.EqualTo(1));

            assetManager2.Unload(c2Copy);

            // Check that everything is properly unloaded
            Assert.That(((IReferencable)c2Copy).ReferenceCount, Is.EqualTo(0));
            Assert.That(((IReferencable)c1ChildCopy).ReferenceCount, Is.EqualTo(1));

            assetManager2.Unload(c1ChildCopy);

            // Check that everything is properly unloaded
            Assert.That(((IReferencable)c1ChildCopy).ReferenceCount, Is.EqualTo(0));
        }

        [Test]
        public void LifetimeNoSimpleConstructor()
        {
            var c1 = new C { I = 18 };
            c1.Child2 = new D(18);

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("c1", c1);

            var c1Copy = assetManager2.Load<C>("c1");
            Assert.That(((IReferencable)c1Copy).ReferenceCount, Is.EqualTo(1));
            Assert.That(((IReferencable)c1Copy.Child2.Value).ReferenceCount, Is.EqualTo(1));

            assetManager2.Unload(c1Copy);
            Assert.That(((IReferencable)c1Copy).ReferenceCount, Is.EqualTo(0));
            Assert.That(((IReferencable)c1Copy.Child2.Value).ReferenceCount, Is.EqualTo(0));
        }

        [Test]
        public void LifetimeCycles()
        {
            var c1 = new C { I = 18 };
            var c2 = new C { I = 20 };
            c1.Child = c2;
            c2.Child = c1;

            var assetManager1 = new AssetManager();
            var assetManager2 = new AssetManager();

            assetManager1.Save("c1", c1);

            var c1Copy = assetManager2.Load<C>("c1");
            Assert.That(((IReferencable)c1Copy).ReferenceCount, Is.EqualTo(1));
            Assert.That(((IReferencable)c1Copy.Child.Value).ReferenceCount, Is.EqualTo(1));

            assetManager2.Unload(c1Copy);
            Assert.That(((IReferencable)c1Copy).ReferenceCount, Is.EqualTo(0));
            Assert.That(((IReferencable)c1Copy.Child.Value).ReferenceCount, Is.EqualTo(0));
        }

        [ModuleInitializer]
        internal static void Initialize()
        {
            // Register ADataConverter
            ConverterContext.RegisterConverter(new ADataConverter());
            ConverterContext.RegisterConverter(new BDataConverter());
        }
    }
}
