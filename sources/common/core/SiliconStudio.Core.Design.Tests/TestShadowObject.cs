// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;
using System.Collections.Generic;

namespace SiliconStudio.Core.Design.Tests
{
    [TestFixture]
    public class TestShadowObject
    {
        [Test]
        public void TestGetAndGetOrCreate()
        {
            ShadowObject.Enable = true;
            var obj = new object();

            var shadowObject = ShadowObject.Get(obj);
            Assert.Null(shadowObject);

            shadowObject = ShadowObject.GetOrCreate(obj);
            Assert.NotNull(shadowObject);

            shadowObject.IsSelected = true;

            var shadowObject2 = ShadowObject.GetOrCreate(obj);
            Assert.AreEqual(shadowObject, shadowObject2);
        }

        [Test]
        public void TestCopy()
        {
            ShadowObject.Enable = true;
            var obj = new object();
            var obj1 = new object();

            var shadowObject = ShadowObject.GetOrCreate(obj);
            shadowObject.IsSelected = true;

            ShadowObject.Copy(obj, obj1);

            var shadowObject1 = ShadowObject.Get(obj1);
            Assert.NotNull(shadowObject1);

            Assert.True(shadowObject1.IsSelected);
        }

        [Test]
        public void TestIdentifierHelper()
        {
            // Has IdentifierHelper is using ShadowObject, we will test it here
            ShadowObject.Enable = true;
            var obj = new object();

            var id = IdentifiableHelper.GetId(obj);
            Assert.AreNotEqual(Guid.Empty, id);

            var id1 = IdentifiableHelper.GetId(obj);
            Assert.AreEqual(id, id1);

            // We should not get an id for a collection
            var idCollection = IdentifiableHelper.GetId(new List<object>());
            Assert.AreEqual(Guid.Empty, idCollection);

            // We should not get an id for a dictionary
            var idDict = IdentifiableHelper.GetId(new MyDictionary());
            Assert.AreEqual(Guid.Empty, idDict);
        }

        private class MyDictionary : Dictionary<object, object>
        {
        }
    }
}