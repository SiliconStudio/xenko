// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum.Tests
{
    public static class Helper
    {
        public static void TestObjectContentNode(IGraphNode node, object obj, int childCount)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Check that the content is of the expected type.
            Assert.AreEqual(typeof(ObjectContent), node.Content.GetType());
            // Check that the content is properly referencing its node.
            Assert.AreEqual(node, node.Content.OwnerNode);
            // A node with an ObjectContent should have the same name that the type of its content.
            Assert.AreEqual(obj.GetType().Name, node.Name);
            // A node with an ObjectContent should be a root node.
            Assert.IsNull(node.Parent);
            // A node with an ObjectContent should have the related object as value of its content.
            Assert.AreEqual(obj, node.Content.Retrieve());
            // A node with an ObjectContent should not contain a reference.
            Assert.AreEqual(false, node.Content.IsReference);
            // Check that we have the expected number of children.
            Assert.AreEqual(childCount, node.Children.Count);
        }

        public static void TestMemberContentNode(IGraphNode containerNode, IGraphNode memberNode, object container, object member, string memberName, bool isReference)
        {
            if (containerNode == null) throw new ArgumentNullException(nameof(containerNode));
            if (memberNode == null) throw new ArgumentNullException(nameof(memberNode));
            if (container == null) throw new ArgumentNullException(nameof(container));

            // Check that the content is of the expected type.
            Assert.AreEqual(typeof(MemberContent), memberNode.Content.GetType());
            // Check that the content is properly referencing its node.
            Assert.AreEqual(memberNode, memberNode.Content.OwnerNode);
            // A node with a MemberContent should have the same name that the member in the container.
            Assert.AreEqual(memberName, memberNode.Name);
            // A node with a MemberContent should have its container as parent.
            Assert.AreEqual(containerNode, memberNode.Parent);
            // A node with a MemberContent should have the member value as value of its content.
            Assert.AreEqual(member, memberNode.Content.Retrieve());
            // A node with a primitive MemberContent should not contain a reference.
            Assert.AreEqual(isReference, memberNode.Content.IsReference);
        }

        public static void TestNonNullObjectReference(IReference reference, object targetValue)
        {
            var objReference = reference as ObjectReference;

            // Check that the reference is not null.
            Assert.IsNotNull(objReference);
            // Check that the values match.
            Assert.AreEqual(targetValue, objReference.TargetNode.Content.Retrieve());
            // Check that the values match.
            Assert.AreEqual(targetValue, objReference.ObjectValue);
            // Check that the target is an object content node.
            TestObjectContentNode(objReference.TargetNode, targetValue, objReference.TargetNode.Children.Count);
        }

        public static void TestNonNullObjectReference(IReference reference, IGraphNode targetNode, object targetValue)
        {
            if (targetNode == null) throw new ArgumentNullException(nameof(targetNode));
            var objReference = reference as ObjectReference;

            // Check that the reference is not null.
            Assert.IsNotNull(objReference);
            // Check that the Guids match.
            Assert.AreEqual(targetNode.Guid, objReference.TargetGuid);
            // Check that the nodes match.
            Assert.AreEqual(targetNode, objReference.TargetNode);
            // Check that the values match.
            Assert.AreEqual(targetValue, objReference.ObjectValue);
            // Check that the target is an object content node.
            TestObjectContentNode(targetNode, targetValue, targetNode.Children.Count);
        }

        public static void TestNullObjectReference(IReference reference)
        {
            var objReference = reference as ObjectReference;

            // Check that the reference is not null.
            Assert.IsNotNull(objReference);
            // Check that the Guids match.
            Assert.AreEqual(Guid.Empty, objReference.TargetGuid);
            // Check that the nodes match.
            Assert.AreEqual(null, objReference.TargetNode);
            // Check that the values match.
            Assert.AreEqual(null, objReference.ObjectValue);
        }

        public static void TestReferenceEnumerable(IReference reference, object targetValue)
        {
            var referenceEnum = reference as ReferenceEnumerable;
            var collection = (ICollection)targetValue;

            // Check that the reference is not null.
            Assert.IsNotNull(referenceEnum);
            // Check that the counts match.
            Assert.AreEqual(collection.Count, referenceEnum.Count);
            Assert.AreEqual(collection.Count, referenceEnum.Indices.Count);
            // Check that the object references match.
            foreach (var objReference in referenceEnum.Zip(collection.Cast<object>(), Tuple.Create))
            {
                Assert.AreEqual(objReference.Item1.ObjectValue, objReference.Item2);
                if (objReference.Item2 != null)
                {
                    TestNonNullObjectReference(objReference.Item1, objReference.Item2);
                }
                else
                {
                    TestNullObjectReference(objReference.Item1);
                }
            }
            // TODO: rework reference system and enable this
            //Assert.IsNull(reference.Index);
        }

        public static void TestStructContentNode(IGraphNode structNode, object structValue, int childCount)
        {
            if (structNode == null) throw new ArgumentNullException(nameof(structNode));
            if (structValue == null) throw new ArgumentNullException(nameof(structValue));
            // Check that the content is of the expected type.
            Assert.AreEqual(typeof(MemberContent), structNode.Content.GetType());
            // Check that the content is properly referencing its node.
            Assert.AreEqual(structNode, structNode.Content.OwnerNode);
            // A node with an ObjectContent should be a root node.
            Assert.IsNotNull(structNode.Parent);
            // A node with an ObjectContent should have the related object as value of its content.
            Assert.AreEqual(structValue, structNode.Content.Retrieve());
            // A node with an ObjectContent should not contain a reference.
            Assert.AreEqual(false, structNode.Content.IsReference);
            // Check that we have the expected number of children.
            Assert.AreEqual(childCount, structNode.Children.Count);
        }

        [Obsolete]
        public static void PrintModelContainerContent(NodeContainer container, IGraphNode rootNode = null)
        {
            Console.WriteLine(@"Container content:");
            Console.WriteLine(@"------------------");
            // Print the root node first, if specified
            if (rootNode != null)
                Console.WriteLine(rootNode.PrintHierarchy());

            // Print other nodes next
            // TODO: FIXME
            //foreach (var node in container.Guids.Select(container.GetNode).Where(x => x != rootNode))
            //{
            //    Console.WriteLine(node.PrintHierarchy());
            //}
            Console.WriteLine(@"------------------");
        }

        [Obsolete]
        public static void ConsistencyCheck(NodeContainer container, object rootObject)
        {
            return;
            var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            var model = container.GetNode(rootObject);
            visitor.Check((GraphNode)model, rootObject, rootObject.GetType(), true);
            foreach (var node in container.Nodes)
            {
                visitor.Check((GraphNode)node, node.Content.Value, node.Content.Type, true);
            }
        }
    }
}
