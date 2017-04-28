// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpDX.Win32;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Streaming
{
    public class StreamingManager : GameSystemBase, ITexturesStreamingProvider
    {
        private readonly List<StreamableResource> _resources = new List<StreamableResource>(512);
        private readonly List<StreamableResource> _priorityUpdateQueue = new List<StreamableResource>(64); // Could be Queue<T> but it doesn't support .Remove(T)
        private int _lastUpdateResourcesIndex;
        private DateTime _lastUpdateTime = DateTime.MinValue;
        private bool _isDisposing;

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

            Enabled = true;
        }
        
        /// <inheritdoc />
        protected override void Destroy()
        {
            _isDisposing = true;

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
            Debug.Assert(resource != null && _isDisposing == false);

            lock (_resources)
            {
                Debug.Assert(!_resources.Contains(resource));

                _resources.Add(resource);
            }
        }

        internal void UnregisterResource(StreamableResource resource)
        {
            if (_isDisposing)
                return;

            Debug.Assert(resource != null);

            lock (_resources)
            {
                Debug.Assert(_resources.Contains(resource));

                _resources.Remove(resource);
                _priorityUpdateQueue.RemoveAll(x => x == resource);
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
        void ITexturesStreamingProvider.RegisterTexture(Texture obj, ref ImageDescription imageDescription, ContentStorageHeader storageHeader)
        {
            Debug.Assert(obj != null && storageHeader != null);

            // Get content storage container
            var storage = ContentStreaming.GetStorage(storageHeader);
            if (storage == null)
            {
                // TODO: send error to log?
                return;
            }

            lock (_resources)
            {
                // Find resource or create new
                var resource = _resources.Find(x => x.Resource == obj) as StreamingTexture;
                if (resource == null)
                {
                    resource = new StreamingTexture(this, obj);
                }

                // Update resource storage/description information (may be modified on asset rebuilding)
                resource.Init(storage, ref imageDescription);

                // Register quicker update for that resource
                RequestUpdate(resource);
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Configuration
            TimeSpan ManagerUpdatesInterval = TimeSpan.FromMilliseconds(10);
            TimeSpan ResourceUpdatesInterval = TimeSpan.FromMilliseconds(200);
            const int MaxResourcesPerUpdate = 30;

            if (!((Game)Game).Input.IsKeyDown(Keys.P))
                return;

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
            Debug.Assert(resource != null && resource.CanBeUpdated);

            // TODO: should we lock resource during the update? it's done in a main thread so probably no chance to collide with gpu async but even though..

            // Pick group and handler dedicated for that resource
            // TODO: finish resource groups and streaming handlers implementation
            //var group = resource.Group;
            //var handler = group.Handler;

            // Calculate target quality for that asset
            StreamingQuality targetQuality = StreamingQuality.Maximum;
            //if (resource.IsDynamic)
            {
                //targetQuality = handler.CalculateTargetQuality(resource, now);
                // TODO: here we should apply resources group master scale (based on game settings quality level and memory level)
                targetQuality.Normalize();
            }

            // Calculate target residency level (discrete value)
            var currentResidency = resource.CurrentResidency;
            var allocatedResidency = resource.AllocatedResidency;
            //var targetResidency = handler.CalculateResidency(resource, targetQuality);
            var targetResidency = (resource as StreamingTexture).Description.MipLevels; // TODO: remove hardoded value for textures, use steraming groups/handlers
            Debug.Assert(allocatedResidency >= currentResidency && allocatedResidency >= 0);

            // Update target residency smoothing
            // TODO: use move quality samples and use max or avg value - make that input it smooth - or use PID
            //resource.QualitySamples.Add(targetResidency);
            //targetResidency = resource.QualitySamples.Maximum();

            // Assign last update time
            var updateTimeDelta = now - resource.LastUpdate;
            resource.LastUpdate = now;

            // Check if a target residency level has been changed
            if (targetResidency != resource.TargetResidency)
            {
                // Register change
                resource.TargetResidency = targetResidency;
                resource.TargetResidencyChange = now;
            }

            // Check if need to change resource current residency
            if (targetResidency != currentResidency)
            {
                // for now just hardoced streaming for textures to make it work
                resource.CreateStreamingTask(targetResidency).Start();

                // TODO: finish dynamic streaming using code below \/ \/ \/

                /*// Check if need to increase it's residency
                if (targetResidency > currentResidency)
                {
                    // Check if need to allocate memory for that resource
                    Task allocateTask = null;
                    if (allocatedResidency < targetResidency)
                    {
                        // TODO: check memory pool for that resource group -> if out of memory call memory decrease situation for a group

                        // Update resource allocation
                        allocateTask = resource.UpdateAllocation(targetResidency);

                        // Ensure that resource residency didn't change (just check for any leaks)
                        Debug.Assert(currentResidency == resource.CurrentResidency);
                    }

                    // Calculate residency level to stream in (resources may want to incease/decrease it's quality in steps rather than at once)
                    //var requestedResidency = handler.CalculateRequestedResidency(resource, targetResidency);
                    var requestedResidency = targetResidency;

                    // Create streaming task (resource type specific)
                    var streamingTask = resource.CreateStreamingTask(requestedResidency);

                    // Start tasks
                    if (streamingTask != null)
                    {
                        if (allocateTask != null)
                        {
                            allocateTask.ContinueWith(streamingTask);
                            allocateTask.Start();
                        }
                        else
                        {
                            streamingTask.Start();
                        }
                    }
                    else
                    {
                        // Log Resource created null streaming task?

                        if (allocateTask != null)
                            allocateTask.Start();
                    }
                }
                else
                {
                    // TODO: finish logic here...

                    // TODO: decrease residency level

                    // TODO: check case for deallocation, when do it?
                }*/
            }
            else
            {
                // TODO: Check if target residency is stable (no changes for a while)

                // TODO: deallocate or decrease memory usage after timout? (timeout should be smaller on low mem)
            }
        }
    }
}
