// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using NUnit.Framework;

using SiliconStudio.Core;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestConstruction
    {
        #region Test class definitions
        public class SimpleObject
        {
            public SimpleObject()
            {
            }

            public SimpleObject(int firstValue, int secondValue, int thirdValue, int fourthValue)
            {
                FirstValue = firstValue;
                SecondValue = secondValue;
                ThirdValue = thirdValue;
                FourthValue = fourthValue;
                Collection = new List<object>();
                Dictionary = new Dictionary<string, object>();
            }

            [DataMember(0)]
            public SimpleObject SubObject { get; set; }
            
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(7)]
            public int SecondValue { get; set; }

            [DataMember(2)]
            public int ThirdValue { get; set; }

            [DataMember(3)]
            public int? FourthValue { get; set; }

            [DataMemberIgnore]
            public int MemberToIgnore { get; set; }

            [DataMember(4)]
            public string Name { get; set; }

            [DataMember(5)]
            public List<object> Collection { get; set; }

            [DataMember(6)]
            public Dictionary<string, object> Dictionary { get; set; }

        }

        public struct Struct1
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public struct Struct2
        {
            [DataMember(1)]
            public int FirstValue { get; set; }

            [DataMember(2)]
            public int SecondValue { get; set; }
        }

        public class ClassWithStructs
        {
            [DataMember(1)]
            public Struct1 FirstStruct { get; set; }

            [DataMember(2)]
            public Struct2 SecondStruct { get; set; }
        }

        public class ClassWithNullObject
        {
            [DataMember(1)]
            public SimpleObject NullObject { get; set; }
        }

        #endregion Test class definitions

        [Test]
        public void TestNodeConstruction()
        {
            var obj = new SimpleObject(1, 2, 3, 4)
            {
                Name = "Test",
                MemberToIgnore = int.MaxValue,
                SubObject = new SimpleObject(5, 6, 7, 8),
                Collection = 
                {
                    "List Item",
                    22.5,
                    Guid.NewGuid(),
                    new List<string> { "one", "two", "three" },
                    new SimpleObject(9, 10, 11, 12),
                },
                Dictionary =
                {
                    { "Item1", "List Item" },
                    { "Item2", 22.5 },
                    { "Item3", Guid.NewGuid() },
                    { "Item4", new List<string> { "one", "two", "three" } },
                    { "Item5", new SimpleObject(9, 10, 11, 12) },
                },
            };

            var container = new ModelContainer();
            var node = (ModelNode)container.GetOrCreateModelNode(obj, obj.GetType());
            Helper.PrintModelContainerContent(container, node);
            // Run the consistency check to verify construction.
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestConstructionWithNullObject()
        {
            var obj = new ClassWithNullObject();
            var container = new ModelContainer();
            var node = (ModelNode)container.GetOrCreateModelNode(obj, obj.GetType());
            Helper.PrintModelContainerContent(container, node);
            // TODO: Asserts regarding the status of the null value
            // Run the consistency check to verify construction.
            Helper.ConsistencyCheck(container, obj);
        }
    }
}
