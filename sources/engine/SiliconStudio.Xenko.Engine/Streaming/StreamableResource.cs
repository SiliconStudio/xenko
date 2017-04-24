using System;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Base class for all resources that can be dynamicly streamed.
    /// </summary>
    public abstract class StreamableResource
    {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        public StreamingManager Manager { get; private set; }

        /// <summary>
        /// Gets the current residency level.
        /// </summary>
        public abstract int CurrentResidency { get; }

        /// <summary>
        /// Gets the allocated residency level.
        /// </summary>
        public abstract int AllocatedResidency { get; }

        /// <summary>
        /// Gets the target residency level.
        /// </summary>
        public int TargetResidency { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this resource is allocated.
        /// </summary>
        public bool IsAllocated => AllocatedResidency > 0;
        
        protected StreamableResource(StreamingManager manager)
        {
            Manager = manager;
        }
    }
}
