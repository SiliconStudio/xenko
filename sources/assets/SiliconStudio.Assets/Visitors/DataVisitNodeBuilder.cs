// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using SiliconStudio.Assets.Tracking;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Assets.Visitors
{
    /// <summary>
    /// A visitor for producing a <see cref="DataVisitNode"/> for an object hierarchy.
    /// </summary>
    public sealed class DataVisitNodeBuilder : AssetVisitorBase
    {
        private readonly Stack<DataVisitNode> stackItems = new Stack<DataVisitNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitNodeBuilder"/> class.
        /// </summary>
        /// <param name="typeDescriptorFactory">The type descriptor factory.</param>
        /// <param name="rootInstance">The root instance of the object to visit.</param>
        /// <exception cref="System.ArgumentNullException">rootInstance</exception>
        private DataVisitNodeBuilder(ITypeDescriptorFactory typeDescriptorFactory, object rootInstance)
            : base(typeDescriptorFactory)
        {
            CustomVisitors.AddRange(AssetRegistry.GetDataVisitNodeBuilders());

            if (rootInstance == null) throw new ArgumentNullException(nameof(rootInstance));
            RootInstance = rootInstance;
            var objectDescriptor = typeDescriptorFactory.Find(rootInstance.GetType()) as ObjectDescriptor;
            if (objectDescriptor == null)
                throw new ArgumentException(@"Expecting an object", nameof(rootInstance));
            stackItems.Push(new DataVisitObject(rootInstance, objectDescriptor));
        }

        /// <summary>
        /// Gets the current <see cref="DataVisitNode"/>.
        /// </summary>
        /// <value>
        /// The current <see cref="DataVisitNode"/>.
        /// </value>
        public DataVisitNode CurrentNode => stackItems.Peek();

        /// <summary>
        /// Gets the root object visited by this instance.
        /// </summary>
        public object RootInstance { get; }

        /// <summary>
        /// Creates <see cref="DataVisitNode"/> from the specified instance.
        /// </summary>
        /// <param name="typeDescriptorFactory">The type descriptor factory.</param>
        /// <param name="rootInstance">The root instance to generate diff nodes.</param>
        /// <param name="customVisitors">Add </param>
        /// <returns>A diff node object.</returns>
        public static DataVisitObject Run(ITypeDescriptorFactory typeDescriptorFactory, object rootInstance, List<IDataCustomVisitor> customVisitors = null)
        {
            if (rootInstance == null) return null;
            var builder = new DataVisitNodeBuilder(typeDescriptorFactory, rootInstance);
            if (customVisitors != null)
            {
                builder.CustomVisitors.AddRange(customVisitors);
            }
            return builder.Run();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        /// <returns>Returns the root node associated with the instance being visited.</returns>
        public DataVisitObject Run()
        {
            Visit(RootInstance);
            return (DataVisitObject)stackItems.Pop();
        }

        public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            if (!AcceptMember(member))
            {
                return;
            }

            var node = stackItems.Peek();
            var newNode = new DataVisitMember(value, member);
            AddMember(node, newNode);

            stackItems.Push(newNode);
            base.VisitObjectMember(container, containerDescriptor, member, value);
            stackItems.Pop();
        }

        public override void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            var node = stackItems.Peek();
            // TODO modify DataVisitorBase to allow only IList?
            var newNode = new DataVisitListItem(index, item, itemDescriptor);
            AddItem(node, newNode);

            stackItems.Push(newNode);
            base.VisitCollectionItem(collection, descriptor, index, item, itemDescriptor);
            stackItems.Pop();
        }

        public override void VisitDictionaryKeyValue(object dictionary, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
        {
            var node = stackItems.Peek();
            // TODO modify DataVisitorBase to allow only IDictionary?
            var newNode = new DataVisitDictionaryItem(key, keyDescriptor, value, valueDescriptor);
            AddItem(node, newNode);

            stackItems.Push(newNode);
            base.VisitDictionaryKeyValue(dictionary, descriptor, key, keyDescriptor, value, valueDescriptor);
            stackItems.Pop();
        }

        public override void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            var node = stackItems.Peek();
            var newNode = new DataVisitArrayItem(index, item, itemDescriptor);
            AddItem(node, newNode);

            stackItems.Push(newNode);
            base.VisitArrayItem(array, descriptor, index, item, itemDescriptor);
            stackItems.Pop();
        }

        /// <summary>
        /// Adds a member to a <see cref="DataVisitNode"/> instance.
        /// </summary>
        /// <param name="thisObject">The this object.</param>
        /// <param name="member">The member.</param>
        /// <exception cref="System.ArgumentNullException">member</exception>
        private static void AddMember(DataVisitNode thisObject, DataVisitMember member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (thisObject.Members == null)
                thisObject.Members = new List<DataVisitNode>();

            member.Parent = thisObject;
            thisObject.Members.Add(member);
        }

        /// <summary>
        /// Adds an item (array, list or dictionary item) to a <see cref="DataVisitNode"/> instance.
        /// </summary>
        /// <param name="thisObject">The this object.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.ArgumentNullException">item</exception>
        private static void AddItem(DataVisitNode thisObject, DataVisitNode item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (thisObject.Items == null)
                thisObject.Items = new List<DataVisitNode>();

            item.Parent = thisObject;
            thisObject.Items.Add(item);
        }

        private static bool AcceptMember(IMemberDescriptor member)
        {
            if (!typeof(Asset).IsAssignableFrom(member.DeclaringType))
                return true;

            // Skip some properties that are not using when visiting
            return member.Name != SourceHashesHelper.MemberName && member.Name != Asset.BaseProperty && member.Name != Asset.BasePartsProperty && member.Name != "Id";
        }
    }
}
