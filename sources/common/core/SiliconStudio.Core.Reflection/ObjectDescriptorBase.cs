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

        protected IMemberDescriptorBase[] members;
        protected Dictionary<string, IMemberDescriptorBase> mapMembers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectDescriptorBase" /> class.
        /// </summary>
        public ObjectDescriptorBase(IAttributeRegistry attributeRegistry, Type type)
        {
            if (attributeRegistry == null) throw new ArgumentNullException(nameof(attributeRegistry));
            if (type == null) throw new ArgumentNullException(nameof(type));

            AttributeRegistry = attributeRegistry;
            Type = type;
            IsCompilerGenerated = AttributeRegistry.GetAttribute<CompilerGeneratedAttribute>(type) != null;
        }

        protected IAttributeRegistry AttributeRegistry { get; }

        public Type Type { get; }

        public IEnumerable<IMemberDescriptorBase> Members => members;

        public int Count => members?.Length ?? 0;

        public bool HasMembers => members?.Length > 0;

        public abstract DescriptorCategory Category { get; }

        public IMemberDescriptorBase this[string name]
        {
            get
            {
                if (mapMembers == null)
                    throw new KeyNotFoundException(name);
                IMemberDescriptorBase member;
                mapMembers.TryGetValue(name, out member);
                return member;
            }
        }

        public abstract void Initialize();

        public bool IsCompilerGenerated { get; }

        public bool Contains(string memberName)
        {
            return mapMembers != null && mapMembers.ContainsKey(memberName);
        }

        protected abstract List<IMemberDescriptorBase> PrepareMembers();

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
