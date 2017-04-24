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
    }
}
