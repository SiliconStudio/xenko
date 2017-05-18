// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Base class for all resources that can be dynamicly streamed.
    /// </summary>
    [DebuggerDisplay("Resource {Storage.Url}; Residency: {CurrentResidency}/{MaxResidency}")]
    public abstract class StreamableResource : ComponentBase
    {
        protected DatabaseFileProvider fileProvider;

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
        /// Gets the maximum residency level.
        /// </summary>
        public abstract int MaxResidency { get; }

        /// <summary>
        /// Gets the target residency level.
        /// </summary>
        public int TargetResidency { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether always fully load that resource.
        /// </summary>
        /// <value><c>true</c> if always fully load that resource; otherwise, <c>false</c>.</value>
        public bool ForceFullyLoaded { get; set; }

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
            fileProvider = ContentManager.FileProvider;

            if (Storage != null)
            {
                // TODO: add reference?
            }
        }

        /// <summary>
        /// Stream resource to the target residency level.
        /// </summary>
        /// <param name="residency">The target residency.</param>
        [NotNull]
        internal abstract Task StreamAsync(int residency);

        /// <summary>
        /// Releases this resources on StreamingManager shutdown.
        /// </summary>
        internal virtual void Release()
        {
            Dispose();
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            if (Storage != null)
            {
                // TODO: remove reference?
            }

            Manager.UnregisterResource(this);
            Manager = null;

            base.Destroy();
        }
    }
}
