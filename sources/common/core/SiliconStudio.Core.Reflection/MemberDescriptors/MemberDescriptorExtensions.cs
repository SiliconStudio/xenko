// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Reflection;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Extension methods for <see cref="IMemberDescriptor"/>
    /// </summary>
    public static class MemberDescriptorExtensions
    {
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
