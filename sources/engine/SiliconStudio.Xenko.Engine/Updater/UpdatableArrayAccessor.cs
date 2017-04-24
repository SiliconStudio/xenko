// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines how to get and set an array value for the <see cref="UpdateEngine"/>.
    /// </summary>
    public class UpdatableArrayAccessor<T> : UpdatableField<T>
    {
        public UpdatableArrayAccessor(int index) : base(0)
        {
            Offset = UpdateEngineHelper.ArrayFirstElementOffset + index * Size;
        }

        /// <inheritdoc/>
        public override EnterChecker CreateEnterChecker()
        {
            // Compute index
            var index = (Offset - UpdateEngineHelper.ArrayFirstElementOffset)/Size;

            // Expect an array at least index + 1 items
            return new ListEnterChecker<T>(index + 1);
        }
    }
}
