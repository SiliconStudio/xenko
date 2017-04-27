// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Defines an update compiled by <see cref="UpdateEngine.Compile"/>
    /// for subsequent uses by <see cref="UpdateEngine.Run"/>.
    /// </summary>
    public struct CompiledUpdate
    {
        /// <summary>
        /// Stores the list of update operations.
        /// </summary>
        internal UpdateOperation[] UpdateOperations;

        /// <summary>
        /// Stores the list of pre-allocated objects for non-blittable struct unboxing.
        /// </summary>
        internal object[] TemporaryObjects;
    }
}
