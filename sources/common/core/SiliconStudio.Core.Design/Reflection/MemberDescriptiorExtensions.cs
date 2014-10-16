// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
    }
}