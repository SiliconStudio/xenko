// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;

using SiliconStudio.Assets.Visitors;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// This analysis provides a method for visiting asset and file references 
    /// (<see cref="IContentReference" /> or <see cref="UFile" /> or <see cref="UDirectory" />)
    /// </summary>
    public class AssetReferenceAnalysis
    {
        private static readonly object CachingLock = new object();

        private static readonly Dictionary<object, List<AssetReferenceLink>> CachingReferences = new Dictionary<object, List<AssetReferenceLink>>();

        private static bool enableCaching;

        /// <summary>
        /// Gets or sets the enable caching. Only used when loading packages
        /// </summary>
        /// <value>The enable caching.</value>
        internal static bool EnableCaching
        {
            get
            {
                return enableCaching;
            }
            set
            {
                lock (CachingLock)
                {
                    if (enableCaching != value)
                    {
                        CachingReferences.Clear();
                    }

                    enableCaching = value;
                }
            }
        }

        /// <summary>
        /// Gets all references (subclass of <see cref="IContentReference" /> and <see cref="UFile" />) from the specified asset
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>A list of references.</returns>
        public static List<AssetReferenceLink> Visit(object obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            List<AssetReferenceLink> assetReferences = null;

            lock (CachingLock)
            {
                if (enableCaching)
                {
                    if (CachingReferences.TryGetValue(obj, out assetReferences))
                    {
                        assetReferences = new List<AssetReferenceLink>(assetReferences);
                    }
                }
            }

            if (assetReferences == null)
            {
                assetReferences = new List<AssetReferenceLink>();
                
                var assetReferenceVistor = new AssetReferenceVistor { References = assetReferences };
                assetReferenceVistor.Visit(obj);

                lock (CachingLock)
                {
                    if (enableCaching)
                    {
                        CachingReferences[obj] = assetReferences;
                    }
                }
            }

            return assetReferences;
        }

        private class AssetReferenceVistor : AssetVisitorBase
        {
            public AssetReferenceVistor()
            {
                References = new List<AssetReferenceLink>();
            }

            public List<AssetReferenceLink> References { get; set; }

            public override void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
            {
                base.VisitArrayItem(array, descriptor, index, item, itemDescriptor);
                var assetReference = item as AssetReference;
                var assetBase = item as AssetBase;
                var attachedReference = item != null ? AttachedReferenceManager.GetAttachedReference(item) : null;
                if (assetReference != null)
                {
                    AddLink(item,
                        (guid, location) =>
                        {
                            var newValue = new AssetReference(guid.HasValue ? guid.Value : assetReference.Id, location);
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
                else if (assetBase != null)
                {
                    AddLink(item,
                        (guid, location) =>
                        {
                            var newValue = new AssetBase(location, assetBase.Asset);
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
                else if (attachedReference != null)
                {
                    AddLink(attachedReference,
                        (guid, location) =>
                        {
                            object newValue = guid.HasValue && guid.Value != Guid.Empty ? AttachedReferenceManager.CreateSerializableVersion(descriptor.ElementType, guid.Value, location) : null;
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
                else if (item is UFile)
                {
                    AddLink(item,
                        (guid, location) =>
                        {
                            var newValue = new UFile(location);
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
                else if (item is UDirectory)
                {
                    AddLink(item,
                        (guid, location) =>
                        {
                            var newValue = new UFile(location);
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
            }

            public override void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
            {
                base.VisitCollectionItem(collection, descriptor, index, item, itemDescriptor);
                var assetReference = item as AssetReference;
                var assetBase = item as AssetBase;
                var attachedReference = item != null ? AttachedReferenceManager.GetAttachedReference(item) : null;
                // TODO force support for IList in CollectionDescriptor
                if (assetReference != null)
                {
                    var list = (IList)collection;
                    AddLink(assetReference, (guid, location) => list[index] = new AssetReference(guid.HasValue ? guid.Value : assetReference.Id, location));
                }
                else if (assetBase != null)
                {
                    var list = (IList)collection;
                    AddLink(assetBase, (guid, location) => list[index] = new AssetBase(location, assetBase.Asset));
                }
                else if (attachedReference != null)
                {
                    var list = (IList)collection;
                    AddLink(attachedReference, (guid, location) => list[index] = guid.HasValue && guid.Value != Guid.Empty ? AttachedReferenceManager.CreateSerializableVersion(descriptor.ElementType, guid.Value, location) : null);
                }
                else if (item is UFile)
                {
                    var list = (IList)collection;
                    AddLink(item, (guid, location) => list[index] = new UFile(location));
                }
                else if (item is UDirectory)
                {
                    var list = (IList)collection;
                    AddLink(item, (guid, location) => list[index] = new UDirectory(location));
                }
            }

            public override void VisitDictionaryKeyValue(object dictionaryObj, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
            {
                base.VisitDictionaryKeyValue(dictionaryObj, descriptor, key, keyDescriptor, value, valueDescriptor);
                var assetReference = value as AssetReference;
                var assetBase = value as AssetBase;
                var attachedReference = value != null ? AttachedReferenceManager.GetAttachedReference(value) : null;
                if (assetReference != null)
                {
                    AddLink(assetReference,
                        (guid, location) =>
                        {
                            var newValue = new AssetReference(guid.HasValue ? guid.Value : assetReference.Id, location);
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
                else if (assetBase != null)
                {
                    AddLink(assetBase,
                        (guid, location) =>
                        {
                            var newValue = new AssetBase(location, assetBase.Asset);
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
                else if (attachedReference != null)
                {
                    AddLink(attachedReference,
                        (guid, location) =>
                        {
                            object newValue = guid.HasValue && guid.Value != Guid.Empty ? AttachedReferenceManager.CreateSerializableVersion(descriptor.ValueType, guid.Value, location) : null;
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
                else if (value is UFile)
                {
                    AddLink(value,
                        (guid, location) =>
                        {
                            var newValue = new UFile(location);
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
                else if (value is UDirectory)
                {
                    AddLink(value,
                        (guid, location) =>
                        {
                            var newValue = new UDirectory(location);
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
            }

            public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
            {
                base.VisitObjectMember(container, containerDescriptor, member, value);
                var assetReference = value as AssetReference;
                var assetBase = value as AssetBase;
                var attachedReference = value != null ? AttachedReferenceManager.GetAttachedReference(value) : null;
                if (assetReference != null)
                {
                    AddLink(assetReference,
                        (guid, location) =>
                        {
                            var newValue = new AssetReference(guid.HasValue ? guid.Value : assetReference.Id, location);
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
                else if (assetBase != null)
                {
                    AddLink(assetBase,
                        (guid, location) =>
                        {
                            var newValue = new AssetBase(location, assetBase.Asset);
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
                else if (attachedReference != null)
                {
                    AddLink(attachedReference,
                        (guid, location) =>
                        {
                            object newValue = guid.HasValue && guid.Value != Guid.Empty ? AttachedReferenceManager.CreateSerializableVersion(member.Type, guid.Value, location) : null;
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
                else if (value is UFile)
                {
                    AddLink(value,
                        (guid, location) =>
                        {
                            var newValue = new UFile(location);
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
                else if (value is UDirectory)
                {
                    AddLink(value,
                        (guid, location) =>
                        {
                            var newValue = new UDirectory(location);
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
            }

            private void AddLink(object value, Func<Guid?, string, object> updateReference)
            {
                References.Add(new AssetReferenceLink(CurrentPath.Clone(), value, updateReference));
            }
        }
    }
}