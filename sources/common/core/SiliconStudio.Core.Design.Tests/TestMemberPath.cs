// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Design.Tests
{
    /// <summary>
    /// Tests for the <see cref="MemberPath"/> class.
    /// </summary>
    [TestFixture]
    public class TestMemberPath : TestMemberPathBase
    {
        /// <summary>
        /// Initialize the tests.
        /// </summary>
        [OneTimeSetUp]
        public override void Initialize()
        {
            base.Initialize();
            ShadowObject.Enable = true;
        }

        [Test]
        public void TestMyClass()
        {
            var testClass = new MyClass
            {
                Sub = new MyClass(),
                Maps = { ["XXX"] = new MyClass() }
            };
            testClass.Subs.Add(new MyClass());

            // 1) MyClass.Value = 1
            var memberPath = new MemberPath();
            memberPath.Push(MemberValue);

            object value;
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Value);

            // 2) MyClass.Sub.Value = 1
            memberPath.Clear();
            memberPath.Push(MemberSub);
            memberPath.Push(MemberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Sub.Value);

            // 3) MyClass.Struct.X = 1
            memberPath.Clear();
            memberPath.Push(MemberStruct);
            memberPath.Push(MemberX);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Struct.X);

            // 3) MyClass.Maps["XXX"].Value = 1
            memberPath.Clear();
            memberPath.Push(MemberMaps);
            memberPath.Push(MapClassDesc, "XXX");
            memberPath.Push(MemberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Maps["XXX"].Value);

            // 4) MyClass.Subs[0].Value = 1
            memberPath.Clear();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 0);
            memberPath.Push(MemberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Subs[0].Value);

            // 5) MyClass.Subs[0].X (invalid)
            memberPath.Clear();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 0);
            memberPath.Push(MemberX);

            Assert.IsFalse(memberPath.TryGetValue(testClass, out value));
            Assert.IsFalse(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));

            // 6) Remove key MyClass.Maps.Remove("XXX")
            memberPath.Clear();
            memberPath.Push(MemberMaps);
            memberPath.Push(MapClassDesc, "XXX");
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.DictionaryRemove, null));
            Assert.IsFalse(testClass.Maps.ContainsKey("XXX"));

            // 7) Re-add a value to the dictionary
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, new MyClass()));
            Assert.IsTrue(testClass.Maps.ContainsKey("XXX"));

            // 8) Remove key MyClass.Subs.Remove(0)
            memberPath.Clear();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 0);
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.CollectionRemove, null));
            Assert.AreEqual(0, testClass.Subs.Count);

            // 9) Add a key MyClass.Subs.Add(new MyClass())
            memberPath.Clear();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 0);
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.CollectionAdd, new MyClass()));
            Assert.AreEqual(1, testClass.Subs.Count);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.Resolve"/> method.
        /// Case: simple path
        /// </summary>
        [Test]
        public void TestResolveSimple()
        {
            var memberPath = new MemberPath();
            memberPath.Push(MemberSub);
            memberPath.Push(MemberStruct);
            memberPath.Push(MemberClass);
            memberPath.Push(MemberMaps);
            memberPath.Push(MapClassDesc, "toto");
            memberPath.Push(MemberValue);

            var reference = new MyClass { Sub = new MyClass { Struct = new MyStruct { Class = new MyClass { Maps = { {"toto", new MyClass { Value = 1 } } } } } } };
            var dual = new MyClass { Sub = new MyClass { Struct = new MyStruct { Class = new MyClass { Maps = { { "toto", new MyClass { Value = 2 } } } } } } };

            var resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(1, resolvedPaths.Count);

            object value;
            Assert.IsTrue(resolvedPaths[0].TryGetValue(dual, out value));
            Assert.AreEqual(dual.Sub.Struct.Class.Maps["toto"].Value, value);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.Resolve"/> method.
        /// Case: broken path
        /// </summary>
        [Test]
        public void TestResolveBroken()
        {
            var memberPath = new MemberPath();
            memberPath.Push(MemberSub);
            memberPath.Push(MemberStruct);
            memberPath.Push(MemberClass);
            memberPath.Push(MemberValue);

            var reference = new MyClass { Sub = new MyClass { Struct = new MyStruct { Class = new MyClass { Value = 1 } } } };
            var dual = new MyClass { Sub = new MyClass { Struct = new MyStruct { Class = null } } };

            var resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(0, resolvedPaths.Count);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.Resolve"/> method.
        /// Case: broken collection path
        /// </summary>
        [Test]
        public void TestResolveCollectionBroken()
        {
            var memberPath = new MemberPath();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 0);

            var referenceElt = new MyClass();
            IdentifiableHelper.SetId(referenceElt, Guid.NewGuid());

            var reference = new MyClass { Subs = { referenceElt } };
            var dual = new MyClass();
            dual.Subs.Add(new MyClass());
            dual.Subs.Add(new MyClass());

            var resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(0, resolvedPaths.Count);

            memberPath = new MemberPath();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 1);
            
            resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(0, resolvedPaths.Count);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.Resolve"/> method.
        /// Case: complex path
        /// </summary>
        [Test]
        public void TestResolveComplex()
        {
            var memberPath = new MemberPath();
            memberPath.Push(MemberSubs);
            memberPath.Push(ListClassDesc, 1);
            memberPath.Push(MemberSub);
            memberPath.Push(MemberValue);

            var id = Guid.NewGuid();
            var referenceElt = new MyClass { Sub = new MyClass { Value = 1 } };
            var dualElt1 = new MyClass { Sub = new MyClass { Value = 2 } };
            var dualElt2 = new MyClass { Sub = new MyClass { Value = 3 } };
            var brokenDual = new MyClass();
            IdentifiableHelper.SetId(referenceElt, id);
            IdentifiableHelper.SetId(dualElt1, id);
            IdentifiableHelper.SetId(dualElt2, id);
            IdentifiableHelper.SetId(brokenDual, id);

            var reference = new MyClass { Subs = { new MyClass(), referenceElt } };
            var dual = new MyClass { Subs = { new MyClass(), new MyClass(), dualElt1, brokenDual, new MyClass(), dualElt2 } };

            var resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(2, resolvedPaths.Count);

            object value;
            Assert.IsTrue(resolvedPaths[0].TryGetValue(dual, out value));
            Assert.AreEqual(dual.Subs[2].Sub.Value, value);

            Assert.IsTrue(resolvedPaths[1].TryGetValue(dual, out value));
            Assert.AreEqual(dual.Subs[5].Sub.Value, value);
        }
    }
}
