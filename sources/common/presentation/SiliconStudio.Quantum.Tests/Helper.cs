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
        /// <summary>
        /// Tests the validity of a node that is an object that is not a collection
        /// </summary>
        /// <param name="node">The node to validate.</param>
        /// <param name="obj">The value represented by this node.</param>
        /// <param name="childCount">The number of members expected in the node.</param>
        public static void TestNonCollectionObjectContentNode(IGraphNode node, object obj, int childCount)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Check that the content is of the expected type.
            Assert.IsInstanceOf<ObjectContent>(node.Content);
            // Check that the content is properly referencing its node.
            Assert.AreEqual(node, node.Content.OwnerNode);
            // A node with an ObjectContent should have the same name that the type of its content.
            Assert.AreEqual(obj.GetType().Name, node.Name);
            // A node with an ObjectContent should be a root node.
            Assert.IsNull(node.Parent);
            // A node with an ObjectContent should have the related object as value of its content.
            Assert.AreEqual(obj, node.Content.Retrieve());
            // A node with an ObjectContent should not contain a reference if it does not represent a collection.
            Assert.AreEqual(false, node.Content.IsReference);
            // Check that we have the expected number of children.
            Assert.AreEqual(childCount, node.Children.Count);
        }

        /// <summary>
        /// Tests the validity of a node that is an object that is a collection
        /// </summary>
        /// <param name="node">The node to validate.</param>
        /// <param name="obj">The value represented by this node.</param>
        /// <param name="isReference">Indicate whether the node is expected to contain an enumerable reference to the collection items.</param>
        public static void TestCollectionObjectContentNode(IGraphNode node, object obj, bool isReference)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            // Check that the content is of the expected type.
            Assert.IsInstanceOf<ObjectContent>(node.Content);
            // Check that the content is properly referencing its node.
            Assert.AreEqual(node, node.Content.OwnerNode);
            // A node with an ObjectContent should have the same name that the type of its content.
            Assert.AreEqual(obj.GetType().Name, node.Name);
            // A node with an ObjectContent should be a root node.
            Assert.IsNull(node.Parent);
            // A node with an ObjectContent should have the related object as value of its content.
            Assert.AreEqual(obj, node.Content.Retrieve());
            if (isReference)
            {
                // A node with an ObjectContent representing a collection of reference types should contain an enumerable reference.
                Assert.AreEqual(true, node.Content.IsReference);
                Assert.IsInstanceOf<ReferenceEnumerable>(node.Content.Reference);
            }
            else
            {
                // A node with an ObjectContent representing a collection of primitive or struct types should not contain a refernce.
                Assert.AreEqual(false, node.Content.IsReference);            
            }
            // A node with an ObjectContent representing a collection should not have any child.
            Assert.AreEqual(0, node.Children.Count);
        }

        /// <summary>
        /// Tests the validity of a node that is a member of an object.
        /// </summary>
        /// <param name="containerNode">The node of the container of this member.</param>
        /// <param name="memberNode">The memeber node to validate.</param>
        /// <param name="container">The value represented by the container node.</param>
        /// <param name="member">The value of the member represented by the member node.</param>
        /// <param name="memberName">The name of the member to validate.</param>
        /// <param name="isReference">Indicate whether the member node is expected to contain a reference to the value it represents.</param>
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

        /// <summary>
        /// Tests the validity of a reference to a non-null target object.
        /// </summary>
        /// <param name="reference">The reference to test.</param>
        /// <param name="targetValue">The actual value pointed by the reference.</param>
        /// <param name="hasIndex">Indicates whether the reference has an index.</param>
        public static void TestNonNullObjectReference(IReference reference, object targetValue, bool hasIndex)
        {
            var objReference = reference as ObjectReference;

            // Check that the reference is not null.
            Assert.IsNotNull(objReference);
            // Check that the values match.
            Assert.AreEqual(targetValue, objReference.TargetNode.Content.Retrieve());
            // Check that the values match.
            Assert.AreEqual(targetValue, objReference.ObjectValue);
            // Check that that we have an index if expected.
            Assert.AreEqual(hasIndex, !reference.Index.IsEmpty);
            // Check that the target is an object content node.
            TestNonCollectionObjectContentNode(objReference.TargetNode, targetValue, objReference.TargetNode.Children.Count);
        }

        /// <summary>
        /// Tests the validity of a reference to a non-null target node.
        /// </summary>
        /// <param name="reference">The reference to test.</param>
        /// <param name="targetNode">The node that is supposed to be the target of the reference.</param>
        /// <param name="targetValue">The actual value pointed by the reference.</param>
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
            TestNonCollectionObjectContentNode(targetNode, targetValue, targetNode.Children.Count);
        }

        /// <summary>
        /// Tests the validity of a reference to a null target object.
        /// </summary>
        /// <param name="reference">The reference to test.</param>
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

        /// <summary>
        /// Tests the validity of an enumerable reference.
        /// </summary>
        /// <param name="reference">The reference to test.</param>
        /// <param name="targetValue">The actual collection pointed by the reference.</param>
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
                    TestNonNullObjectReference(objReference.Item1, objReference.Item2, true);
                }
                else
                {
                    TestNullObjectReference(objReference.Item1);
                }
            }
            // TODO: rework reference system and enable this
            //Assert.IsNull(reference.Index);
        }

        /// <summary>
        /// Tests the validity of a node that represents a structure.
        /// </summary>
        /// <param name="structNode">The structure node to test.</param>
        /// <param name="structValue">The value of the structure represented by this node.</param>
        /// <param name="childCount">The number of members expected in the node.</param>
        public static void TestStructContentNode(IGraphNode structNode, object structValue, int childCount)
        {
            if (structNode == null) throw new ArgumentNullException(nameof(structNode));
            if (structValue == null) throw new ArgumentNullException(nameof(structValue));
            // Check that the content is properly referencing its node.
            Assert.AreEqual(structNode, structNode.Content.OwnerNode);
            // A struct node should have the related struct as value of its content.
            Assert.AreEqual(structValue, structNode.Content.Retrieve());
            // A struct node should not contain a reference.
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
            //var visitor = new ModelConsistencyCheckVisitor(container.NodeBuilder);
            //var model = container.GetNode(rootObject);
            //visitor.Check((GraphNode)model, rootObject, rootObject.GetType(), true);
            //foreach (var node in container.Nodes)
            //{
            //    visitor.Check((GraphNode)node, node.Content.Value, node.Content.Type, true);
            //}
        }
    }
}
