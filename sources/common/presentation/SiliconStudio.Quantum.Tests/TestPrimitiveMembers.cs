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
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(IntMember.Member), false);

            // Update from object
            obj.Member = 6;
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(IntMember.Member), false);

            // Update from Quantum
            containerNode.Children.First().Content.Update(7);
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(IntMember.Member), false);
        }

        [Test]
        public void TestStringMember()
        {
            var obj = new StringMember { Member = "aaa" };
            var container = new NodeContainer();

            // Construction
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(StringMember.Member), false);

            // Update from object
            obj.Member = "bbb";
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(StringMember.Member), false);

            // Update from Quantum
            containerNode.Children.First().Content.Update("ccc");
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(StringMember.Member), false);
        }

        [Test]
        public void TestGuidMember()
        {
            var obj = new GuidMember { Member = Guid.NewGuid() };
            var container = new NodeContainer();

            // Construction
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(GuidMember.Member), false);

            // Update from object
            obj.Member = Guid.NewGuid();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(EnumMember.Member), false);

            // Update from Quantum
            containerNode.Children.First().Content.Update(Guid.NewGuid());
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(EnumMember.Member), false);
        }

        [Test]
        public void TestEnumMember()
        {
            var obj = new EnumMember { Member = TestEnum.Value1 };
            var container = new NodeContainer();

            // Construction
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(EnumMember.Member), false);

            // Update from object
            obj.Member = TestEnum.Value2;
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(EnumMember.Member), false);

            // Update from Quantum
            containerNode.Children.First().Content.Update(TestEnum.Value3);
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(EnumMember.Member), false);
        }

        [Test]
        public void TestRegisteredPrimitiveClassMember()
        {
            var obj = new RegisteredPrimitiveClassMember { Member = new PrimitiveClass { Value = 1 } };
            var container = new NodeContainer();
            container.NodeBuilder.RegisterPrimitiveType(typeof(PrimitiveClass));

            // Construction
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveClassMember.Member), false);

            // Update from object
            obj.Member = new PrimitiveClass { Value = 2 };
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveClassMember.Member), false);

            // Update from Quantum
            containerNode.Children.First().Content.Update(new PrimitiveClass { Value = 3 });
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveClassMember.Member), false);
        }

        [Test]
        public void TestRegisteredPrimitiveStructMember()
        {
            var obj = new RegisteredPrimitiveStructMember { Member = new PrimitiveStruct { Value = 1 } };
            var container = new NodeContainer();
            container.NodeBuilder.RegisterPrimitiveType(typeof(PrimitiveStruct));

            // Construction
            var containerNode = (GraphNode)container.GetOrCreateNode(obj);
            Helper.TestNonCollectionObjectContentNode(containerNode, obj, 1);
            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveStructMember.Member), false);

            // Update from object
            obj.Member = new PrimitiveStruct { Value = 2 };
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveClassMember.Member), false);

            // Update from Quantum
            containerNode.Children.Last().Content.Update(new PrimitiveStruct { Value = 3 });
            Helper.TestMemberContentNode(containerNode, memberNode, obj, obj.Member, nameof(RegisteredPrimitiveClassMember.Member), false);
        }
    }
}
