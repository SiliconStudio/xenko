// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Quantum
{
    public static class ModelNodeExtensions
    {
        public static void SetValue(this IModelNode node, object value, object index = null)
        {
            if (index != null)
            {
                var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    collectionDescriptor.SetValue(node.Content.Value, (int)index, value);
                }
                else if (dictionaryDescriptor != null)
                {
                    dictionaryDescriptor.SetValue(node.Content.Value, index, value);
                }
                else
                    throw new NotSupportedException("Unable to set the node value, the collection is unsupported");

                // TODO: shouldn't this be done as long as the value is not a primitive type?
                var memberContent = node.Content as MemberContent;
                memberContent?.UpdateReferences();
            }
            else
            {
                node.Content.Value = value;
            }
        }

        public static object GetValue(this IModelNode node, object index)
        {
            if (index != null)
            {
                var collectionDescriptor = node.Content.Descriptor as CollectionDescriptor;
                var dictionaryDescriptor = node.Content.Descriptor as DictionaryDescriptor;
                if (collectionDescriptor != null)
                {
                    return collectionDescriptor.GetValue(node.Content.Value, (int)index);
                }
                if (dictionaryDescriptor != null)
                {
                    return dictionaryDescriptor.GetValue(node.Content.Value, index);
                }

                throw new NotSupportedException("Unable to get the node value, the collection is unsupported");
            }
            return node.Content.Value;
        }

        /// <summary>
        /// Retrieve the child node of the given <see cref="IModelNode"/> that matches the given name.
        /// </summary>
        /// <param name="modelNode">The view model node to look into.</param>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or <c>null</c> if no child matches.</returns>
        public static IModelNode GetChild(this IModelNode modelNode, string name)
        {
            return modelNode.Children.FirstOrDefault(x => x.Name == name);
        }

        /// <summary>
        /// Retrieve the child node of the given <see cref="IModelNode"/> that matches the given name. If the node represents an object reference, it returns the referenced object.
        /// </summary>
        /// <param name="modelNode">The view model node to look into.</param>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns>The child node that matches the given name, or the referenced node if the child hold an object reference, or <c>null</c> if no child matches.</returns>
        public static IModelNode GetReferencedChild(this IModelNode modelNode, string name)
        {
            var child = modelNode.GetChild(name);
            return child != null ? child.ResolveTarget() : null;
        }

        /// <summary>
        /// Gets the child of the given node that matches the given name. If the given node holds an object reference, resolve the target of the reference
        /// and gets the child of the target node that matches the given name.
        /// </summary>
        /// <param name="modelNode">The model node.</param>
        /// <param name="name">The name of the child to retrieve.</param>
        /// <returns></returns>
        public static IModelNode GetChildThroughReferences(this IModelNode modelNode, string name)
        {
            var child = modelNode.GetChild(name) ?? modelNode.ResolveTarget().GetChild(name);
            return child;
        }

        /// <summary>
        /// Gets the target node of the given <see cref="IModelNode"/> if it holds an object reference, or returns the node itself otherwise
        /// </summary>
        /// <param name="modelNode">The node that may contains an object reference.</param>
        /// <returns>The target node of the given <see cref="IModelNode"/> if it holds an object reference, or the node itself otherwise.</returns>
        public static IModelNode ResolveTarget(this IModelNode modelNode)
        {
            var objReference = modelNode.Content.Reference as ObjectReference;
            return objReference != null ? objReference.TargetNode : modelNode;
        }

        /// <summary>
        /// Gets whether a given <see cref="ITypeDescriptor"/> represents a collection or a dictionary of primitive values.
        ///  
        /// </summary>
        /// <param name="descriptor">The type descriptor to check.</param>
        /// <returns><c>true</c> if the given <see cref="ITypeDescriptor"/> represents a collection or a dictionary of primitive values, <c>false</c> otherwise.</returns>
        public static bool IsPrimitiveCollection(this ITypeDescriptor descriptor)
        {
            var collectionDescriptor = descriptor as CollectionDescriptor;
            var dictionaryDescriptor = descriptor as DictionaryDescriptor;
            Type elementType = null;
            if (collectionDescriptor != null)
            {
                elementType = collectionDescriptor.ElementType;
            }
            else if (dictionaryDescriptor != null)
            {
                elementType = dictionaryDescriptor.ValueType;

            }
            if (elementType != null)
            {
                if (elementType.IsNullable())
                    elementType = Nullable.GetUnderlyingType(elementType);

                return elementType.IsPrimitive || elementType == typeof(string) || elementType.IsEnum;
            }
            return false;
        }
    }
}