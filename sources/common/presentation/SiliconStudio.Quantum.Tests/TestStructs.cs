using System.Linq;
using NUnit.Framework;

namespace SiliconStudio.Quantum.Tests
{
    [TestFixture]
    public class TestStructs
    {
        public struct SimpleStruct
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public override string ToString() => $"{{SimpleStruct: ({Value}), {Name}}}";
        }

        public struct FirstNestingStruct
        {
            public SecondNestingStruct Struct1 { get; set; }
            public override string ToString() => $"{{FirstNestingStruct: {Struct1}}}";
        }

        public struct SecondNestingStruct
        {
            public SimpleStruct Struct2 { get; set; }
            public override string ToString() => $"{{SecondNestingStruct: {Struct2}}}";
        }

        public class StructContainer
        {
            public SimpleStruct Struct { get; set; }
            public override string ToString() => $"{{StructContainer: {Struct}}}";
        }

        public class NestingStructContainer
        {
            public FirstNestingStruct Struct { get; set; }
            public override string ToString() => $"{{NestingStructContainer: {Struct}}}";
        }

        [Test]
        public void TestSimpleStruct()
        {
            var nodeContainer = new NodeContainer();
            var container = new StructContainer { Struct = new SimpleStruct { Name = "Test", Value = 1 } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);

            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 2);
            var structMember1Node = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, structMember1Node, container.Struct, container.Struct.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = memberNode.Children.Last();
            Helper.TestMemberContentNode(memberNode, structMember2Node, container.Struct, container.Struct.Value, nameof(SimpleStruct.Value), false);
        }

        [Test]
        public void TestSimpleStructUpdate()
        {
            var nodeContainer = new NodeContainer();
            var container = new StructContainer { Struct = new SimpleStruct { Name = "Test", Value = 1 } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var memberNode = containerNode.Children.First();
            memberNode.Content.Update(new SimpleStruct { Name = "Test2", Value = 2 });

            Assert.AreEqual("Test2", container.Struct.Name);
            Assert.AreEqual(2, container.Struct.Value);
            var structMember1Node = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, structMember1Node, container.Struct, container.Struct.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = memberNode.Children.Last();
            Helper.TestMemberContentNode(memberNode, structMember2Node, container.Struct, container.Struct.Value, nameof(SimpleStruct.Value), false);
        }

        [Test]
        public void TestSimpleStructMemberUpdate()
        {
            var nodeContainer = new NodeContainer();
            var container = new StructContainer { Struct = new SimpleStruct { Name = "Test", Value = 1 } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);

            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 2);
            var structMember1Node = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, structMember1Node, container.Struct, container.Struct.Name, nameof(SimpleStruct.Name), false);
            structMember1Node.Content.Update("Test2");
            Assert.AreEqual("Test2", container.Struct.Name);
            Helper.TestMemberContentNode(memberNode, structMember1Node, container.Struct, container.Struct.Name, nameof(SimpleStruct.Name), false);
            Helper.TestMemberContentNode(memberNode, memberNode.Children.First(), container.Struct, container.Struct.Name, nameof(SimpleStruct.Name), false);
        }

        [Test]
        public void TestNestedStruct()
        {
            var nodeContainer = new NodeContainer();
            var container = new NestingStructContainer { Struct = new FirstNestingStruct { Struct1 = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test", Value = 1 } } } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);

            var memberNode = containerNode.Children.First();
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 1);
            var firstNestingMemberNode = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, firstNestingMemberNode, container.Struct, container.Struct.Struct1, nameof(FirstNestingStruct.Struct1), false);
            Helper.TestStructContentNode(firstNestingMemberNode, container.Struct.Struct1, 1);
            var secondNestingMemberNode = firstNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(firstNestingMemberNode, secondNestingMemberNode, container.Struct.Struct1, container.Struct.Struct1.Struct2, nameof(SecondNestingStruct.Struct2), false);
            Helper.TestStructContentNode(secondNestingMemberNode, container.Struct.Struct1.Struct2, 2);
            var structMember1Node = secondNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember1Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = secondNestingMemberNode.Children.Last();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember2Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Value, nameof(SimpleStruct.Value), false);
        }

        [Test]
        public void TestFirstNestedStructUpdate()
        {
            var nodeContainer = new NodeContainer();
            var container = new NestingStructContainer { Struct = new FirstNestingStruct { Struct1 = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test", Value = 1 } } } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var memberNode = containerNode.Children.First();

            var newStruct = new FirstNestingStruct { Struct1 = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test2", Value = 2 } } };
            memberNode.Content.Update(newStruct);
            Assert.AreEqual("Test2", container.Struct.Struct1.Struct2.Name);
            Assert.AreEqual(2, container.Struct.Struct1.Struct2.Value);

            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 1);
            var firstNestingMemberNode = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, firstNestingMemberNode, container.Struct, container.Struct.Struct1, nameof(FirstNestingStruct.Struct1), false);
            Helper.TestStructContentNode(firstNestingMemberNode, container.Struct.Struct1, 1);
            var secondNestingMemberNode = firstNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(firstNestingMemberNode, secondNestingMemberNode, container.Struct.Struct1, container.Struct.Struct1.Struct2, nameof(SecondNestingStruct.Struct2), false);
            Helper.TestStructContentNode(secondNestingMemberNode, container.Struct.Struct1.Struct2, 2);
            var structMember1Node = secondNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember1Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = secondNestingMemberNode.Children.Last();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember2Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Value, nameof(SimpleStruct.Value), false);
        }

        [Test]
        public void TestSecondNestedStructUpdate()
        {
            var nodeContainer = new NodeContainer();
            var container = new NestingStructContainer { Struct = new FirstNestingStruct { Struct1 = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test", Value = 1 } } } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var memberNode = containerNode.Children.First();

            var newStruct = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test2", Value = 2 } };
            memberNode.Children.First().Content.Update(newStruct);
            Assert.AreEqual("Test2", container.Struct.Struct1.Struct2.Name);
            Assert.AreEqual(2, container.Struct.Struct1.Struct2.Value);

            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 1);
            var firstNestingMemberNode = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, firstNestingMemberNode, container.Struct, container.Struct.Struct1, nameof(FirstNestingStruct.Struct1), false);
            Helper.TestStructContentNode(firstNestingMemberNode, container.Struct.Struct1, 1);
            var secondNestingMemberNode = firstNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(firstNestingMemberNode, secondNestingMemberNode, container.Struct.Struct1, container.Struct.Struct1.Struct2, nameof(SecondNestingStruct.Struct2), false);
            Helper.TestStructContentNode(secondNestingMemberNode, container.Struct.Struct1.Struct2, 2);
            var structMember1Node = secondNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember1Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = secondNestingMemberNode.Children.Last();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember2Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Value, nameof(SimpleStruct.Value), false);
        }

        [Test]
        public void TestNestedStructMemberUpdate()
        {
            var nodeContainer = new NodeContainer();
            var container = new NestingStructContainer { Struct = new FirstNestingStruct { Struct1 = new SecondNestingStruct { Struct2 = new SimpleStruct { Name = "Test", Value = 1 } } } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var memberNode = containerNode.Children.First();

            memberNode.Children.First().Children.First().Children.First().Content.Update("Test2");
            memberNode.Children.First().Children.First().Children.Last().Content.Update(2);
            Assert.AreEqual("Test2", container.Struct.Struct1.Struct2.Name);
            Assert.AreEqual(2, container.Struct.Struct1.Struct2.Value);

            Helper.TestNonCollectionObjectContentNode(containerNode, container, 1);
            Helper.TestMemberContentNode(containerNode, memberNode, container, container.Struct, nameof(StructContainer.Struct), false);
            Helper.TestStructContentNode(memberNode, container.Struct, 1);
            var firstNestingMemberNode = memberNode.Children.First();
            Helper.TestMemberContentNode(memberNode, firstNestingMemberNode, container.Struct, container.Struct.Struct1, nameof(FirstNestingStruct.Struct1), false);
            Helper.TestStructContentNode(firstNestingMemberNode, container.Struct.Struct1, 1);
            var secondNestingMemberNode = firstNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(firstNestingMemberNode, secondNestingMemberNode, container.Struct.Struct1, container.Struct.Struct1.Struct2, nameof(SecondNestingStruct.Struct2), false);
            Helper.TestStructContentNode(secondNestingMemberNode, container.Struct.Struct1.Struct2, 2);
            var structMember1Node = secondNestingMemberNode.Children.First();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember1Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Name, nameof(SimpleStruct.Name), false);
            var structMember2Node = secondNestingMemberNode.Children.Last();
            Helper.TestMemberContentNode(secondNestingMemberNode, structMember2Node, container.Struct.Struct1.Struct2, container.Struct.Struct1.Struct2.Value, nameof(SimpleStruct.Value), false);
        }
    }
}
