// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Quantum.References
{
    /// <summary>
    /// A class representing a reference to another object that has a different model.
    /// </summary>
    public sealed class ObjectReference : IReference
    {
        private object orphanObject;

        /// <summary>
        /// Initialize a new instance of the <see cref="ObjectReference"/> class using a data object.
        /// </summary>
        /// <remarks>This constructor should be used when the given <see cref="objectValue"/> has no mode node yet existing.</remarks>
        /// <param name="objectValue">A data object to reference. Can be null.</param>
        /// <param name="objectType">The type of data object to reference.</param>
        /// <param name="index">The index of this reference in its parent reference, if it is a <see cref="ReferenceEnumerable"/>.</param>
        internal ObjectReference(object objectValue, Type objectType, Index index)
        {
            Reference.CheckReferenceCreationSafeGuard();
            if (objectType == null) throw new ArgumentNullException(nameof(objectType));
            if (objectValue != null && !objectType.IsInstanceOfType(objectValue)) throw new ArgumentException(@"The given type does not match the given object.", nameof(objectValue));
            orphanObject = objectValue;
            Type = objectType;
            Index = index;
        }

        /// <summary>
        /// Gets the model node targeted by this reference, if available.
        /// </summary>
        public IGraphNode TargetNode { get; private set; }

        /// <inheritdoc/>
        public object ObjectValue => TargetNode != null ? TargetNode.Content.Value : orphanObject;

        /// <inheritdoc/>
        public Type Type { get; }

        /// <inheritdoc/>
        public Index Index { get; }

        /// <inheritdoc/>
        public ObjectReference AsObject => this;

        /// <inheritdoc/>
        public ReferenceEnumerable AsEnumerable { get { throw new InvalidCastException("This reference is not a ReferenceEnumerable"); } }

        /// <summary>
        /// Gets the <see cref="Guid"/> of the model node targeted by this reference, if available.
        /// </summary>
        public Guid TargetGuid { get; private set; }

        /// <inheritdoc/>
        public bool HasIndex(Index index)
        {
            return index.IsEmpty;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            TargetNode = null;
            TargetGuid = Guid.Empty;
            orphanObject = null;
        }

        /// <inheritdoc/>
        public void Refresh(object newObjectValue)
        {
            Clear();
            orphanObject = newObjectValue;
        }

        /// <inheritdoc/>
        public bool Equals(IReference other)
        {
            var otherReference = other as ObjectReference;
            if (otherReference == null)
                return false;

            return TargetGuid == otherReference.TargetGuid && TargetNode == otherReference.TargetNode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            string result = "";
            if (TargetNode != null)
                result += "[HasNode] ";
            else if (TargetGuid != Guid.Empty)
                result += "[HasGuid] ";

            result += TargetGuid + " " + (ObjectValue?.ToString() ?? "null");
            return result;
        }

        /// <summary>
        /// Set the <see cref="TargetNode"/> and <see cref="TargetGuid"/> of the targeted object by retrieving it from or creating it to the given <see cref="NodeContainer"/>.
        /// </summary>
        /// <param name="nodeContainer">The <see cref="NodeContainer"/> used to retrieve or create the target node.</param>
        /// <param name="nodeFactory">The factory to use to create nodes.</param>
        internal IGraphNode SetTarget(NodeContainer nodeContainer, NodeFactoryDelegate nodeFactory)
        {
            if (nodeContainer == null) throw new ArgumentNullException(nameof(nodeContainer));
            IGraphNode targetNode = nodeContainer.GetOrCreateNodeInternal(ObjectValue, nodeFactory);
            if (targetNode != null)
            {
                if (targetNode.Content.Value != null && !Type.IsInstanceOfType(targetNode.Content.Value))
                    throw new InvalidOperationException(@"The type of the retrieved node content does not match the type of this reference");

                // TODO: Disabled this exception which is triggered when a circular reference is made. This will be properly fixed when we'll get rid of the ShouldProcessReference mechanism.
                //if (TargetNode != null || TargetGuid != Guid.Empty)
                //    throw new InvalidOperationException("TargetNode has already been set.");

                if (targetNode.Content.Value != null && !Type.IsInstanceOfType(targetNode.Content.Value))
                    throw new InvalidOperationException("TargetNode type does not match the reference type.");

                TargetNode = targetNode;
                TargetGuid = targetNode.Guid;
            }
            return targetNode;
        }
    }
}
