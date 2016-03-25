using System;
using System.Linq;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestPrimitiveMembers
    {
        public enum TestEnum
        {
            Value1,
            Value2,
            Value3,
        }

        public class IntMember
        {
            public int Member { get; set; }
        }

        public class StringMember
        {
            public string Member { get; set; }
        }

        public class GuidMember
        {
            public Guid Member { get; set; }
        }

        public class EnumMember
        {
            public TestEnum Member { get; set; }
        }

        public class PrimitiveClass
        {
            public int Value { get; set; }
        }

        public struct PrimitiveStruct
        {
            public int Value { get; set; }
        }

        public class RegisteredPrimitiveClassMember
        {
            public PrimitiveClass Member { get; set; }
        }
        public class RegisteredPrimitiveStructMember
        {
            public PrimitiveStruct Member { get; set; }
        }

        [Test]
        public void TestIntMember()
        {
            var obj = new IntMember { Member = 5 };
            var container = new NodeContainer();

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(IntMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = 6;
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.First().Content.Update(7);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestStringMember()
        {
            var obj = new StringMember { Member = "aaa" };
            var container = new NodeContainer();

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(IntMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = "bbb";
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.First().Content.Update("ccc");
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestGuidMember()
        {
            var obj = new GuidMember { Member = Guid.NewGuid() };
            var container = new NodeContainer();

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(IntMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = Guid.NewGuid();
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.First().Content.Update(Guid.NewGuid());
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestEnumMember()
        {
            var obj = new EnumMember { Member = TestEnum.Value1 };
            var container = new NodeContainer();

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(IntMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = TestEnum.Value2;
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.First().Content.Update(TestEnum.Value3);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestRegisteredPrimitiveClassMember()
        {
            var obj = new RegisteredPrimitiveClassMember { Member = new PrimitiveClass { Value = 1 } };
            var container = new NodeContainer();
            container.NodeBuilder.RegisterPrimitiveType(typeof(PrimitiveClass));

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(RegisteredPrimitiveClassMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = new PrimitiveClass { Value = 2 };
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.First().Content.Update(new PrimitiveClass { Value = 3 });
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }

        [Test]
        public void TestRegisteredPrimitiveStructMember()
        {
            var obj = new RegisteredPrimitiveStructMember { Member = new PrimitiveStruct { Value = 1 } };
            var container = new NodeContainer();
            container.NodeBuilder.RegisterPrimitiveType(typeof(PrimitiveStruct));

            // Construction
            var node = (GraphNode)container.GetOrCreateNode(obj);
            Assert.AreEqual(obj, node.Content.Value);
            Assert.AreEqual(1, node.Children.Count);
            Assert.AreEqual(nameof(RegisteredPrimitiveStructMember.Member), node.Children.First().Name);
            Assert.AreEqual(false, node.Children.First().Content.IsReference);
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from object
            obj.Member = new PrimitiveStruct { Value = 2 };
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);

            // Update from Quantum
            node.Children.Last().Content.Update(new PrimitiveStruct { Value = 3 });
            Assert.AreEqual(obj.Member, node.Children.First().Content.Value);
        }
    }
}
