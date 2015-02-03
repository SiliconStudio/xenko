// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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

            public override void VisitObject(object obj, ObjectDescriptor descriptor, bool visitMembers)
            {
                base.VisitObject(obj, descriptor, visitMembers);
                var reference = obj as ContentReference;
                if (reference != null)
                {
                    AddLink(reference, (guid, location) =>
                    {
                        reference.Id = guid.HasValue ? guid.Value : reference.Id;
                        reference.Location = location;
                        return reference;
                    });
                }
                else
                {
                    var attachedReference = AttachedReferenceManager.GetAttachedReference(obj);
                    if (attachedReference != null)
                    {
                        AddLink(new AttachedContentReference(attachedReference), (guid, location) =>
                        {
                            if (guid.HasValue)
                                attachedReference.Id = guid.Value;
                            attachedReference.Url = location;
                            return attachedReference;
                        });
                    }
                }
            }

            public override void VisitArrayItem(Array array, ArrayDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
            {
                base.VisitArrayItem(array, descriptor, index, item, itemDescriptor);
                var assetReference = item as AssetReference;
                if (assetReference != null)
                {
                    AddLink(item,
                        (guid, location) =>
                        {
                            var newValue = AssetReference.New(descriptor.ElementType, guid.HasValue ? guid.Value : assetReference.Id, location);
                            array.SetValue(newValue, index);
                            return newValue;
                        });
                }
                else
                {
                    var assetBase = item as AssetBase;
                    if (assetBase != null)
                    {
                        AddLink(item,
                            (guid, location) =>
                            {
                                var newValue = new AssetBase(location, assetBase.Asset);
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
            }

            public override void VisitCollectionItem(IEnumerable collection, CollectionDescriptor descriptor, int index, object item, ITypeDescriptor itemDescriptor)
            {
                base.VisitCollectionItem(collection, descriptor, index, item, itemDescriptor);
                var assetReference = item as AssetReference;
                // TODO force support for IList in CollectionDescriptor
                if (assetReference != null)
                {
                    var list = (IList)collection;
                    AddLink(assetReference, (guid, location) => list[index] = AssetReference.New(descriptor.ElementType, guid.HasValue ? guid.Value : assetReference.Id, location));
                }
                else
                {
                    var assetBase = item as AssetBase;
                    if (assetBase != null)
                    {
                        var list = (IList)collection;
                        AddLink(assetBase, (guid, location) => list[index] = new AssetBase(location, assetBase.Asset));
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
            }

            public override void VisitDictionaryKeyValue(object dictionaryObj, DictionaryDescriptor descriptor, object key, ITypeDescriptor keyDescriptor, object value, ITypeDescriptor valueDescriptor)
            {
                base.VisitDictionaryKeyValue(dictionaryObj, descriptor, key, keyDescriptor, value, valueDescriptor);
                var assetReference = value as AssetReference;
                if (assetReference != null)
                {
                    AddLink(assetReference,
                        (guid, location) =>
                        {
                            var newValue = AssetReference.New(descriptor.ValueType, guid.HasValue ? guid.Value : assetReference.Id, location);
                            descriptor.SetValue(dictionaryObj, key, newValue);
                            return newValue;
                        });
                }
                else
                {
                    var assetBase = value as AssetBase;
                    if (assetBase != null)
                    {
                        AddLink(assetBase,
                            (guid, location) =>
                            {
                                var newValue = new AssetBase(location, assetBase.Asset);
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
            }

            public override void VisitObjectMember(object container, ObjectDescriptor containerDescriptor, IMemberDescriptor member, object value)
            {
                base.VisitObjectMember(container, containerDescriptor, member, value);
                var assetReference = value as AssetReference;
                if (assetReference != null)
                {
                    AddLink(assetReference,
                        (guid, location) =>
                        {
                            var newValue = AssetReference.New(member.Type, guid.HasValue ? guid.Value : assetReference.Id, location);
                            member.Set(container, newValue);
                            return newValue;
                        });
                }
                else
                {
                    var assetBase = value as AssetBase;
                    if (assetBase != null)
                    {
                        AddLink(assetBase,
                            (guid, location) =>
                            {
                                var newValue = new AssetBase(location, assetBase.Asset);
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
            }

            private void AddLink(object value, Func<Guid?, string, object> updateReference)
            {
                References.Add(new AssetReferenceLink(CurrentPath.Clone(), value, updateReference));
            }
        }
    }
}