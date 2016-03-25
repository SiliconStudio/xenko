// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests.Obsolete
{
    [TestFixture(Ignore = true)]
    public class ObsoleteTestConstruction
    {
        public class PrimitiveMember
        {
            public int Member { get; set; }
        }

        public class StringMember
        {
            public string Member { get; set; }
        }

        public class ReferenceMember
        {
            public StringMember Member { get; set; }
        }


        [Test]
        public void TestObjectReferenceMember()
        {
            var obj = new ReferenceMember { Member = new StringMember { Member = "a" } };
            var container = new NodeContainer();

            // Construction
            var node = container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(ReferenceMember.Member), node.Children.First().Name);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
            Assert.AreEqual(true, node.Children.First().Content.IsReference);
            Assert.IsInstanceOf<ObjectReference>(node.Children.First().Content.Reference);
            var reference = (ObjectReference)node.Children.First().Content.Reference;
            Assert.AreEqual(obj.Member, reference.ObjectValue);
            Assert.IsNotNull(reference.TargetNode);
            Assert.AreEqual(obj.Member, reference.TargetNode.Content.Value);
            node = reference.TargetNode;
            Assert.AreEqual(obj.Member, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(StringMember.Member), node.Children.First().Name);
            Assert.AreEqual(obj.Member.Member, node.Children.First().Content.Value);
            obj.Member.Member = "b";
            Assert.AreEqual(obj.Member.Member, node.Children.First().Content.Value);
            node.Children.First().Content.Update("c");
            Assert.AreEqual(obj.Member.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestNullReferenceMember()
        {
            var obj = new ReferenceMember { Member = null };

            var container = new NodeContainer();
            var node = container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(ReferenceMember.Member), node.Children.First().Name);
            Assert.AreEqual(null, node.Children.First().Content.Value);
            Assert.AreEqual(true, node.Children.First().Content.IsReference);
            Assert.IsInstanceOf<ObjectReference>(node.Children.First().Content.Reference);
            var reference = (ObjectReference)node.Children.First().Content.Reference;
            Assert.AreEqual(null, reference.ObjectValue);
            Assert.IsNull(reference.TargetNode);

            node.Children.First().Content.Update(new StringMember { Member = "a" });
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(ReferenceMember.Member), node.Children.First().Name);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
            Assert.AreEqual(true, node.Children.First().Content.IsReference);
            Assert.IsInstanceOf<ObjectReference>(node.Children.First().Content.Reference);
            reference = (ObjectReference)node.Children.First().Content.Reference;
            Assert.AreEqual(obj.Member, reference.ObjectValue);
            Assert.IsNotNull(reference.TargetNode);
            Assert.AreEqual(obj.Member, reference.TargetNode.Content.Value);
            node = reference.TargetNode;
            Assert.AreEqual(obj.Member, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(StringMember.Member), node.Children.First().Name);
            Assert.AreEqual(obj.Member.Member, node.Children.First().Content.Value);
        }

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

            var container = new NodeContainer();
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, node);
            // Run the consistency check to verify construction.
            Helper.ConsistencyCheck(container, obj);
        }

        [Test]
        public void TestConstructionWithNullObject()
        {
            var obj = new ClassWithNullObject();
            var container = new NodeContainer();
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Helper.PrintModelContainerContent(container, node);
            // TODO: Asserts regarding the status of the null value
            // Run the consistency check to verify construction.
            Helper.ConsistencyCheck(container, obj);
        }
    }
}
