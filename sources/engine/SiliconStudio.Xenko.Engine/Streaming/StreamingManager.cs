// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SiliconStudio.Core;
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
        private readonly List<StreamableResource> resources = new List<StreamableResource>(512);
        private readonly List<StreamableResource> priorityUpdateQueue = new List<StreamableResource>(64); // Could be Queue<T> but it doesn't support .Remove(T)
        private int lastUpdateResourcesIndex;
        private DateTime lastUpdateTime = DateTime.MinValue;
        private bool isDisposing;

        // Configuration
        public TimeSpan ManagerUpdatesInterval = TimeSpan.FromMilliseconds(10);
        public TimeSpan ResourceUpdatesInterval = TimeSpan.FromMilliseconds(200);
        public const int MaxResourcesPerUpdate = 30;

        /// <summary>
        /// Gets the content streaming service.
        /// </summary>
        public ContentStreamingService ContentStreaming { get; }

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => resources;

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

            ((Game)Game).Script.AddTask(Update, -100);
        }
        
        /// <inheritdoc />
        protected override void Destroy()
        {
            isDisposing = true;

            if (Services.GetService(typeof(StreamingManager)) == this)
            {
                Services.RemoveService(typeof(StreamingManager));
            }
            if (Services.GetService(typeof(ITexturesStreamingProvider)) == this)
            {
                Services.RemoveService(typeof(ITexturesStreamingProvider));
            }

            lock (resources)
            {
                resources.ForEach(x => x.Dispose());
                resources.Clear();
                priorityUpdateQueue.Clear();
            }

            ContentStreaming.Dispose();

            base.Destroy();
        }

        internal void RegisterResource(StreamableResource resource)
        {
            Debug.Assert(resource != null && isDisposing == false);

            lock (resources)
            {
                Debug.Assert(!resources.Contains(resource));

                resources.Add(resource);
            }
        }

        internal void UnregisterResource(StreamableResource resource)
        {
            if (isDisposing)
                return;

            Debug.Assert(resource != null);

            lock (resources)
            {
                Debug.Assert(resources.Contains(resource));

                resources.Remove(resource);
                priorityUpdateQueue.RemoveAll(x => x == resource);
            }
        }

        /// <summary>
        /// Requests the streamable resource update.
        /// </summary>
        /// <param name="resource">The resource to update.</param>
        public void RequestUpdate(StreamableResource resource)
        {
            lock (resources)
            {
                priorityUpdateQueue.Add(resource);
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

            lock (resources)
            {
                // Find resource or create new
                var resource = resources.Find(x => x.Resource == obj) as StreamingTexture;
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

        private async Task Update()
        {
            while (!IsDisposed)
            {
                // temp for testing...
                if (!((Game)Game).Input.IsKeyDown(Keys.P))
                {
                    ContentStreaming.Update();
                    await ((Game)Game).Script.NextFrame();
                    continue;
                }

                // Update resources
                lock (resources)
                {
                    // Check if update resources
                    var now = DateTime.UtcNow;
                    var delta = now - lastUpdateTime;
                    int resourcesCount = Resources.Count;
                    if (Resources.Count > 0 && delta >= ManagerUpdatesInterval)
                    {
                        lastUpdateTime = now;
                        int resourcesUpdates = Math.Min(MaxResourcesPerUpdate, resourcesCount);

                        // Update high priority queue and then rest of the resources
                        // Note: resources in the update queue are updated always, while others only between specified intervals
                        int resourcesChecks = resourcesCount - priorityUpdateQueue.Count;
                        while (priorityUpdateQueue.Count > 0 && resourcesUpdates-- > 0)
                        {
                            var resource = priorityUpdateQueue[0];
                            priorityUpdateQueue.RemoveAt(0);
                            if (resource.CanBeUpdated)
                                Update(resource, ref now);
                        }
                        while (resourcesUpdates > 0 && resourcesChecks-- > 0)
                        {
                            // Move forward
                            lastUpdateResourcesIndex++;
                            if (lastUpdateResourcesIndex >= resourcesCount)
                                lastUpdateResourcesIndex = 0;

                            // Peek resource
                            var resource = resources[lastUpdateResourcesIndex];

                            // Try to update it
                            if (now - resource.LastUpdate >= ResourceUpdatesInterval && resource.CanBeUpdated)
                            {
                                Update(resource, ref now);
                                resourcesUpdates--;
                            }
                        }

                        // TODO: add StreamingManager stats, update time per frame, updates per frame, etc.
                    }
                }
                
                ContentStreaming.Update();

                // TODO: sleep microThread for ManagerUpdatesInterval ??
                await ((Game)Game).Script.NextFrame();
            }
        }

        private void Update(StreamableResource resource, ref DateTime now)
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
                // Check if need to increase it's residency
                if (targetResidency > currentResidency)
                {
                    // Check if need to allocate memory for that resource
                    Task allocatingTask = null;
                    if (allocatedResidency < targetResidency)
                    {
                        // TODO: check memory pool for that resource group -> if out of memory call memory decrease situation for a group

                        // Update resource allocation
                        allocatingTask = resource.UpdateAllocation(targetResidency);

                        // Ensure that resource residency didn't change (just check for any leaks)
                        Debug.Assert(currentResidency == resource.CurrentResidency);
                    }

                    // Calculate residency level to stream in (resources may want to incease/decrease it's quality in steps rather than at once)
                    //var requestedResidency = handler.CalculateRequestedResidency(resource, targetResidency);// TODO: use resource groups and handlers
                    var requestedResidency = targetResidency;

                    // Create streaming task (resource type specific)
                    var streamingTask = resource.CreateStreamingTask(requestedResidency);

                    // Start tasks
                    if (allocatingTask != null)
                    {
                        allocatingTask.ContinueWith(x => streamingTask);
                        allocatingTask.Start();
                    }
                    else
                    {
                        streamingTask.Start();
                    }
                }
                else
                {
                    // TODO: finish logic here...

                    // TODO: decrease residency level

                    // TODO: check case for deallocation, when do it?
                }
            }
            else
            {
                // TODO: Check if target residency is stable (no changes for a while)

                // TODO: deallocate or decrease memory usage after timout? (timeout should be smaller on low mem)
            }
        }
    }
}
