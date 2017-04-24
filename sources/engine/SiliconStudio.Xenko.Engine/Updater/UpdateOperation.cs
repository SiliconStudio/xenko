// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines an update operation for internal use by <see cref="UpdateEngine"/>.
    /// </summary>
    struct UpdateOperation
    {
        internal UpdateOperationType Type;
        internal UpdatableMember Member;
        internal EnterChecker EnterChecker;

        // TODO: Should we switch to short + short? (note: could be a problem with big arrays)

        // Apply an offset to current object pointer.
        public int AdjustOffset;

        // Note: It is either an offset (blittable struct) or an index into object array (reference types and non blittable struct)
        public int DataOffset;

        // Number of operations to skip if this is null
        public int SkipCountIfNull;
    
        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
