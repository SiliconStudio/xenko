// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Core.Serialization.Contents
{
    public enum ContentReferenceState
    {
        /// <summary>
        /// Never try to load the data reference.
        /// </summary>
        NeverLoad = 0,

        /// <summary>
        /// Data reference has already been loaded.
        /// </summary>
        Loaded = 3,

        /// <summary>
        /// Data reference has been set to a new value by the user.
        /// It will be changed to <see cref="Loaded"/> as soon as it has been written by the <see cref="ContentManager"/>.
        /// </summary>
        Modified = 5,
    }
}
