// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Core.Extensions
{
    public static class TypeDescriptorExtensions
    {
        private static readonly List<Type> AllInstantiableTypes = new List<Type>();
        private static readonly List<Type> AllTypes = new List<Type>();
        private static readonly List<Assembly> AllAssemblies = new List<Assembly>();
        private static readonly Dictionary<Type, List<Type>> InheritableInstantiableTypes = new Dictionary<Type, List<Type>>();
        private static readonly Dictionary<Type, List<Type>> InheritableTypes = new Dictionary<Type, List<Type>>();

        static TypeDescriptorExtensions()
        {
            AssemblyRegistry.AssemblyRegistered += ClearCache;
            AssemblyRegistry.AssemblyUnregistered += ClearCache;
        }

        public static bool MatchType(this ITypeDescriptor descriptor, Type type)
        {
            return type.IsAssignableFrom(descriptor.Type);
        }

        public static T GetAttribute<T>(this ITypeDescriptor descriptor, MemberInfo memberInfo) where T : Attribute
        {
            return descriptor.Factory.AttributeRegistry.GetAttribute<T>(memberInfo);
        }

        public static T GetAttribute<T>(this ITypeDescriptor descriptor, IMemberDescriptor memberDescriptor) where T : Attribute
        {
            var memberDescriptorBase = memberDescriptor as MemberDescriptorBase;
            return memberDescriptorBase?.MemberInfo != null ? descriptor.Factory.AttributeRegistry.GetAttribute<T>(memberDescriptorBase.MemberInfo) : null;
        }

        public static IEnumerable<Type> GetInheritedInstantiableTypes(this Type type)
        {
            lock (AllAssemblies)
            {
                List<Type> result;
                if (!InheritableInstantiableTypes.TryGetValue(type, out result))
                {
                    // If allTypes is empty, then reload it
                    if (AllInstantiableTypes.Count == 0)
                    {
                        // Just keep a list of assemblies in order to check which assemblies was scanned by this method
                        if (AllAssemblies.Count == 0)
                        {
                            AllAssemblies.AddRange(AssemblyRegistry.Find(AssemblyCommonCategories.Assets));
                        }
                        AllInstantiableTypes.AddRange(AllAssemblies.SelectMany(x => x.GetTypes().Where(IsInstantiableType)));
                    }

                    result = AllInstantiableTypes.Where(type.IsAssignableFrom).ToList();
                    InheritableInstantiableTypes.Add(type, result);
                }
                return result;
            }
        }

        public static IEnumerable<Type> GetInheritedTypes(this Type type)
        {
            lock (AllAssemblies)
            {
                List<Type> result;
                if (!InheritableTypes.TryGetValue(type, out result))
                {
                    // If allTypes is empty, then reload it
                    if (AllTypes.Count == 0)
                    {
                        // Just keep a list of assemblies in order to check which assemblies was scanned by this method
                        if (AllAssemblies.Count == 0)
                        {
                            AllAssemblies.AddRange(AssemblyRegistry.Find(AssemblyCommonCategories.Assets));
                        }
                        AllTypes.AddRange(AllAssemblies.SelectMany(x => x.GetTypes().Where(y => y.IsPublic || y.IsNestedPublic)));
                    }

                    result = AllTypes.Where(type.IsAssignableFrom).ToList();
                    InheritableTypes.Add(type, result);
                }
                return result;
            }
        }

        private static bool IsInstantiableType(Type x)
        {
            return (x.IsPublic || x.IsNestedPublic) && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null;
        }

        private static void ClearCache(object sender, AssemblyRegisteredEventArgs e)
        {
            lock (AllAssemblies)
            {
                AllAssemblies.Clear();
                AllInstantiableTypes.Clear();
                AllTypes.Clear();
                InheritableTypes.Clear();
                InheritableInstantiableTypes.Clear();
            }
        }

        /// <summary>
        /// Attempts to return the type of inner values of an <see cref="ITypeDescriptor"/>, if it represents an enumerable type. If the given type descriptor is
        /// a <see cref="CollectionDescriptor"/>, this method will return its <see cref="CollectionDescriptor.ElementType"/> property. If the given type descriptor
        /// is a <see cref="DictionaryDescriptor"/>, this method will return its <see cref="DictionaryDescriptor.ValueType"/>. Otherwise, it will return the
        /// <see cref="ITypeDescriptor.Type"/> property.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <returns>The type of inner values of an <see cref="ITypeDescriptor"/>.</returns>
        public static Type GetInnerCollectionType(this ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;

            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
                type = collectionDescriptor.ElementType;

            var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
            if (dictionaryDescriptor != null)
                type = dictionaryDescriptor.ValueType;

            return type;
        }

        /// <summary>
        /// Attempts to return the content of an object according to its type descriptor and a potential index. If the given type descriptor is
        /// a <see cref="CollectionDescriptor"/> or a <see cref="DictionaryDescriptor"/>, this method will return the value of <see cref="instance"/>
        /// at the provided index. Otherwise, it will return the instance itself.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <param name="instance">The instance object on which to retrieve the content value.</param>
        /// <param name="index">The index to use, if the instance is a collection or a dictionary.</param>
        /// <returns>The item of the collection at the given index if the instance is a collection, otherwise the instance itself.</returns>
        public static object GetInnerCollectionContent(this ITypeDescriptor typeDescriptor, object instance, object index)
        {
            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
                instance = collectionDescriptor.GetValue(instance, index);

            var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
            if (dictionaryDescriptor != null)
                instance = dictionaryDescriptor.GetValue(instance, index);

            return instance;
        }

        /// <summary>
        /// Attempts to set the item of an object according to its type descriptor and a potential index. If the given type descriptor is
        /// a <see cref="CollectionDescriptor"/> or a <see cref="DictionaryDescriptor"/>, this method will set the value of <see cref="instance"/>
        /// at the provided index. Otherwise, it does nothing.
        /// </summary>
        /// <param name="typeDescriptor">The type descriptor.</param>
        /// <param name="instance">The instance object on which to set the item value.</param>
        /// <param name="index">The index to use, if the instance is a collection or a dictionary.</param>
        /// <param name="newValue">The new value to set.</param>
        /// <returns>The instance itself if is a collection, otherwise the newValue.</returns>
        public static object SetInnerCollectionContent(this ITypeDescriptor typeDescriptor, object instance, object index, object newValue)
        {
            var collectionDescriptor = typeDescriptor as CollectionDescriptor;
            if (collectionDescriptor != null)
            {
                collectionDescriptor.SetValue(instance, index, newValue);
                return instance;
            }

            var dictionaryDescriptor = typeDescriptor as DictionaryDescriptor;
            if (dictionaryDescriptor != null)
            {
                dictionaryDescriptor.SetValue(instance, index, newValue);
                return instance;
            }

            return newValue;
        }
    }
}
