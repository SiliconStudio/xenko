using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Serializers
{
    /// <summary>
    /// An object used when when visiting an <see cref="AssetComposite"/> that tracks whether a part of this asset should be serialized as a reference or not.
    /// </summary>
    public class AssetCompositeVisitorContext
    {
        private readonly Stack<State> states = new Stack<State>();

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
            states.Push(new State(null, attributes.Where(x => x.ExistsTopLevel).Select(x => x.ReferenceableType).ToArray()));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCompositeVisitorContext"/> class.
        /// </summary>
        /// <param name="assetPartReferenceAttributes">The list of <see cref="AssetPartReferenceAttribute"/> defining the behavior of the visit.</param>
        public AssetCompositeVisitorContext(IEnumerable<AssetPartReferenceAttribute> assetPartReferenceAttributes)
        {
            var attributes = assetPartReferenceAttributes.ToArray();
            References = attributes;
        }

        /// <summary>
        /// Gets the collection of <see cref="AssetPartReferenceAttribute"/> describing part types and their behavior during serialization.
        /// </summary>
        public AssetPartReferenceAttribute[] References { get; }

        /// <summary>
        /// Gets whether the node currently entered is an asset part that should be serialized as a reference.
        /// </summary>
        public bool SerializeAsReference { get; private set; }

        /// <summary>
        /// Notifies that the visitor entered a node.
        /// </summary>
        /// <param name="type">The type of node the visitor entered.</param>
        /// <returns>True if this call has pushed a value to <see cref="states"/>, False otherwise.</returns>
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
                var lastEntered = states.Count > 0 ? (State?)states.Peek() : null;

                // It is a container for the type we're evaluating?
                if (lastEntered != null && !lastEntered.Value.ContainedTypes.Any(x => x.IsAssignableFrom(type)))
                {
                    // Otherwise, serialize a reference to this object instead.
                    SerializeAsReference = true;
                }

                states.Push(new State(typeAttribute, typeAttribute.ContainedTypes));
                return true;
            }
            return false;
        }

        public bool EnterNode(IMemberDescriptor member)
        {
            var assetPartContainedAttribute = member.GetCustomAttributes<AssetPartContainedAttribute>(true).FirstOrDefault();
            if (assetPartContainedAttribute != null)
            {
                states.Push(new State(null, assetPartContainedAttribute.ContainedTypes));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Notifies that the visitor left a node.
        /// </summary>
        /// <param name="removeLastEnteredNode"></param>
        /// <remarks>The value passed to <paramref name="removeLastEnteredNode"/> should be the return value of <see cref="EnterNode"/>.</remarks>
        public void LeaveNode(bool removeLastEnteredNode)
        {
            // Reset this flag for sanity
            SerializeAsReference = false;

            // Did we enter a referenceable type and actually serialized it?
            if (removeLastEnteredNode)
            {
                states.Pop();
            }
        }

        struct State
        {
            public readonly AssetPartReferenceAttribute EnteredType;
            public readonly Type[] ContainedTypes;

            public State(AssetPartReferenceAttribute enteredType, Type[] containedTypes)
            {
                EnteredType = enteredType;
                ContainedTypes = containedTypes;
            }
        }

        public AssetPartReferenceAttribute GetLastEnteredType()
        {
            // Reminder: Stack<T> enumerates backward
            foreach (var state in states)
            {
                if (state.EnteredType != null)
                    return state.EnteredType;
            }

            return null;
        }
    }
}
