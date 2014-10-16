// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Tests
{
    [DataContract]
    public class MyAssetData
    {
        public ContentReference<MyAssetData> Child { get; set; }
    }

    public class MyAssetToVisit : Asset
    {
        public MyAssetToVisit()
        {
            MyReferences = new List<AssetReference<Asset>>();
            MyAssetDatas = new Dictionary<string, ContentReference<MyAssetData>>();
        }

        public AssetReference<Asset> MyReference { get; set; }

        public List<AssetReference<Asset>> MyReferences { get; set; }

        public Dictionary<string, ContentReference<MyAssetData>> MyAssetDatas { get; set; }

        public MyAssetData MyAssetData { get; set; }
    }

    public class MyCustomVisitor : AssetVisitorBase
    {
        public MyCustomVisitor()
        {
            CollectedGuids = new List<Guid>();
        }

        public List<Guid> CollectedGuids { get; set; }

        public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            var reference = obj as IContentReference;
            if (reference != null)
            {
                CollectedGuids.Add(reference.Id);
            }
            base.VisitObject(obj, descriptor, visitMembers);
        }
    }

    [TestFixture]
    public class TestVisitor
    {
        [Test]
        public void TestVisit()
        {
            var instance = new MyAssetToVisit();

            var ref1 = new Guid("13cbb80e-35d5-4c1b-96da-c1e0acade4ea");
            var ref2 = new Guid("86a0dcd6-9bcb-442f-84b0-65866a5f1cbc");
            var ref3 = new Guid("3d0ba228-c001-4dfa-859c-04179e5cc2c0");
            var ref4 = new Guid("432740f7-133f-48a9-a118-e8955b3bdd60");
            var ids = new List<Guid>() { ref1, ref2, ref3, ref4 };
            ids.Sort();

            instance.MyReference = new AssetReference<Asset>(ref1, "test");
            instance.MyReferences.Add(new AssetReference<Asset>(ref2, "test2"));
            instance.MyAssetDatas.Add("key" , new ContentReference<MyAssetData>(ref3, "test3"));
            instance.MyAssetData = new MyAssetData { Child = new ContentReference<MyAssetData>(ref4, "test4") };

            var visitor = new MyCustomVisitor();
            visitor.Visit(instance);

            visitor.CollectedGuids.Sort();
            Assert.AreEqual(4, visitor.CollectedGuids.Count);
            Assert.AreEqual(ids, visitor.CollectedGuids);
        }
    }
}
