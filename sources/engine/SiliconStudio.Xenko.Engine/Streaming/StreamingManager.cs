// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;

namespace SiliconStudio.Xenko.Streaming
{
    public class StreamingManager : GameSystemBase, ITexturesStreamingProvider
    {
        private readonly List<StreamableResource> _resources = new List<StreamableResource>(512);
        private readonly List<StreamableResource> _priorityUpdateQueue = new List<StreamableResource>(64); // Could be Queue<T> but it doesn't support .Remove(T)
        private int _lastUpdateResourcesIndex;
        private DateTime _lastUpdateTime = DateTime.MinValue;

        /// <summary>
        /// Gets the content streaming service.
        /// </summary>
        public ContentStreamingService ContentStreaming { get; }

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => _resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingManager"/> class.
        /// </summary>
        /// <param name="registry">The servicies registry.</param>
        /// <remarks>
        /// The GameSystem is expecting the following services to be registered: <see cref="T:SiliconStudio.Xenko.Games.IGame" /> and <see cref="T:SiliconStudio.Core.Serialization.Contents.IContentManager" />.
        /// </remarks>
        public StreamingManager(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(StreamingManager), this);
            registry.AddService(typeof(ITexturesStreamingProvider), this);

            ContentStreaming = new ContentStreamingService();
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            if (Services.GetService(typeof(StreamingManager)) == this)
            {
                Services.RemoveService(typeof(StreamingManager));
            }
            if (Services.GetService(typeof(ITexturesStreamingProvider)) == this)
            {
                Services.RemoveService(typeof(ITexturesStreamingProvider));
            }

            lock (_resources)
            {
                _resources.ForEach(x => x.Dispose());
                _resources.Clear();
                _priorityUpdateQueue.Clear();
            }

            ContentStreaming.Dispose();

            base.Destroy();
        }

        internal void RegisterResource(StreamableResource resource)
        {
            Debug.Assert(resource != null);

            lock (_resources)
            {
                Debug.Assert(!_resources.Contains(resource));

                _resources.Add(resource);

                // Register quicker update for that resource
                RequestUpdate(resource);
            }
        }

        internal void UnregisterResource(StreamableResource resource)
        {
            Debug.Assert(resource != null);

            lock (_resources)
            {
                Debug.Assert(_resources.Contains(resource));

                _resources.Remove(resource);
                _priorityUpdateQueue.Remove(resource);
            }
        }

        /// <summary>
        /// Requests the streamable resource update.
        /// </summary>
        /// <param name="resource">The resource to update.</param>
        public void RequestUpdate(StreamableResource resource)
        {
            lock (_resources)
            {
                _priorityUpdateQueue.Add(resource);
            }
        }

        /// <inheritdoc />
        void ITexturesStreamingProvider.RegisterTexture(Texture obj, ContentStorageHeader storageHeader)
        {
            Debug.Assert(obj != null && storageHeader != null);

            // Get content storage container
            var storage = ContentStreaming.GetStorage(storageHeader);
            if (storage == null)
            {
                // TODO: send error to log?
                return;
            }

            // Create streamable resource
            var resource = new StreamingTexture(this, storage, obj);
            RegisterResource(resource);
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Configuration
            TimeSpan ManagerUpdatesInterval = TimeSpan.FromMilliseconds(10);
            TimeSpan ResourceUpdatesInterval = TimeSpan.FromMilliseconds(200);
            const int MaxResourcesPerUpdate = 30;

            // Check time since last update
            var now = DateTime.UtcNow;
            var delta = now - _lastUpdateTime;
            int resourcesCount = Resources.Count;
            if (resourcesCount == 0 || delta < ManagerUpdatesInterval)
                return;
            _lastUpdateTime = now;

            // Update resources
            lock (_resources)
            {
                int resourcesUpdates = Math.Min(MaxResourcesPerUpdate, resourcesCount);

                // Update high priority queue and then rest of the resources
                // Note: resources in the update queue are updated always, while others only between specified intervals
                int resourcesChecks = resourcesCount - _priorityUpdateQueue.Count;
                while (_priorityUpdateQueue.Count > 0 && resourcesUpdates-- > 0)
                {
                    var resource = _priorityUpdateQueue[0];
                    _priorityUpdateQueue.RemoveAt(0);
                    if (resource.CanBeUpdated)
                        update(resource, ref now);
                }
                while (resourcesUpdates > 0 && resourcesChecks-- > 0)
                {
                    // Move forward
                    _lastUpdateResourcesIndex++;
                    if (_lastUpdateResourcesIndex >= resourcesCount)
                        _lastUpdateResourcesIndex = 0;

                    // Peek resource
                    var resource = _resources[_lastUpdateResourcesIndex];

                    // Try to update it
                    if (now - resource.LastUpdate >= ResourceUpdatesInterval && resource.CanBeUpdated)
                    {
                        update(resource, ref now);
                        resourcesUpdates--;
                    }
                }

                // TODO: add StreamingManager stats, update time per frame, updates per frame, etc.
            }
        }

        private void update(StreamableResource resource, ref DateTime now)
        {
            // TODO: finish this
        }
    }
}
