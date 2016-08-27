// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// Describes how a child element is positioned in depth or stretched within a parent's layout slot.
    /// </summary>
    public enum DepthAlignment
    {
        /// <summary>
        /// The child element is aligned to the front of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the front of the parent's layout slot.</userdoc>
        Front,
        /// <summary>
        /// The child element is aligned to the center of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the center of the parent's layout slot.</userdoc>
        Center,
        /// <summary>
        /// The child element is aligned to the back of the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element is aligned to the back of the parent's layout slot.</userdoc>
        Back,
        /// <summary>
        /// The child element stretches to fill the parent's layout slot.
        /// </summary>
        /// <userdoc>The child element stretches to fill the parent's layout slot.</userdoc>
        Stretch,
    }
}