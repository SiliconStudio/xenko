// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Streaming
{
    public class StreamingManager : GameSystemBase
    {
        private readonly HashSet<StreamableResource> _resources = new HashSet<StreamableResource>();

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => _resources;

        public StreamingManager(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(StreamingManager), this);
        }

        internal void RegisterResource(StreamableResource resource)
        {
            lock (_resources)
            {
                _resources.Add(resource);
            }
        }

        internal void UnregisterResource(StreamableResource resource)
        {
            lock (_resources)
            {
                if (!_resources.Remove(resource))
                    throw new InvalidOperationException("Try to remove a disposed resource not in the list of registered resources.");
            }
        }
    }
}
