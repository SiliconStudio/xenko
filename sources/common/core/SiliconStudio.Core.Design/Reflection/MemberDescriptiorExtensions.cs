// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Extension methods for <see cref="IMemberDescriptor"/>
    /// </summary>
    public static class MemberDescriptiorExtensions
    {
        /// <summary>
        /// Determines whether a member is readonly.
        /// </summary>
        /// <param name="memberDescriptor">The member descriptor.</param>
        /// <returns><c>true</c> if a member is readonly; otherwise, <c>false</c>.</returns>
        public static bool IsReadOnly(this IMemberDescriptor memberDescriptor)
        {
            return memberDescriptor.Mode == DataMemberMode.ReadOnly;
        }

        public static int CompareMetadataTokenWith(this MemberInfo leftMember, MemberInfo rightMember)
        {
            if (leftMember == null)
                return -1;
            if (rightMember == null)
                return 1;

            // If declared in same type, order by metadata token
            if (leftMember.DeclaringType == rightMember.DeclaringType)
                return leftMember.MetadataToken.CompareTo(rightMember.MetadataToken);

            // Otherwise, put base class first
            return (leftMember.DeclaringType.IsSubclassOf(rightMember.DeclaringType)) ? 1 : -1;
        }
    }
}