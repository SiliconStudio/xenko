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
    internal class CustomObjectSerializerBackend : DefaultObjectSerializerBackend
    {
        private readonly ITypeDescriptorFactory typeDescriptorFactory;
        private ITypeDescriptor cachedDescriptor;


        public CustomObjectSerializerBackend(ITypeDescriptorFactory typeDescriptorFactory)
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

        public override string ReadMemberName(ref ObjectContext objectContext, string memberName, out bool skipMember)
        {
            var newMemberName = memberName.Trim(Override.PostFixSealed, Override.PostFixNew);
            var objectType = objectContext.Instance.GetType();

            if (newMemberName.Length != memberName.Length)
            {
                var overrideType = OverrideType.Base;
                if (memberName.Contains(Override.PostFixNewSealed) || memberName.EndsWith(Override.PostFixNewSealedAlt))
                {
                    overrideType = OverrideType.New | OverrideType.Sealed;
                }
                else if (memberName.EndsWith(Override.PostFixNew))
                {
                    overrideType = OverrideType.New;
                }
                else if (memberName.EndsWith(Override.PostFixSealed))
                {
                    overrideType = OverrideType.Sealed;
                }

                if (overrideType != OverrideType.Base)
                {
                    if (cachedDescriptor == null || cachedDescriptor.Type != objectType)
                    {
                        cachedDescriptor = typeDescriptorFactory.Find(objectType);
                    }
                    var memberDescriptor = cachedDescriptor[newMemberName];
                    objectContext.Instance.SetOverride(memberDescriptor, overrideType);
                }
            }

            var resultMemberName = base.ReadMemberName(ref objectContext, newMemberName, out skipMember);
            // If ~Id was not found as a member, don't generate an error, as we may have switched an object
            // to NonIdentifiable but we don't want to write an upgrader for this
            if (!IdentifiableHelper.IsIdentifiable(objectType) && memberName == IdentifiableHelper.YamlSpecialId)
            {
                skipMember = true;
            }
            return resultMemberName;
        }
    }
}