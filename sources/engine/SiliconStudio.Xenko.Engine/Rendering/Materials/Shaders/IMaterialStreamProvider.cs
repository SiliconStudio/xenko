// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// An interface to query streams used by materials. 
    /// </summary>
    /// <remarks>
    /// This is not an exhaustive list but is used to allow to display a specific map in the editor.
    /// </remarks>
    public interface IMaterialStreamProvider
    {
        /// <summary>
        /// Gets the streams used by a material
        /// </summary>
        /// <returns>IEnumerable&lt;MaterialStream&gt;.</returns>
        IEnumerable<MaterialStreamDescriptor> GetStreams();
    }
}
