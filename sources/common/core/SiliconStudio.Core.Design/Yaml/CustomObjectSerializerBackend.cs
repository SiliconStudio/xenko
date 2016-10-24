// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading;
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
        private static readonly object MemberPathKey = new object();
        public static readonly object OverrideDictionaryKey = new object();

        public CustomObjectSerializerBackend(ITypeDescriptorFactory typeDescriptorFactory)
        {
            if (typeDescriptorFactory == null) throw new ArgumentNullException(nameof(typeDescriptorFactory));
            this.typeDescriptorFactory = typeDescriptorFactory;
        }

        public override object ReadMemberValue(ref ObjectContext objectContext, IMemberDescriptor memberDescriptor, object memberValue, Type memberType)
        {
            var memberObjectContext = new ObjectContext(objectContext.SerializerContext, memberValue, objectContext.SerializerContext.FindTypeDescriptor(memberType));

            var member = memberDescriptor as MemberDescriptorBase;
            if (member != null && objectContext.Settings.Attributes.GetAttribute<NonIdentifiableCollectionItemsAttribute>(member.MemberInfo) != null)
            {
                memberObjectContext.Properties.Add(NonIdentifiableCollectionItemsAttribute.Key, true);
            }

            var memberPath = GetCurrentPath(ref objectContext, true);
            memberPath.Push(memberDescriptor);
            SetCurrentPath(ref memberObjectContext, memberPath);
            var result = ReadYaml(ref memberObjectContext);
            return result;
        }

        public override void WriteMemberValue(ref ObjectContext objectContext, IMemberDescriptor memberDescriptor, object memberValue, Type memberType)
        {
            var memberObjectContext = new ObjectContext(objectContext.SerializerContext, memberValue, objectContext.SerializerContext.FindTypeDescriptor(memberType));

            var member = memberDescriptor as MemberDescriptorBase;
            if (member != null && objectContext.Settings.Attributes.GetAttribute<NonIdentifiableCollectionItemsAttribute>(member.MemberInfo) != null)
            {
                memberObjectContext.Properties.Add(NonIdentifiableCollectionItemsAttribute.Key, true);
            }

            WriteYaml(ref memberObjectContext);
        }

        public override string ReadMemberName(ref ObjectContext objectContext, string memberName, out bool skipMember)
        {
            var objectType = objectContext.Instance.GetType();

            OverrideType overrideType;
            var realMemberName = TrimAndParseOverride(memberName, out overrideType);

            if (overrideType != OverrideType.Base)
            {
                if (cachedDescriptor == null || cachedDescriptor.Type != objectType)
                {
                    cachedDescriptor = typeDescriptorFactory.Find(objectType);
                }
                var memberDescriptor = cachedDescriptor[realMemberName];

                object property;
                if (!objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out property))
                {
                    property = new Dictionary<MemberPath, OverrideType>();
                    objectContext.SerializerContext.Properties.Add(OverrideDictionaryKey, property);
                }
                var overrides = (Dictionary<MemberPath, OverrideType>)property;

                var memberPath = GetCurrentPath(ref objectContext, true);
                memberPath.Push(memberDescriptor);
                overrides.Add(memberPath, overrideType);

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
                    var overrideType = objectContext.Instance.GetOverride(customDescriptor);
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

            base.WriteMemberName(ref objectContext, member, memberName);
        }

        public override object ReadCollectionItem(ref ObjectContext objectContext, object value, Type itemType, int index)
        {
            var memberPath = GetCurrentPath(ref objectContext, true);
            memberPath.Push((CollectionDescriptor)objectContext.Descriptor, index);
            var itemObjectContext = new ObjectContext(objectContext.SerializerContext, value, objectContext.SerializerContext.FindTypeDescriptor(itemType));
            SetCurrentPath(ref itemObjectContext, memberPath);
            return ReadYaml(ref itemObjectContext);
        }

        public override object ReadDictionaryKey(ref ObjectContext objectContext, Type keyType)
        {
            var key = objectContext.Reader.Peek<Scalar>();
            OverrideType overrideType;
            var keyName = TrimAndParseOverride(key.Value, out overrideType);
            key.Value = keyName;

            var keyValue = base.ReadDictionaryKey(ref objectContext, keyType);

            if (overrideType != OverrideType.Base)
            {
                object property;
                if (!objectContext.SerializerContext.Properties.TryGetValue(OverrideDictionaryKey, out property))
                {
                    property = new Dictionary<MemberPath, OverrideType>();
                    objectContext.SerializerContext.Properties.Add(OverrideDictionaryKey, property);
                }
                var overrides = (Dictionary<MemberPath, OverrideType>)property;

                var memberPath = GetCurrentPath(ref objectContext, true);
                memberPath.Push(objectContext.Descriptor, keyValue);
                overrides.Add(memberPath, overrideType);
            }

            return keyValue;
        }

        public override void WriteDictionaryKey(ref ObjectContext objectContext, object key, Type keyType)
        {
            base.WriteDictionaryKey(ref objectContext, key, keyType);
        }

        public override object ReadDictionaryValue(ref ObjectContext objectContext, Type valueType, object key)
        {
            var memberPath = GetCurrentPath(ref objectContext, true);
            memberPath.Push((DictionaryDescriptor)objectContext.Descriptor, key);
            var valueObjectContext = new ObjectContext(objectContext.SerializerContext, null, objectContext.SerializerContext.FindTypeDescriptor(valueType));
            SetCurrentPath(ref valueObjectContext, memberPath);
            return ReadYaml(ref valueObjectContext);
        }

        private static MemberPath GetCurrentPath(ref ObjectContext objectContext, bool clone)
        {
            object property;
            var memberPath = !objectContext.Properties.TryGetValue(MemberPathKey, out property) ? new MemberPath() : (MemberPath)property;
            if (clone)
            {
                memberPath = memberPath.Clone();
            }
            return memberPath;
        }

        private static void SetCurrentPath(ref ObjectContext objectContext, MemberPath path)
        {
            objectContext.Properties[MemberPathKey] = path;
        }

        private static string TrimAndParseOverride(string name, out OverrideType overrideType)
        {
            var realName = name.Trim(Override.PostFixSealed, Override.PostFixNew);

            overrideType = OverrideType.Base;
            if (realName.Length != name.Length)
            {
                if (name.Contains(Override.PostFixNewSealed) || name.EndsWith(Override.PostFixNewSealedAlt))
                {
                    overrideType = OverrideType.New | OverrideType.Sealed;
                }
                else if (name.EndsWith(Override.PostFixNew))
                {
                    overrideType = OverrideType.New;
                }
                else if (name.EndsWith(Override.PostFixSealed))
                {
                    overrideType = OverrideType.Sealed;
                }
            }
            return realName;
        }
    }
}
