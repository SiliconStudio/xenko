// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// A visitor for serializable data (binary, yaml and editor).
    /// </summary>
    public abstract class DataVisitorBase : IDataVisitor
    {
        private readonly HashSet<object> visitedObjects = new HashSet<object>(ReferenceEqualityComparer<object>.Default);
        private readonly Dictionary<Type, IDataCustomVisitor> mapTypeToCustomVisitors = new Dictionary<Type, IDataCustomVisitor>();
        private VisitorContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitorBase"/> class.
        /// </summary>
        /// <param name="attributeRegistry">The attribute registry.</param>
        protected DataVisitorBase(IAttributeRegistry attributeRegistry) : this(new TypeDescriptorFactory(attributeRegistry))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitorBase"/> class.
        /// </summary>
        protected DataVisitorBase()
            : this(Reflection.TypeDescriptorFactory.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataVisitorBase"/> class.
        /// </summary>
        /// <param name="typeDescriptorFactory">The type descriptor factory.</param>
        /// <exception cref="System.ArgumentNullException">typeDescriptorFactory</exception>
        protected DataVisitorBase(ITypeDescriptorFactory typeDescriptorFactory)
        {
            if (typeDescriptorFactory == null) throw new ArgumentNullException("typeDescriptorFactory");
            TypeDescriptorFactory = typeDescriptorFactory;
            CustomVisitors = new List<IDataCustomVisitor>();
            context.DescriptorFactory = TypeDescriptorFactory;
            context.Visitor = this;
            CurrentPath = new MemberPath(16);
        }

        /// <summary>
        /// Gets the type descriptor factory.
        /// </summary>
        /// <value>The type descriptor factory.</value>
        public ITypeDescriptorFactory TypeDescriptorFactory { get; private set; }

        /// <summary>
        /// Gets or sets the custom visitors.
        /// </summary>
        /// <value>The custom visitors.</value>
        public List<IDataCustomVisitor> CustomVisitors { get; private set; }

        /// <summary>
        /// Gets the current member path being visited.
        /// </summary>
        /// <value>The current path.</value>
        protected MemberPath CurrentPath { get; private set; }

        /// <summary>
        /// Gets the attribute registry.
        /// </summary>
        /// <value>The attribute registry.</value>
        public IAttributeRegistry AttributeRegistry
        {
            get
            {
                return TypeDescriptorFactory.AttributeRegistry;
            }
        }

        /// <summary>
        /// Resets this instance (clears the cache of visited objects).
        /// </summary>
        public virtual void Reset()
        {
            visitedObjects.Clear();
        }

        /// <summary>
        /// Visits the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        public void Visit(object obj)
        {
            Visit(obj, null);
        }

        /// <summary>
        /// Visits the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <exception cref="System.ArgumentNullException">
        /// obj
        /// or
        /// descriptor
        /// </exception>
        /// <exception cref="System.ArgumentException">Descriptor [{0}] type doesn't correspond to object type [{1}].ToFormat(descriptor.Type, obj.GetType())</exception>
        protected void Visit(object obj, ITypeDescriptor descriptor)
        {
            if (obj == null)
            {
                VisitNull();
                return;
            }

            var objectType = obj.GetType();
            if (descriptor == null || descriptor.Type != objectType)
            {
                descriptor = TypeDescriptorFactory.Find(objectType);
            }

            if (descriptor is NullableDescriptor)
            {
                descriptor = TypeDescriptorFactory.Find(((NullableDescriptor)descriptor).UnderlyingType);
            }

            context.Instance = obj;
            context.Descriptor = (ObjectDescriptor)descriptor;

            switch (descriptor.Category)
            {
                case DescriptorCategory.Primitive:
                    VisitPrimitive(obj, (PrimitiveDescriptor)descriptor);
                    break;
                default:
                    if (CanVisit(obj))
                    {
                        IDataCustomVisitor customVisitor;
                        if (!mapTypeToCustomVisitors.TryGetValue(objectType, out customVisitor) && CustomVisitors.Count > 0)
                        {
                            for (int i = CustomVisitors.Count - 1; i >= 0; i--)
                            {
                                var dataCustomVisitor = CustomVisitors[i];
                                if (dataCustomVisitor.CanVisit(objectType))
                                {
                                    customVisitor = dataCustomVisitor;
                                    mapTypeToCustomVisitors.Add(objectType, dataCustomVisitor);
                                    break;
                                }
                            }
                        }

                        if (customVisitor != null)
                        {
                            customVisitor.Visit(ref context);
                        }
                        else
                        {
                            VisitObject(obj, context.Descriptor, true);
                        }
                    }
                    break;
            }
        }

        public virtual void VisitNull()
        {
        }

        public virtual void VisitPrimitive(object primitive, PrimitiveDescriptor descriptor)
        {
        }

        public virtual void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
        {
            if (!obj.GetType().IsArray && visitMembers)
            {
                foreach (var member in descriptor.Members)
                {
                    CurrentPath.Push(member);
                    VisitObjectMember(obj, descriptor, member, member.Get(obj));
                    CurrentPath.Pop();
                }
            }

            switch (descriptor.Category)
            {
                case DescriptorCategory.Array:
                    VisitArray((Array)obj, (ArrayDescriptor)descriptor);
                    break;
                case DescriptorCategory.Collection:
                    VisitCollection((IEnumerable)obj, (CollectionDescriptor)descriptor);
                    break;
                case DescriptorCategory.Dictionary:
                    VisitDictionary(obj, (DictionaryDescriptor)descriptor);
                    break;
            }
        }

        public virtual void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
        {
            Visit(value, member.TypeDescriptor);
        }

        public virtual void VisitArray(Array array, ArrayDescriptor descriptor)
        {
            for (int i = 0; i < array.Length; i++)
            {
                var value = array.GetValue(i);
                CurrentPath.Push(descriptor, i);
                VisitArrayItem(array, descriptor, i, value, TypeDescriptorFactory.Find(value?.GetType() ?? descriptor.ElementType));
                CurrentPath.Pop();
            }
        }

        public virtual void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            Visit(item, itemDescriptor);
        }

        public virtual void VisitCollection(IEnumerable collection, CollectionDescriptor descriptor)
        {
            int i = 0;

            // Make a copy in case VisitCollectionItem mutates something
            var items = new List<object>();
            foreach (var item in collection)
            {
                items.Add(item);
            }

            foreach (var item in items)
            {
                CurrentPath.Push(descriptor, i);
                VisitCollectionItem(collection, descriptor, i, item, TypeDescriptorFactory.Find(item?.GetType() ?? descriptor.ElementType));
                CurrentPath.Pop();
                i++;
            }
        }

        public virtual void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
        {
            Visit(item, itemDescriptor);
        }

        public virtual void VisitDictionary(object dictionary, DictionaryDescriptor descriptor)
        {
            // Make a copy in case VisitCollectionItem mutates something
            var items = new List<KeyValuePair<object, object>>();
            foreach (var item in descriptor.GetEnumerator(dictionary))
            {
                items.Add(item);
            }

            foreach (var keyValue in items)
            {
                var key = keyValue.Key;
                var keyDescriptor = TypeDescriptorFactory.Find(keyValue.Key?.GetType() ?? descriptor.KeyType);
                var value = keyValue.Value;
                var valueDescriptor = TypeDescriptorFactory.Find(keyValue.Value?.GetType() ?? descriptor.ValueType);

                CurrentPath.Push(descriptor, key);
                VisitDictionaryKeyValue(dictionary, descriptor, key, keyDescriptor, value, valueDescriptor);
                CurrentPath.Pop();
            }
        }

        public virtual void VisitDictionaryKeyValue(object dictionary, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
        {
            Visit(key, keyDescriptor);
            Visit(value, valueDescriptor);
        }

        protected virtual bool CanVisit(object obj)
        {
            // Always visit valuetypes
            if (obj.GetType().GetTypeInfo().IsValueType)
            {
                return true;
            }

            if (visitedObjects.Contains(obj))
            {
                return false;
            }
            visitedObjects.Add(obj);
            return true;
        }
    }
}