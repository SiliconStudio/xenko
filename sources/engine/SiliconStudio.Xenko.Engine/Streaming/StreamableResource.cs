// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Base class for all resources that can be dynamicly streamed.
    /// </summary>
    public abstract class StreamableResource : ComponentBase
    {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        public StreamingManager Manager { get; private set; }

        /// <summary>
        /// Gets the resource storage.
        /// </summary>
        public ContentStorage Storage { get; private set; }
        
        /// <summary>
        /// Gets the resource object.
        /// </summary>
        public abstract object Resource { get; }

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

        /// <summary>
        /// Determines whether this instance can be updated. Which means: no async streaming, no pending action in background.
        /// </summary>
        /// <returns><c>true</c> if this instance can be updated; otherwise, <c>false</c>.</returns>
        internal abstract bool CanBeUpdated { get; }

        /// <summary>
        /// The last update time.
        /// </summary>
        internal DateTime LastUpdate;

        /// <summary>
        /// The last target residency change time.
        /// </summary>
        internal DateTime TargetResidencyChange;

        protected StreamableResource(StreamingManager manager)
        {
            Manager = manager;
            Manager.RegisterResource(this);
            LastUpdate = TargetResidencyChange = DateTime.MinValue;
        }

        protected void Init(ContentStorage storage)
        {
            if (Storage != null)
            {
                // TODO: remove reference?
            }

            Storage = storage;

            if (Storage != null)
            {
                // TODO: add reference?
            }
        }
        
        /// <summary>
        /// Updates the resource allocation to the given residency level. May not be updated now but in an async operation.
        /// </summary>
        /// <param name="residency">The target allocation residency.</param>
        /// <returns>Async task that updates resource allocation or null if already done it.</returns>
        [CanBeNull]
        internal abstract Task UpdateAllocation(int residency);

        /// <summary>
        /// Creates streaming task (or tasks sequence) to perform resource streaming for the desire residency level.
        /// </summary>
        /// <param name="residency">The target residency.</param>
        /// <returns>Async task or tasks that update resource residency level. Must be preceded with UpdateAllocation call.</returns>
        [NotNull]
        internal abstract Task CreateStreamingTask(int residency);

        /// <inheritdoc />
        protected override void Destroy()
        {
            Manager.UnregisterResource(this);
            Manager = null;

            base.Destroy();
        }
    }
}
