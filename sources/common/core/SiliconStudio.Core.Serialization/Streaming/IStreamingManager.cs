// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Core.Streaming
{
    /// <summary>
    /// Interface for Streaming Manager service.
    /// </summary>
    public interface IStreamingManager
    {
        /// <summary>
        /// Puts request to load given resource up to the maximum residency level.
        /// </summary>
        /// <param name="obj">The streamable resource object.</param>
        void FullyLoadResource(object obj);
    }
}
