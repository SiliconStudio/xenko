// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Core.Reflection
{
    /// <summary>
    /// Extensions methods for <see cref="MemberPath"/>
    /// </summary>
    public static class MemberPathExtensions
    {
        /// <summary>
        /// Get the shadow attributes of the node objects along the member path.
        /// </summary>
        /// <typeparam name="T">The attribute type</typeparam>
        /// <param name="memberPath">The member path</param>
        /// <param name="rootObject">The root object to search in</param>
        /// <param name="attributeKey">The key specifying the shadow attributes to search for</param>
        /// <returns>The shadow attributes of the path nodes</returns>
        public static IEnumerable<T> GetNodeAttributes<T>(this MemberPath memberPath, object rootObject, PropertyKey<T> attributeKey)
        {
            foreach (var node in memberPath.GetNodes(rootObject))
            {
                if(node.Object == null)
                    continue;

                T value;

                // first return the attribute on the referencing object itself.
                if (node.Object.TryGetDynamicProperty(ThisDescriptor.Default, attributeKey, out value))
                    yield return value;

                // then return the attribute on its member.
                if (node.Descriptor != null && node.Object.TryGetDynamicProperty(node.Descriptor, attributeKey, out value))
                    yield return value;
            }
        }

        /// <summary>
        /// Get the override attributes of the node objects along the member path.
        /// </summary>
        /// <param name="memberPath">The member path</param>
        /// <param name="rootObject">The root object to search in</param>
        /// <returns>The override attributes of the path nodes</returns>
        public static IEnumerable<OverrideType> GetNodeOverrides(this MemberPath memberPath, object rootObject)
        {
            return GetNodeAttributes(memberPath, rootObject, Override.OverrideKey);
        }
    }
}