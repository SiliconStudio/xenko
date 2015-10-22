// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering.Materials
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