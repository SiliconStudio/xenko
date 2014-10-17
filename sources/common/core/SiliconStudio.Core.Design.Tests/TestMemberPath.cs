// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Tests
{
    /// <summary>
    /// Tests for the <see cref="MemberPath"/> class.
    /// </summary>
    [TestFixture]
    public class TestMemberPath
    {
        public struct MyStruct
        {
            public int X { get; set; }
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

        [Test]
        public void TestMyClass()
        {
            var typeFactory = new TypeDescriptorFactory();
            var myClassDesc = typeFactory.Find(typeof(MyClass));
            var myStructDesc = typeFactory.Find(typeof(MyStruct));
            var listClassDesc = (CollectionDescriptor)typeFactory.Find(typeof(List<MyClass>));
            var mapClassDesc = (DictionaryDescriptor)typeFactory.Find(typeof(Dictionary<string, MyClass>));

            var memberValue = myClassDesc.Members.FirstOrDefault(member => member.Name == "Value");
            var memberSub = myClassDesc.Members.FirstOrDefault(member => member.Name == "Sub");
            var memberStruct = myClassDesc.Members.FirstOrDefault(member => member.Name == "Struct");
            var memberSubs = myClassDesc.Members.FirstOrDefault(member => member.Name == "Subs");
            var memberMaps = myClassDesc.Members.FirstOrDefault(member => member.Name == "Maps");
            var memberX = myStructDesc.Members.FirstOrDefault(member => member.Name == "X");

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
    }
}