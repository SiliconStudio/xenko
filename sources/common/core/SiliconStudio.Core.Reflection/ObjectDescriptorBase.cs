using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Default implementation of a <see cref="ITypeDescriptor"/>.
    /// </summary>
    public abstract class ObjectDescriptorBase : ITypeDescriptor
    {
        protected static readonly string SystemCollectionsNamespace = typeof(int).Namespace;

        protected IMemberDescriptor[] members;
        protected Dictionary<string, IMemberDescriptor> mapMembers;
        private HashSet<string> remapMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptorBase" /> class.
        /// </summary>
        protected ObjectDescriptorBase(IAttributeRegistry attributeRegistry, Type type)
        {
            if (attributeRegistry == null) throw new ArgumentNullException(nameof(attributeRegistry));
            if (type == null) throw new ArgumentNullException(nameof(type));

            AttributeRegistry = attributeRegistry;
            Type = type;
            IsCompilerGenerated = AttributeRegistry.GetAttribute<CompilerGeneratedAttribute>(type) != null;
        }

        protected IAttributeRegistry AttributeRegistry { get; }

        public Type Type { get; }

        public IEnumerable<IMemberDescriptor> Members => members;

        public int Count => members?.Length ?? 0;

        public bool HasMembers => members?.Length > 0;

        public abstract DescriptorCategory Category { get; }

        public bool IsMemberRemapped(string name)
        {
            return remapMembers != null && remapMembers.Contains(name);
        }

        public IMemberDescriptor this[string name]
        {
            get
            {
                if (mapMembers == null)
                    throw new KeyNotFoundException(name);
                IMemberDescriptor member;
                mapMembers.TryGetValue(name, out member);
                return member;
            }
        }

        public virtual void Initialize(IComparer<object> keyComparer)
        {
            if (members != null)
                return;

            var memberList = PrepareMembers();

            // Sort members by name
            // This is to make sure that properties/fields for an object 
            // are always displayed in the same order
            if (keyComparer != null)
            {
                memberList.Sort(keyComparer);
            }

            // Free the member list
            members = memberList.ToArray();

            // If no members found, we don't need to build a dictionary map
            if (members.Length <= 0)
                return;

            mapMembers = new Dictionary<string, IMemberDescriptor>(members.Length);

            foreach (var member in members)
            {
                IMemberDescriptor existingMember;
                if (mapMembers.TryGetValue(member.Name, out existingMember))
                {
                    throw new InvalidOperationException("Failed to get ObjectDescriptor for type [{0}]. The member [{1}] cannot be registered as a member with the same name is already registered [{2}]".ToFormat(Type.FullName, member, existingMember));
                }

                mapMembers.Add(member.Name, member);

                // If there is any alternative names, register them
                if (member.AlternativeNames != null)
                {
                    foreach (var alternateName in member.AlternativeNames)
                    {
                        if (mapMembers.TryGetValue(alternateName, out existingMember))
                        {
                            throw new InvalidOperationException($"Failed to get ObjectDescriptor for type [{Type.FullName}]. The member [{member}] cannot be registered as a member with the same name [{alternateName}] is already registered [{existingMember}]");
                        }
                        if (remapMembers == null)
                        {
                            remapMembers = new HashSet<string>();
                        }

                        mapMembers[alternateName] = member;
                        remapMembers.Add(alternateName);
                    }
                }
            }
        }

        public bool IsCompilerGenerated { get; }

        public bool Contains(string memberName)
        {
            return mapMembers != null && mapMembers.ContainsKey(memberName);
        }

        protected abstract List<IMemberDescriptor> PrepareMembers();

        protected bool IsMemberToVisit(MemberInfo memberInfo)
        {
            // Remove all SyncRoot from members
            if (memberInfo is PropertyInfo && memberInfo.Name == "SyncRoot" && memberInfo.DeclaringType != null && (memberInfo.DeclaringType.Namespace ?? string.Empty).StartsWith(SystemCollectionsNamespace))
            {
                return false;
            }

            Type memberType = null;
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                memberType = fieldInfo.FieldType;
            }
            else
            {
                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    memberType = propertyInfo.PropertyType;
                }
            }

            if (memberType  != null)
            {
                if (typeof(Delegate).IsAssignableFrom(memberType))
                {
                    return false;
                }
            }


            // Member is not displayed if there is a YamlIgnore attribute on it
            if (AttributeRegistry.GetAttribute<DataMemberIgnoreAttribute>(memberInfo) != null)
                return false;

            return true;
        }
    }
}
