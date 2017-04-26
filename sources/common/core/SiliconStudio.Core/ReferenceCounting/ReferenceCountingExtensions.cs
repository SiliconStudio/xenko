// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Runtime.CompilerServices;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.ReferenceCounting
{
    internal static class ReferenceCountingExtensions
    {
        /// <summary>
        /// Increments the reference count of this instance.
        /// </summary>
        /// <returns>The method returns the new reference count.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddReferenceInternal([NotNull] this IReferencable referencable)
        {
            return referencable.AddReference();
        }

        /// <summary>
        /// Decrements the reference count of this instance.
        /// </summary>
        /// <returns>The method returns the new reference count.</returns>
        /// <remarks>When the reference count is going to 0, the component should release/dispose dependents objects.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReleaseInternal([NotNull] this IReferencable referencable)
        {
            return referencable.Release();
        }
    }
}
