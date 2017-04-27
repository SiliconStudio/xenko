// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.Diagnostics
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public enum ComponentEventType
    {
        /// <summary>
        /// ComponentBase constructor event.
        /// </summary>
        Instantiate = 0,

        /// <summary>
        /// ComponentBase.Destroy() event.
        /// </summary>
        Destroy = 1,

        /// <summary>
        /// IReferencable.AddReference() event.
        /// </summary>
        AddReference = 2,

        /// <summary>
        /// IReferenceable.Release() event.
        /// </summary>
        Release = 3,
    }
}
