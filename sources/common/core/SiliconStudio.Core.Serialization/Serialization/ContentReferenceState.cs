// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Core.Serialization
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