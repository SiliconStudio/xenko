// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SharpYaml.Serialization;
using SharpYaml.Serialization.Serializers;
using SiliconStudio.Core.Reflection;
using IMemberDescriptor = SiliconStudio.Core.Reflection.IMemberDescriptor;
using ITypeDescriptor = SiliconStudio.Core.Reflection.ITypeDescriptor;

namespace SiliconStudio.Core.Yaml
{
    /// <summary>
    /// Internal class used when serializing/deserializing an object.
    /// </summary>
    internal class OverrideKeyMappingTransform : DefaultObjectSerializerBackend
    {
        private readonly ITypeDescriptorFactory typeDescriptorFactory;
        private ITypeDescriptor cachedDescriptor;

        private const char PostFixSealed = '!';

        private const char PostFixNew = '*';

        private const string PostFixNewSealed = "*!";

        private const string PostFixNewSealedAlt = "!*";

        public OverrideKeyMappingTransform(ITypeDescriptorFactory typeDescriptorFactory)
        {
            if (typeDescriptorFactory == null) throw new ArgumentNullException("typeDescriptorFactory");
            this.typeDescriptorFactory = typeDescriptorFactory;
        }

        public override void WriteMemberName(ref ObjectContext objectContext, SharpYaml.Serialization.IMemberDescriptor member, string memberName)
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
                        memberName += PostFixNew;
                    }
                    if ((overrideType & OverrideType.Sealed) != 0)
                    {
                        memberName += PostFixSealed;
                    }
                }
            }

            base.WriteMemberName(ref objectContext, member, memberName);
        }

        public override string ReadMemberName(ref ObjectContext objectContext, string memberName)
        {
            var newMemberName = memberName.Trim(PostFixSealed, PostFixNew);

            if (newMemberName.Length != memberName.Length)
            {
                var overrideType = OverrideType.Base;
                if (memberName.Contains(PostFixNewSealed) || memberName.EndsWith(PostFixNewSealedAlt))
                {
                    overrideType = OverrideType.New | OverrideType.Sealed;
                }
                else if (memberName.EndsWith(PostFixNew))
                {
                    overrideType = OverrideType.New;
                }
                else if (memberName.EndsWith(PostFixSealed))
                {
                    overrideType = OverrideType.Sealed;
                }

                if (overrideType != OverrideType.Base)
                {
                    var objectType = objectContext.Instance.GetType();
                    if (cachedDescriptor == null || cachedDescriptor.Type != objectType)
                    {
                        cachedDescriptor = typeDescriptorFactory.Find(objectType);
                    }
                    var memberDescriptor = cachedDescriptor[newMemberName];
                    objectContext.Instance.SetOverride(memberDescriptor, overrideType);
                }
            }

            return base.ReadMemberName(ref objectContext, newMemberName);
        }
    }
}