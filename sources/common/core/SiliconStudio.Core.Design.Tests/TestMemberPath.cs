// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Design.Tests
{
    /// <summary>
    /// Tests for the <see cref="MemberPath"/> class.
    /// </summary>
    [TestFixture]
    public class TestMemberPath
    {
        private IMemberDescriptor memberValue;
        private IMemberDescriptor memberSub;
        private IMemberDescriptor memberStruct;
        private IMemberDescriptor memberSubs;
        private IMemberDescriptor memberMaps;
        private IMemberDescriptor memberX;
        private IMemberDescriptor memberClass;

        private CollectionDescriptor listClassDesc;
        private DictionaryDescriptor mapClassDesc;

        public struct MyStruct
        {
            public int X { get; set; }

            public MyClass Class { get; set; }
        }

        public class MyClass
        {
            public MyClass()
            {
                Subs = new List<MyClass>();
                Maps = new Dictionary<string, MyClass>();
            }

            public int Value { get; set; }

            public MyClass Sub { get; set; }

            public MyStruct Struct { get; set; }

            public List<MyClass> Subs { get; set; }

            public Dictionary<string, MyClass> Maps { get; set; }
        }

        /// <summary>
        /// Initialize the tests.
        /// </summary>
        [TestFixtureSetUp]
        public void Initializer()
        {
            var typeFactory = new TypeDescriptorFactory();
            var myClassDesc = typeFactory.Find(typeof(MyClass));
            var myStructDesc = typeFactory.Find(typeof(MyStruct));
            listClassDesc = (CollectionDescriptor)typeFactory.Find(typeof(List<MyClass>));
            mapClassDesc = (DictionaryDescriptor)typeFactory.Find(typeof(Dictionary<string, MyClass>));

            memberValue = myClassDesc.Members.FirstOrDefault(member => member.Name == "Value");
            memberSub = myClassDesc.Members.FirstOrDefault(member => member.Name == "Sub");
            memberStruct = myClassDesc.Members.FirstOrDefault(member => member.Name == "Struct");
            memberSubs = myClassDesc.Members.FirstOrDefault(member => member.Name == "Subs");
            memberMaps = myClassDesc.Members.FirstOrDefault(member => member.Name == "Maps");
            memberX = myStructDesc.Members.FirstOrDefault(member => member.Name == "X");
            memberClass = myStructDesc.Members.FirstOrDefault(member => member.Name == "Class");
        }

        [Test]
        public void TestMyClass()
        {
            var testClass = new MyClass { Sub = new MyClass() };
            testClass.Maps["XXX"] = new MyClass();
            testClass.Subs.Add(new MyClass());

            // 1) MyClass.Value = 1
            var memberPath = new MemberPath();
            memberPath.Push(memberValue);

            object value;
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Value);

            // 2) MyClass.Sub.Value = 1
            memberPath.Clear();
            memberPath.Push(memberSub);
            memberPath.Push(memberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Sub.Value);

            // 3) MyClass.Struct.X = 1
            memberPath.Clear();
            memberPath.Push(memberStruct);
            memberPath.Push(memberX);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Struct.X);

            // 3) MyClass.Maps["XXX"].Value = 1
            memberPath.Clear();
            memberPath.Push(memberMaps);
            memberPath.Push(mapClassDesc, "XXX");
            memberPath.Push(memberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Maps["XXX"].Value);

            // 4) MyClass.Subs[0].Value = 1
            memberPath.Clear();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 0);
            memberPath.Push(memberValue);

            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));
            Assert.IsTrue(memberPath.TryGetValue(testClass, out value));
            Assert.AreEqual(1, value);
            Assert.AreEqual(1, testClass.Subs[0].Value);

            // 5) MyClass.Subs[0].X (invalid)
            memberPath.Clear();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 0);
            memberPath.Push(memberX);

            Assert.IsFalse(memberPath.TryGetValue(testClass, out value));
            Assert.IsFalse(memberPath.Apply(testClass, MemberPathAction.ValueSet, 1));

            // 6) Remove key MyClass.Maps.Remove("XXX")
            memberPath.Clear();
            memberPath.Push(memberMaps);
            memberPath.Push(mapClassDesc, "XXX");
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.DictionaryRemove, null));
            Assert.IsFalse(testClass.Maps.ContainsKey("XXX"));

            // 7) Re-add a value to the dictionary
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.ValueSet, new MyClass()));
            Assert.IsTrue(testClass.Maps.ContainsKey("XXX"));

            // 8) Remove key MyClass.Subs.Remove(0)
            memberPath.Clear();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 0);
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.CollectionRemove, null));
            Assert.AreEqual(0, testClass.Subs.Count);

            // 9) Add a key MyClass.Subs.Add(new MyClass())
            memberPath.Clear();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 0);
            Assert.IsTrue(memberPath.Apply(testClass, MemberPathAction.CollectionAdd, new MyClass()));
            Assert.AreEqual(1, testClass.Subs.Count);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.GetNodes"/> function.
        /// </summary>
        [Test]
        public void TestGetNodes()
        {
            var memberPath = new MemberPath();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 2);
            memberPath.Push(memberMaps);
            memberPath.Push(mapClassDesc, "toto");
            memberPath.Push(memberStruct);
            memberPath.Push(memberClass);
            memberPath.Push(memberValue);

            var obj = new MyClass();
            obj.Subs.Add(new MyClass());
            obj.Subs.Add(new MyClass());
            obj.Subs.Add(new MyClass());
            obj.Subs[2].Maps["toto"] = new MyClass();
            obj.Subs[2].Maps["toto"].Struct = new MyStruct { Class = new MyClass() };

            var nodes = memberPath.GetNodes(obj).ToList();

            Assert.AreEqual(7, nodes.Count);

            Assert.AreEqual(obj, nodes[0].Object);
            Assert.AreEqual(obj.Subs, nodes[1].Object);
            Assert.AreEqual(obj.Subs[2], nodes[2].Object);
            Assert.AreEqual(obj.Subs[2].Maps, nodes[3].Object);
            Assert.AreEqual(obj.Subs[2].Maps["toto"], nodes[4].Object);
            Assert.AreEqual(obj.Subs[2].Maps["toto"].Struct, nodes[5].Object);
            Assert.AreEqual(obj.Subs[2].Maps["toto"].Struct.Class, nodes[6].Object);

            Assert.AreEqual(memberSubs, nodes[0].Descriptor);
            Assert.AreEqual(null, nodes[1].Descriptor);
            Assert.AreEqual(memberMaps, nodes[2].Descriptor);
            Assert.AreEqual(null, nodes[3].Descriptor);
            Assert.AreEqual(memberStruct, nodes[4].Descriptor);
            Assert.AreEqual(memberClass, nodes[5].Descriptor);
            Assert.AreEqual(memberValue, nodes[6].Descriptor);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPathExtensions.GetNodeOverrides"/> function.
        /// </summary>
        [Test]
        public void TestGetNodeAttributes()
        {
            var memberPath = new MemberPath();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 2);
            memberPath.Push(memberMaps);
            memberPath.Push(mapClassDesc, "toto");
            memberPath.Push(memberStruct);
            memberPath.Push(memberClass);
            memberPath.Push(memberValue);

            var obj = new MyClass();
            obj.Subs.Add(new MyClass());
            obj.Subs.Add(new MyClass());
            obj.Subs.Add(new MyClass());
            obj.Subs[2].Maps["toto"] = new MyClass();
            obj.Subs[2].Maps["toto"].Struct = new MyStruct { Class = new MyClass() };

            obj.SetOverride(memberSubs, OverrideType.New | OverrideType.Sealed);
            obj.Subs[2].SetOverride(memberMaps, OverrideType.New);
            obj.Subs[2].Maps["toto"].SetOverride(memberStruct, OverrideType.Sealed);
            obj.Subs[2].Maps["toto"].Struct.SetOverride(memberClass, OverrideType.New);
            obj.Subs[2].Maps["toto"].Struct.Class.SetOverride(memberValue, OverrideType.Sealed);

            var overrides = memberPath.GetNodeOverrides(obj).ToList();
            Assert.AreEqual(4, overrides.Count);
            Assert.AreEqual(OverrideType.New | OverrideType.Sealed, overrides[0]);
            Assert.AreEqual(OverrideType.New, overrides[1]);
            Assert.AreEqual(OverrideType.Sealed, overrides[2]);
            Assert.AreEqual(OverrideType.Sealed, overrides[3]);
        }

        /// <summary>
        /// Tests for the <see cref="MemberPath.Resolve"/> method.
        /// Case: simple path
        /// </summary>
        [Test]
        public void TestResolveSimple()
        {
            var memberPath = new MemberPath();
            memberPath.Push(memberSub);
            memberPath.Push(memberStruct);
            memberPath.Push(memberClass);
            memberPath.Push(memberMaps);
            memberPath.Push(mapClassDesc, "toto");
            memberPath.Push(memberValue);

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
            memberPath.Push(memberSub);
            memberPath.Push(memberStruct);
            memberPath.Push(memberClass);
            memberPath.Push(memberValue);

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
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 0);

            var referenceElt = new MyClass();
            referenceElt.SetId(Guid.NewGuid());

            var reference = new MyClass { Subs = { referenceElt } };
            var dual = new MyClass();
            dual.Subs.Add(new MyClass());
            dual.Subs.Add(new MyClass());

            var resolvedPaths = memberPath.Resolve(reference, dual).ToList();
            Assert.AreEqual(0, resolvedPaths.Count);

            memberPath = new MemberPath();
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 1);
            
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
            memberPath.Push(memberSubs);
            memberPath.Push(listClassDesc, 1);
            memberPath.Push(memberSub);
            memberPath.Push(memberValue);

            var id = Guid.NewGuid();
            var referenceElt = new MyClass { Sub = new MyClass { Value = 1 } };
            var dualElt1 = new MyClass { Sub = new MyClass { Value = 2 } };
            var dualElt2 = new MyClass { Sub = new MyClass { Value = 3 } };
            var brokenDual = new MyClass();
            referenceElt.SetId(id);
            dualElt1.SetId(id);
            dualElt2.SetId(id);
            brokenDual.SetId(id);

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