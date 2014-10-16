// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    // TODO: refactor this helper class according to latest changes in Quantum
    public static class ViewModelConstructor
    {
        /// <summary>
        /// Create a node with the given content.
        /// </summary>
        /// <param name="name">The property name</param>
        /// <param name="content">The <see cref="IContent"/> representing the content of the child node to create.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The node that has been created.</returns>
        public static IViewModelNode CreateNode(string name, IContent content, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (content == null) throw new ArgumentNullException("content");

            var node = new ViewModelNode(name, content);
            node.Content.Flags |= contentFlags;
            node.Content.SerializeFlags |= serializeFlags;
            return node;
        }

        /// <summary>
        /// Add a child node to the given node with the given content.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The property name</param>
        /// <param name="content">The <see cref="IContent"/> representing the content of the child node to create.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddChildNode(IViewModelNode parentNode, string name, IContent content, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            if (parentNode == null) throw new ArgumentNullException("parentNode");
            var node = CreateNode(name, content, contentFlags, serializeFlags);
            parentNode.Children.Add(node);
            return node;
        }

        /// <summary>
        /// Create a node which contains a model object. This node content will be read-only and won't be serialized.
        /// </summary>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="value">a non-null object.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The node that has been created.</returns>
        public static IViewModelNode CreateModelNode(string name, object value, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            return CreateNode(name, new ObjectContent(value, value.GetType(), null), contentFlags, serializeFlags);
        }

        /// <summary>
        /// Add a child node which contains a model object. This node content will be read-only and won't be serialized.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="value">a non-null object.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddModelNode(IViewModelNode parentNode, string name, object value, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            return AddChildNode(parentNode, name, new ObjectContent(value, value.GetType(), null), contentFlags, serializeFlags);
        }

        /// <summary>
        /// Add a child node which contains a simple value-type or string element.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="value">A value-type object or a string.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddValueNode(IViewModelNode parentNode, string name, object value, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            return AddChildNode(parentNode, name, new ValueViewModelContent(value), contentFlags, serializeFlags);
        }

        /// <summary>
        /// Add a child node which is bound to a property of its parent's content value.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddPropertyNode(IViewModelNode parentNode, string name, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            var content = new PropertyInfoViewModelContent(new ParentValueViewModelContent(), parentNode.Content.Value.GetType().GetProperty(name));
            return AddChildNode(parentNode, name, content, contentFlags, serializeFlags);
        }
        
        /// <summary>
        /// Add a child node which contains an enumerable content of property info contents from a list. This is most suitable for edition.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="list">The list.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddContentListNode<T>(IViewModelNode parentNode, string name, IList<T> list)
        {
            var listNode = new ViewModelNode(name, new ObjectContent(list, list.GetType(), null));
            string indexerName = ((DefaultMemberAttribute)list.GetType().GetCustomAttributes(typeof(DefaultMemberAttribute), true)[0]).MemberName;
            PropertyInfo indexerProperty = list.GetType().GetProperty(indexerName);
            listNode.Content = new EnumerableViewModelContent<IContent>(() => list.Select((x, i) =>
                new PropertyInfoViewModelContent(listNode.Content, indexerProperty, i) { OwnerNode = listNode })
                );
            parentNode.Children.Add(listNode);
            return listNode;
        }

        /// <summary>
        /// Add a child node which contains an enumerable content of value contents from a list. This is most suitable for visualization.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="list">The list of value content.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created.</returns>
        public static IViewModelNode AddListNode<T>(IViewModelNode parentNode, string name, IList<T> list, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            return AddChildNode(parentNode, name, new EnumerableViewModelContent<T>(() => list), contentFlags, serializeFlags);
        }

        /// <summary>
        /// Helper function to generate an enumerable of <see cref="ViewModelReference"/> from an enumerable of model objects.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="enumerable">The enumerable of model objects</param>
        /// <param name="context">The current <see cref="ViewModelContext"/> used to update references Guids</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>A node containing an enumerable of <see cref="ViewModelReference"/> with up-to-date Guids.</returns>
        public static IViewModelNode AddReferenceEnumerableNode(IViewModelNode parentNode, string name, IEnumerable<object> enumerable, ViewModelContext context, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            IEnumerable<ViewModelReference> references = enumerable.Select(x =>
            {
                var reference = new ViewModelReference(x);
                reference.UpdateGuid(context);
                return reference;
            });
            return AddChildNode(parentNode, name, new EnumerableViewModelContent<ViewModelReference>(() => references), contentFlags, serializeFlags);
        }

        /// <summary>
        /// Add a child node that can be used to group or sort sub children. It is abstracted when using nodes such as PropertyNode.
        /// </summary>
        /// <param name="parentNode">The parent of the category node to create.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <returns>A new <see cref="ViewModelProxyNode"/> that is a child of <see cref="parentNode"/> and which can contains other children of the model of parentNode.</returns>
        public static IViewModelNode AddCategoryNode(IViewModelNode parentNode, string name)
        {
            var node = new ViewModelProxyNode(name, parentNode);
            parentNode.Children.Add(node);
            return node;
        }

        /// <summary>
        /// Add a a child node which contain a command that can be executed.
        /// </summary>
        /// <param name="parentNode">The parent name.</param>
        /// <param name="name">The name of the node to create.</param>
        /// <param name="commandAction">The command to execute.</param>
        /// <param name="contentFlags">The content flags to set.</param>
        /// <param name="serializeFlags">The content serialization flags to set.</param>
        /// <returns>The child node that has been created</returns>
        public static IViewModelNode AddCommandNode(IViewModelNode parentNode, string name, Action<IViewModelNode, object> commandAction, ViewModelContentFlags contentFlags = ViewModelContentFlags.None, ViewModelContentSerializeFlags serializeFlags = ViewModelContentSerializeFlags.SerializeValue)
        {
            var content = new RootViewModelContent((ExecuteCommand)((viewModel, parameter) => commandAction(viewModel, parameter)));
            return AddChildNode(parentNode, name, content, contentFlags, serializeFlags);
        }
    }
}
