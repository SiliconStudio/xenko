using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// An object used when when visiting an <see cref="AssetComposite"/> that tracks whether a part of this asset should be serialized as a reference or not.
    /// </summary>
    public class AssetCompositeVisitorContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCompositeVisitorContext"/> class.
        /// </summary>
        /// <param name="assetCompositeType">The type of <see cref="AssetComposite"/> that will be visited.</param>
        public AssetCompositeVisitorContext(Type assetCompositeType)
        {
            if (!typeof(AssetComposite).IsAssignableFrom(assetCompositeType))
                throw new ArgumentException($"The given type does not inherit from {nameof(AssetComposite)}.", nameof(assetCompositeType));

            var attributes = assetCompositeType.GetCustomAttributes(typeof(AssetPartReferenceAttribute), true).Cast<AssetPartReferenceAttribute>().ToArray();
            References = attributes;
            EnteredTypes = new Stack<AssetPartReferenceAttribute>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCompositeVisitorContext"/> class.
        /// </summary>
        /// <param name="assetPartReferenceAttributes">The list of <see cref="AssetPartReferenceAttribute"/> defining the behavior of the visit.</param>
        public AssetCompositeVisitorContext(IEnumerable<AssetPartReferenceAttribute> assetPartReferenceAttributes)
        {
            var attributes = assetPartReferenceAttributes.ToArray();
            References = attributes;
            EnteredTypes = new Stack<AssetPartReferenceAttribute>();
        }

        /// <summary>
        /// Gets the collection of <see cref="AssetPartReferenceAttribute"/> describing part types and their behavior during serialization.
        /// </summary>
        public AssetPartReferenceAttribute[] References { get; }

        /// <summary>
        /// Gets the stack of part type that have been entered by visit.
        /// </summary>
        public Stack<AssetPartReferenceAttribute> EnteredTypes { get; }

        /// <summary>
        /// Gets whether the node currently entered is an asset part that should be serialized as a reference.
        /// </summary>
        public bool SerializeAsReference { get; private set; }

        /// <summary>
        /// Notifies that the visitor entered a node.
        /// </summary>
        /// <param name="type">The type of node the visitor entered.</param>
        /// <returns>True if this call has pushed a value to <see cref="EnteredTypes"/>, False otherwise.</returns>
        /// <remarks>The value returned by this method should be passed to the corresponding call to <see cref="LeaveNode"/>.</remarks>
        public bool EnterNode(Type type)
        {
            // EnterNode should not be called from inside a part that should be serialized as a reference - this will also fail if calls to LeaveNode are missing.
            if (SerializeAsReference)
                throw new InvalidOperationException("EnterNode invoked inside a node that should be serialized as a reference.");

            // Is this a referenceable type?
            var typeAttribute = References.FirstOrDefault(x => x.ReferenceableType.IsAssignableFrom(type));
            if (typeAttribute != null)
            {
                // What is the last referenceable type we entered? (serialized as-is instead of referenced)
                var lastEntered = EnteredTypes.Count > 0 ? EnteredTypes.Peek() : null;

                // It is a container for the type we're evaluating?
                if (lastEntered != null && !lastEntered.ContainedTypes.Any(x => x.IsAssignableFrom(type)))
                {
                    // Otherwise, serialize a reference to this object instead.
                    SerializeAsReference = true;
                }

                EnteredTypes.Push(typeAttribute);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Notifies that the visitor left a node.
        /// </summary>
        /// <param name="type">The type of node the visitor left.</param>
        /// <param name="removeLastEnteredType">If True, the last value on the <see cref="EnteredTypes"/> stack will be removed.</param>
        /// <remarks>The value passed to <paramref name="removeLastEnteredType"/> should be the return value of <see cref="EnterNode"/>.</remarks>
        public void LeaveNode(Type type, bool removeLastEnteredType)
        {
            // Reset this flag for sanity
            SerializeAsReference = false;

            // Did we enter a referenceable type and actually serialized it?
            if (removeLastEnteredType)
            {
                EnteredTypes.Pop();
            }
        }
    }
}
