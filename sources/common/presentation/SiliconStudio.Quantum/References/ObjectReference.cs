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
        internal ObjectReference(object objectValue, Type objectType, object index)
        {
            Reference.CheckReferenceCreationSafeGuard();
            if (objectType == null) throw new ArgumentNullException("objectType");
            if (objectValue != null && !objectType.IsInstanceOfType(objectValue)) throw new ArgumentException(@"The given type does not match the given object.", "objectValue");
            orphanObject = objectValue;
            Type = objectType;
            Index = index;
        }

        /// <summary>
        /// Gets the model node targeted by this reference, if available.
        /// </summary>
        public IGraphNode TargetNode { get; private set; }

        /// <inheritdoc/>
        public object ObjectValue { get { return TargetNode != null ? TargetNode.Content.Value : orphanObject; } }

        /// <inheritdoc/>
        public Type Type { get; private set; }

        /// <inheritdoc/>
        public object Index { get; private set; }

        /// <inheritdoc/>
        public ObjectReference AsObject { get { return this; } }

        /// <inheritdoc/>
        public ReferenceEnumerable AsEnumerable { get { throw new InvalidCastException("This reference is not a ReferenceEnumerable"); } }

        /// <summary>
        /// Gets the <see cref="Guid"/> of the model node targeted by this reference, if available.
        /// </summary>
        public Guid TargetGuid { get; private set; }

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

        /// <summary>
        /// Set the <see cref="TargetNode"/> and <see cref="TargetGuid"/> of the targeted object by retrieving it from or creating it to the given <see cref="ModelContainer"/>.
        /// </summary>
        /// <param name="modelContainer">The <see cref="ModelContainer"/> used to retrieve or create the target node.</param>
        public IGraphNode SetTarget(ModelContainer modelContainer)
        {
            if (modelContainer == null) throw new ArgumentNullException("modelContainer");
            IGraphNode targetNode = modelContainer.GetOrCreateModelNode(ObjectValue);
            if (targetNode != null)
            {
                if (targetNode.Content.Value != null && !Type.IsInstanceOfType(targetNode.Content.Value)) throw new InvalidOperationException(@"The type of the retrieved node content does not match the type of this reference");

                if (TargetNode != null || TargetGuid != Guid.Empty)
                    throw new InvalidOperationException("TargetNode has already been set.");
                if (targetNode.Content.Value != null && !Type.IsInstanceOfType(targetNode.Content.Value))
                    throw new InvalidOperationException("TargetNode type does not match the reference type.");
                TargetNode = targetNode;
                TargetGuid = targetNode.Guid;
            }
            return targetNode;
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

            result += TargetGuid + " " + (ObjectValue != null ? ObjectValue.ToString() : "null");
            return result;
        }
    }
}