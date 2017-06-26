// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A resource allocated by <see cref="GraphicsResourceAllocator"/> providing allocation informations.
    /// </summary>
    public sealed class GraphicsResourceLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsResourceLink"/> class.
        /// </summary>
        /// <param name="resource">The graphics resource.</param>
        /// <exception cref="System.ArgumentNullException">resource</exception>
        internal GraphicsResourceLink(GraphicsResourceBase resource)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
        }

        /// <summary>
        /// The graphics resource.
        /// </summary>
        public GraphicsResourceBase Resource { get; }

        /// <summary>
        /// Gets the last time this resource was accessed.
        /// </summary>
        /// <value>The last access time.</value>
        public DateTime LastAccessTime { get; internal set; }

        /// <summary>
        /// Gets the total count of access to this resource (include Increment and Decrement)
        /// </summary>
        /// <value>The access total count.</value>
        public long AccessTotalCount { get; internal set; }

        /// <summary>
        /// Gets the access count since last recycle policy was run.
        /// </summary>
        /// <value>The access count since last recycle.</value>
        public long AccessCountSinceLastRecycle { get; internal set; }

        /// <summary>
        /// The number of active reference to this resource.
        /// </summary>
        public int ReferenceCount { get; internal set; }
    }
}
