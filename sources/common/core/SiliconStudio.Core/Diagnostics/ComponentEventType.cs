// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
