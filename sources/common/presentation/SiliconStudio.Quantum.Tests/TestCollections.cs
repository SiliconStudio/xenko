using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests
{
    /// <summary>
    /// This fixture tests advanced scenario that uses collections.
    /// </summary>
    [TestFixture]
    public class TestCollections
    {
        public class SimpleObject
        {
            public string Name;
            public override string ToString() => $"{{SimpleObject: {Name}}}";
        }

        public class ListContainer
        {
            public List<SimpleObject> List { get; set; } = new List<SimpleObject>();
            public List<object> ObjectList { get; set; } = new List<object>();

            public object ObjectMember { get; set; }
        }

        [Test]
        public void TestOverwriteCollection()
        {
            var nodeContainer = new NodeContainer();
            var container = new ListContainer { List = { new SimpleObject(), new SimpleObject() } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var oldList = container.List;
            var oldItem1Node = containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(0));
            var oldItem2Node = containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(1));
            containerNode[nameof(ListContainer.List)].Update(new List<SimpleObject> { new SimpleObject(), new SimpleObject() });
            Assert.AreNotEqual(oldList, containerNode[nameof(ListContainer.List)].Retrieve());
            Assert.AreNotEqual(oldItem1Node, containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(0)));
            Assert.AreNotEqual(oldItem1Node.Retrieve(), containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(0)).Retrieve());
            Assert.AreNotEqual(oldItem2Node, containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(1)));
            Assert.AreNotEqual(oldItem2Node.Retrieve(), containerNode[nameof(ListContainer.List)].IndexedTarget(new Index(1)).Retrieve());
        }

        [Test]
        public void TestMultipleTimesSameObject()
        {
            var items = new[] { new SimpleObject(), new SimpleObject() };
            var nodeContainer = new NodeContainer();
            var container = new ListContainer { List = { items[0], items[1], items[0], items[1] } };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            var memberNode = containerNode[nameof(ListContainer.List)];
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);
            VerifyItem(memberNode, 2, items[0], false);
            VerifyItem(memberNode, 3, items[1], false);

            memberNode.Add(items[0]);
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);
            VerifyItem(memberNode, 2, items[0], false);
            VerifyItem(memberNode, 3, items[1], false);
            VerifyItem(memberNode, 4, items[0], false);

            memberNode.Update(items[1], new Index(2));
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);
            VerifyItem(memberNode, 2, items[1], false);
            VerifyItem(memberNode, 3, items[1], false);
            VerifyItem(memberNode, 4, items[0], false);
        }

        [Test]
        public void TestObjectListWithSimpleObjects()
        {
            var items = new[] { new SimpleObject(), new SimpleObject(), new SimpleObject(), new SimpleObject() };
            var obj = new ListContainer { ObjectList = { items[0], items[1] } };
            var nodeContainer = new NodeContainer();
            var containerNode = nodeContainer.GetOrCreateNode(obj);
            var memberNode = containerNode[nameof(ListContainer.ObjectList)];
            Helper.TestMemberNode(containerNode, memberNode, obj, obj.ObjectList, nameof(ListContainer.ObjectList), true);
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);

            // Add a new item
            memberNode.Add(items[2]);
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);
            VerifyItem(memberNode, 2, items[2], false);

            // Update existing item (with a different type here)
            memberNode.Update(items[3], new Index(2));
            VerifyItem(memberNode, 0, items[0], false);
            VerifyItem(memberNode, 1, items[1], false);
            VerifyItem(memberNode, 2, items[3], false);
        }

        [Test]
        public void TestObjectListWithBoxedPrimitiveValue()
        {
            var obj = new ListContainer { ObjectList = { 1.0f, 2.0f } };
            var nodeContainer = new NodeContainer();

            // Construction
            var containerNode = nodeContainer.GetOrCreateNode(obj);
            var memberNode = containerNode[nameof(ListContainer.ObjectList)];
            Helper.TestMemberNode(containerNode, memberNode, obj, obj.ObjectList, nameof(ListContainer.ObjectList), true);
            VerifyItem(memberNode, 0, 1.0f, true);
            VerifyItem(memberNode, 1, 2.0f, true);

            // Add a new item
            memberNode.Add(3.0f);
            VerifyItem(memberNode, 0, 1.0f, true);
            VerifyItem(memberNode, 1, 2.0f, true);
            VerifyItem(memberNode, 2, 3.0f, true);

            // Update existing item (with a different type here)
            memberNode.Update(4.0, new Index(2));
            VerifyItem(memberNode, 0, 1.0f, true);
            VerifyItem(memberNode, 1, 2.0f, true);
            VerifyItem(memberNode, 2, 4.0, true);
        }

        [Test, Ignore("Update for object members not fully supported yet")]
        public void TestListInObjectMember()
        {
            var nodeContainer = new NodeContainer();
            var values = new object[] { new SimpleObject(), new List<SimpleObject> { new SimpleObject(), new SimpleObject() }, new SimpleObject() };
            var container = new ListContainer { ObjectMember = values[0] };
            var containerNode = nodeContainer.GetOrCreateNode(container);
            Assert.AreEqual(container.ObjectMember, containerNode[nameof(ListContainer.ObjectMember)].Retrieve());
            containerNode[nameof(ListContainer.ObjectMember)].Update(values[1]);
            Assert.AreEqual(values[1], container.ObjectMember);
            Assert.AreEqual(container.ObjectMember, containerNode[nameof(ListContainer.ObjectMember)].Retrieve());
            Assert.AreEqual(true, containerNode[nameof(ListContainer.ObjectMember)].IsReference);
            Assert.NotNull(containerNode[nameof(ListContainer.ObjectMember)].Indices);
            Assert.AreEqual(2, containerNode[nameof(ListContainer.ObjectMember)].Indices.Count());
            Assert.AreEqual(new Index(0), containerNode[nameof(ListContainer.ObjectMember)].Indices.ToList()[0]);
            Assert.AreEqual(new Index(1), containerNode[nameof(ListContainer.ObjectMember)].Indices.ToList()[1]);
            Assert.AreEqual(TypeDescriptorFactory.Default.Find(typeof(object)), containerNode[nameof(ListContainer.ObjectMember)].Descriptor);
            Assert.IsFalse(containerNode[nameof(ListContainer.ObjectMember)].IsPrimitive);
            Assert.AreEqual(((IList)values[1])[0], containerNode[nameof(ListContainer.ObjectMember)].IndexedTarget(new Index(0)));
            Assert.AreEqual(((IList)values[1])[1], containerNode[nameof(ListContainer.ObjectMember)].IndexedTarget(new Index(1)));
            Assert.AreEqual(typeof(object), containerNode[nameof(ListContainer.ObjectMember)].Type);
            // TODO: more things could be checked!
            containerNode[nameof(ListContainer.ObjectMember)].Update(values[2]);
            Assert.AreEqual(values[2], container.ObjectMember);
            Assert.AreEqual(container.ObjectMember, containerNode[nameof(ListContainer.ObjectMember)]);
        }

        private static void VerifyItem(IMemberNode listMemberNode, int index, object expectedValue, bool isPrimitive)
        {
            var enumRef = listMemberNode.ItemReferences;
            var indexValue = new Index(index);

            Assert.NotNull(enumRef);
            Assert.NotNull(listMemberNode.Indices);
            Assert.AreEqual(indexValue, listMemberNode.Indices.ToList()[index]);
            Assert.AreEqual(indexValue, enumRef.Indices.ToList()[index]);
            Assert.AreEqual(indexValue, enumRef.ToList()[index].Index);
            Assert.AreEqual(expectedValue, enumRef.ToList()[index].ObjectValue);
            Assert.NotNull(enumRef.ToList()[index].TargetNode);
            Assert.AreEqual(expectedValue, enumRef.ToList()[index].TargetNode.Retrieve());
            Assert.AreEqual(TypeDescriptorFactory.Default.Find(expectedValue.GetType()), enumRef.ToList()[index].TargetNode.Descriptor);
            Assert.AreEqual(isPrimitive, enumRef.ToList()[index].TargetNode.IsPrimitive);
            Assert.AreEqual(false, enumRef.ToList()[index].TargetNode.IsReference);
            Assert.Null(enumRef.ToList()[index].TargetNode.TargetReference);
            Assert.Null(enumRef.ToList()[index].TargetNode.ItemReferences);
            Assert.AreEqual(expectedValue.GetType(), enumRef.ToList()[index].TargetNode.Type);
            Assert.AreEqual(expectedValue, enumRef.ToList()[index].TargetNode.Value);
        }
    }
}
