// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines how to set and get values from a field of a given type for the <see cref="UpdateEngine"/>.
    /// </summary>
    public class UpdatableField<T> : UpdatableField
    {
        public UpdatableField(int offset)
        {
            Offset = offset;
            Size = Interop.SizeOf<T>();
        }

        /// <inheritdoc/>
        public override Type MemberType
        {
            get { return typeof(T); }
        }

        /// <inheritdoc/>
        public override void SetStruct(IntPtr obj, object data)
        {
#if IL
            // Note: IL is injected by UpdateEngineProcessor
            // Target
            ldarg obj

            // Load source (unboxed pointer)
            ldarg data
            unbox !T

            // *obj = *source
            cpobj !T
#endif
            throw new NotImplementedException();
        }
    }
}
