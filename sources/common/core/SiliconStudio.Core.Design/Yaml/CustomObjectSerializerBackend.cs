// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using IMemberDescriptor = SiliconStudio.Core.Reflection.IMemberDescriptor;
using ITypeDescriptor = SiliconStudio.Core.Reflection.ITypeDescriptor;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Internal class used when serializing/deserializing an object.
    /// </summary>
    // TODO: this class should be internal, and asset-specific code (ie. override, possibly collection with ids) should move in an inheriting class in Assets
    public class CustomObjectSerializerBackend : DefaultObjectSerializerBackend
    {
        private readonly ITypeDescriptorFactory typeDescriptorFactory;
        private ITypeDescriptor cachedDescriptor;
        private static readonly PropertyKey<ObjectPath> MemberPathKey = new PropertyKey<ObjectPath>("MemberPath", typeof(CustomObjectSerializerBackend));
        public static readonly PropertyKey<Dictionary<ObjectPath, OverrideType>> OverrideDictionaryKey = new PropertyKey<Dictionary<ObjectPath, OverrideType>>("OverrideDictionary", typeof(CustomObjectSerializerBackend));

        public CustomObjectSerializerBackend(ITypeDescriptorFactory typeDescriptorFactory)
        {
            if (typeDescriptorFactory == null)
                throw new ArgumentNullException(nameof(typeDescriptorFactory));
            this.typeDescriptorFactory = typeDescriptorFactory;
        }

        public override object ReadMemberValue(ref ObjectContext objectContext, IMemberDescriptor memberDescriptor, object memberValue, Type memberType)
        {
            var memberObjectContext = new ObjectContext(objectContext.SerializerContext, memberValue, objectContext.SerializerContext.FindTypeDescriptor(memberType));

            var member = memberDescriptor as MemberDescriptorBase;
            if (member != null && objectContext.Settings.Attributes.GetAttribute<NonIdentifiableCollectionItemsAttribute>(member.MemberInfo) != null)
            {
                memberObjectContext.Properties.Add(CollectionWithIdsSerializerBase.NonIdentifiableCollectionItemsKey, true);
            }

            var path = GetCurrentPath(ref objectContext, true);
            path.PushMember(memberDescriptor.Name);
            SetCurrentPath(ref memberObjectContext, path);

            var result = ReadYaml(ref memberObjectContext);
            return result;
        }

        public override void WriteMemberValue(ref ObjectContext objectContext, IMemberDescriptor memberDescriptor, object memberValue, Type memberType)
        {
            var memberObjectContext = new ObjectContext(objectContext.SerializerContext, memberValue, objectContext.SerializerContext.FindTypeDescriptor(memberType));

            var member = memberDescriptor as MemberDescriptorBase;
            if (member != null && objectContext.Settings.Attributes.GetAttribute<NonIdentifiableCollectionItemsAttribute>(member.MemberInfo) != null)
            {
                memberObjectContext.Properties.Add(CollectionWithIdsSerializerBase.NonIdentifiableCollectionItemsKey, true);
            }

            var path = GetCurrentPath(ref objectContext, true);
            path.PushMember(memberDescriptor.Name);
            SetCurrentPath(ref memberObjectContext, path);

            WriteYaml(ref memberObjectContext);
        }

        public override string ReadMemberName(ref ObjectContext objectContext, string memberName, out bool skipMember)
        {
            var objectType = objectContext.Instance.GetType();

            OverrideType[] overrideTypes;
            var realMemberName = TrimAndParseOverride(memberName, out overrideTypes);

            // For member names, we have a single override, so we always take the last one of the array (In case of legacy property serialized with ~Name)
            var overrideType = overrideTypes[overrideTypes.Length - 1];
            if (overrideType != OverrideType.Base)
            {
                if (cachedDescriptor == null || cachedDescriptor.Type != objectType)
                {
                    cachedDescriptor = typeDescriptorFactory.Find(objectType);
                }
                var memberDescriptor = cachedDescriptor[realMemberName];

                Dictionary<ObjectPath, OverrideType> overrides;
                if (!objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out overrides))
                {
                    overrides = new Dictionary<ObjectPath, OverrideType>();
                    objectContext.SerializerContext.Properties.Add(OverrideDictionaryKey, overrides);
                }

                var path = GetCurrentPath(ref objectContext, true);
                path.PushMember(realMemberName);
                overrides.Add(path, overrideType);

                objectContext.Instance.SetOverride(memberDescriptor, overrideType);
            }

            var resultMemberName = base.ReadMemberName(ref objectContext, realMemberName, out skipMember);
            // If ~Id was not found as a member, don't generate an error, as we may have switched an object
            // to NonIdentifiable but we don't want to write an upgrader for this
            if (!IdentifiableHelper.IsIdentifiable(objectType) && memberName == IdentifiableHelper.YamlSpecialId)
            {
                skipMember = true;
            }
            return resultMemberName;
        }

        public override void WriteMemberName(ref ObjectContext objectContext, IMemberDescriptor member, string memberName)
        {
            // Replace the key with SiliconStudio.Core.Reflection IMemberDescriptor
            // Cache previous 
            if (member != null)
            {
                var customDescriptor = (IMemberDescriptor)member.Tag;
                if (customDescriptor == null)
                {
                    customDescriptor = typeDescriptorFactory.Find(objectContext.Instance.GetType())[memberName];
                    member.Tag = customDescriptor;
                }

                if (customDescriptor != null)
                {
                    Dictionary<ObjectPath, OverrideType> overrides;
                    if (objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out overrides))
                    {
                        var path = GetCurrentPath(ref objectContext, true);
                        path.PushMember(memberName);

                        OverrideType overrideType;
                        if (overrides.TryGetValue(path, out overrideType))
                        {
                            if ((overrideType & OverrideType.New) != 0)
                            {
                                memberName += Override.PostFixNew;
                            }
                            if ((overrideType & OverrideType.Sealed) != 0)
                            {
                                memberName += Override.PostFixSealed;
                            }
                        }
                    }
                }
            }

            base.WriteMemberName(ref objectContext, member, memberName);
        }

        public override object ReadCollectionItem(ref ObjectContext objectContext, object value, Type itemType, int index)
        {
            var path = GetCurrentPath(ref objectContext, true);
            path.PushIndex(index);
            var itemObjectContext = new ObjectContext(objectContext.SerializerContext, value, objectContext.SerializerContext.FindTypeDescriptor(itemType));
            SetCurrentPath(ref itemObjectContext, path);
            return ReadYaml(ref itemObjectContext);
        }

        public override void WriteCollectionItem(ref ObjectContext objectContext, object item, Type itemType, int index)
        {
            var path = GetCurrentPath(ref objectContext, true);
            path.PushIndex(index);
            var itemObjectcontext = new ObjectContext(objectContext.SerializerContext, item, objectContext.SerializerContext.FindTypeDescriptor(itemType));
            SetCurrentPath(ref itemObjectcontext, path);
            WriteYaml(ref itemObjectcontext);
        }

        public override object ReadDictionaryKey(ref ObjectContext objectContext, Type keyType)
        {
            var key = objectContext.Reader.Peek<Scalar>();
            OverrideType[] overrideTypes;
            var keyName = TrimAndParseOverride(key.Value, out overrideTypes);
            key.Value = keyName;

            var keyValue = base.ReadDictionaryKey(ref objectContext, keyType);

            if (overrideTypes[0] != OverrideType.Base)
            {
                Dictionary<ObjectPath, OverrideType> overrides;
                if (!objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out overrides))
                {
                    overrides = new Dictionary<ObjectPath, OverrideType>();
                    objectContext.SerializerContext.Properties.Add(OverrideDictionaryKey, overrides);
                }

                var path = GetCurrentPath(ref objectContext, true);
                ItemId id;
                if (ObjectPath.IsCollectionWithIdType(objectContext.Descriptor.Type, keyValue, out id))
                {
                    path.PushItemId(id);
                }
                else
                {
                    path.PushIndex(keyValue);
                }
                overrides.Add(path, overrideTypes[0]);
            }

            if (overrideTypes.Length > 1 && overrideTypes[1] != OverrideType.Base)
            {
                ItemId id;
                if (ObjectPath.IsCollectionWithIdType(objectContext.Descriptor.Type, keyValue, out id))
                {
                    Dictionary<ObjectPath, OverrideType> overrides;
                    if (!objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out overrides))
                    {
                        overrides = new Dictionary<ObjectPath, OverrideType>();
                        objectContext.SerializerContext.Properties.Add(OverrideDictionaryKey, overrides);
                    }

                    var path = GetCurrentPath(ref objectContext, true);
                    path.PushIndex(keyValue);
                    overrides.Add(path, overrideTypes[0]);
                }
            }

            return keyValue;
        }

        public override void WriteDictionaryKey(ref ObjectContext objectContext, object key, Type keyType)
        {
            Dictionary<ObjectPath, OverrideType> overrides;
            if (objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out overrides))
            {
                var itemPath = GetCurrentPath(ref objectContext, true);
                ObjectPath keyPath = null;
                ItemId id;
                if (ObjectPath.IsCollectionWithIdType(objectContext.Descriptor.Type, key, out id))
                {
                    keyPath = itemPath.Clone();
                    keyPath.PushIndex(key);
                    itemPath.PushItemId(id);
                }
                else
                {
                    itemPath.PushIndex(key);
                }
                OverrideType overrideType;
                if (overrides.TryGetValue(itemPath, out overrideType))
                {
                    if ((overrideType & OverrideType.New) != 0)
                    {
                        objectContext.SerializerContext.Properties.Set(ItemIdSerializerBase.OverrideInfoKey, Override.PostFixNew.ToString());
                    }
                    if ((overrideType & OverrideType.Sealed) != 0)
                    {
                        objectContext.SerializerContext.Properties.Set(ItemIdSerializerBase.OverrideInfoKey, Override.PostFixSealed.ToString());
                    }
                }
                if (keyPath != null && overrides.TryGetValue(keyPath, out overrideType))
                {
                    if ((overrideType & OverrideType.New) != 0)
                    {
                        objectContext.SerializerContext.Properties.Set(KeyWithIdSerializer.OverrideKeyInfoKey, Override.PostFixNew.ToString());
                    }
                    if ((overrideType & OverrideType.Sealed) != 0)
                    {
                        objectContext.SerializerContext.Properties.Set(KeyWithIdSerializer.OverrideKeyInfoKey, Override.PostFixSealed.ToString());
                    }
                }
            }
            base.WriteDictionaryKey(ref objectContext, key, keyType);
        }

        public override object ReadDictionaryValue(ref ObjectContext objectContext, Type valueType, object key)
        {
            var path = GetCurrentPath(ref objectContext, true);
            ItemId id;
            if (ObjectPath.IsCollectionWithIdType(objectContext.Descriptor.Type, key, out id))
            {
                path.PushItemId(id);
            }
            else
            {
                path.PushIndex(key);
            }
            var valueObjectContext = new ObjectContext(objectContext.SerializerContext, null, objectContext.SerializerContext.FindTypeDescriptor(valueType));
            SetCurrentPath(ref valueObjectContext, path);
            return ReadYaml(ref valueObjectContext);
        }

        public override void WriteDictionaryValue(ref ObjectContext objectContext, object key, object value, Type valueType)
        {
            var path = GetCurrentPath(ref objectContext, true);
            ItemId id;
            if (ObjectPath.IsCollectionWithIdType(objectContext.Descriptor.Type, key, out id))
            {
                path.PushItemId(id);
            }
            else
            {
                path.PushIndex(key);
            }
            var itemObjectcontext = new ObjectContext(objectContext.SerializerContext, value, objectContext.SerializerContext.FindTypeDescriptor(valueType));
            SetCurrentPath(ref itemObjectcontext, path);
            WriteYaml(ref itemObjectcontext);
        }

        private static ObjectPath GetCurrentPath(ref ObjectContext objectContext, bool clone)
        {
            ObjectPath path;
            path = objectContext.Properties.TryGetValue(MemberPathKey, out path) ? path : new ObjectPath();
            if (clone)
            {
                path = path.Clone();
            }
            return path;
        }

        private static void SetCurrentPath(ref ObjectContext objectContext, ObjectPath path)
        {
            objectContext.Properties.Set(MemberPathKey, path);
        }

        internal static string TrimAndParseOverride(string name, out OverrideType[] overrideTypes)
        {
            var split = name.Split('~');

            overrideTypes = new OverrideType[split.Length];
            int i = 0;
            var trimmedName = string.Empty;
            foreach (var namePart in split)
            {
                var realName = namePart.Trim(Override.PostFixSealed, Override.PostFixNew);

                var overrideType = OverrideType.Base;
                if (realName.Length != namePart.Length)
                {
                    if (namePart.Contains(Override.PostFixNewSealed) || namePart.EndsWith(Override.PostFixNewSealedAlt))
                    {
                        overrideType = OverrideType.New | OverrideType.Sealed;
                    }
                    else if (namePart.EndsWith(Override.PostFixNew))
                    {
                        overrideType = OverrideType.New;
                    }
                    else if (namePart.EndsWith(Override.PostFixSealed))
                    {
                        overrideType = OverrideType.Sealed;
                    }
                }
                overrideTypes[i] = overrideType;
                if (i > 0)
                    trimmedName += '~';
                trimmedName += realName;
                ++i;
            }
            return trimmedName;
        }
    }
}
