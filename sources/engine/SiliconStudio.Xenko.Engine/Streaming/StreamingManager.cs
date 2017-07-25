//#define USE_TEST_MANUAL_QUALITY
// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Performs content streaming.
    /// </summary>
    /// <seealso cref="SiliconStudio.Xenko.Games.GameSystemBase" />
    /// <seealso cref="SiliconStudio.Xenko.Graphics.Data.ITexturesStreamingProvider" />
    public class StreamingManager : GameSystemBase, IStreamingManager, ITexturesStreamingProvider
    {
        private readonly List<StreamableResource> resources = new List<StreamableResource>(512);
        private readonly Dictionary<object, StreamableResource> resourcesLookup = new Dictionary<object, StreamableResource>(512);
        private readonly List<StreamableResource> activeStreaming = new List<StreamableResource>(64);
        private int lastUpdateResourcesIndex;
        private bool isDisposing;
        private int frameIndex;
#if USE_TEST_MANUAL_QUALITY
        private int testQuality = 100;
#endif

        private bool HasActiveTaskSlotFree => activeStreaming.Count < MaxTasksRunningSimultaneously;

        /// <summary>
        /// The interval between <see cref="StreamingManager"/> updates.
        /// </summary>
        public TimeSpan ManagerUpdatesInterval = TimeSpan.FromMilliseconds(33);

        /// <summary>
        /// The inverval between streaming updates per single <see cref="StreamableResource"/>
        /// </summary>
        public TimeSpan ResourceUpdatesInterval = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// The <see cref="StreamableResource"/> live timeout. Resources that aren't used for a while are downscaled in quality.
        /// </summary>
        public TimeSpan ResourceLiveTimeout = TimeSpan.FromSeconds(5);
        
        /// <summary>
        /// The maximum number of resources updated per streaming manager tick. Used to balance performance/streaming speed.
        /// </summary>
        public int MaxResourcesPerUpdate = 10;

        /// <summary>
        /// The maximum number of resources streamed at the same time. Used to balance performance/streaming speed.
        /// </summary>
#if SILICONSTUDIO_PLATFORM_ANDROID || SILICONSTUDIO_PLATFORM_IOS
        public const int MaxTasksRunningSimultaneously = 1;
#else
        public const int MaxTasksRunningSimultaneously = 4;
#endif

        /// <summary>
        /// Gets the content streaming service.
        /// </summary>
        public ContentStreamingService ContentStreaming { get; }

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => resources;

        /// <summary>
        /// Gets or sets a value indicating whether resource streaming should be disabled.
        /// </summary>
        public bool DisableStreaming { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingManager"/> class.
        /// </summary>
        /// <param name="services">The servicies registry.</param>
        /// <remarks>
        /// The GameSystem expects the following services to be registered: <see cref="T:SiliconStudio.Xenko.Games.IGame" /> and <see cref="T:SiliconStudio.Core.Serialization.Contents.IContentManager" />.
        /// </remarks>
        public StreamingManager(IServiceRegistry services) : base(services)
        {
            services.AddService(typeof(StreamingManager), this);
            services.AddService(typeof(IStreamingManager), this);
            services.AddService(typeof(ITexturesStreamingProvider), this);

            ContentStreaming = new ContentStreamingService();

            (Game as Game)?.Script.AddTask(Update);
        }

        /// <inheritdoc />
        protected override void Destroy()
        {
            isDisposing = true;

            if (Services.GetService(typeof(StreamingManager)) == this)
            {
                Services.RemoveService(typeof(StreamingManager));
            }
            if (Services.GetService(typeof(IStreamingManager)) == this)
            {
                Services.RemoveService(typeof(IStreamingManager));
            }
            if (Services.GetService(typeof(ITexturesStreamingProvider)) == this)
            {
                Services.RemoveService(typeof(ITexturesStreamingProvider));
            }

            lock (resources)
            {
                resources.ForEach(x => x.Release());
                resources.Clear();
                resourcesLookup.Clear();
                activeStreaming.Clear();
            }

            ContentStreaming.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// Gets the <see cref="StreamableResource"/> corresponding to the given resource object.
        /// </summary>
        /// <typeparam name="T">The type of the streamable resource.</typeparam>
        /// <param name="obj">The object.</param>
        /// <returns>Streamable resource, or null if it can't be found.</returns>
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(object obj) where T : StreamableResource
        {
            StreamableResource result;
            resourcesLookup.TryGetValue(obj, out result);
            return result as T;
        }

        /// <summary>
        /// Gets the <see cref="StreamingTexture"/> corresponding to the given texture object.
        /// </summary>
        /// <param name="obj">The texture object.</param>
        /// <returns>Streamable texture, or null if it can't be found.</returns>
        [CanBeNull]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StreamingTexture Get(Texture obj)
        {
            StreamableResource result;
            resourcesLookup.TryGetValue(obj, out result);
            return result as StreamingTexture;
        }

        internal void RegisterResource(StreamableResource resource)
        {
            Debug.Assert(resource != null && isDisposing == false);

            lock (resources)
            {
                Debug.Assert(!resources.Contains(resource));

                resources.Add(resource);
                resourcesLookup.Add(resource.Resource, resource);
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
                resourcesLookup.Remove(resource.Resource);
                activeStreaming.Remove(resource);
            }
        }

        private StreamingTexture CreateStreamingTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            // Get content storage container
            var storage = ContentStreaming.GetStorage(ref storageHeader);
            if (storage == null)
                throw new ContentStreamingException("Missing content storage.");

            // Find resource or create new
            var resource = Get(obj);
            if (resource == null)
            {
                resource = new StreamingTexture(this, obj);
                RegisterResource(resource);
            }

            // Update resource storage/description information (may be modified on asset rebuilding)
            resource.Init(storage, ref imageDescription);
            
            // Check if cannot use streaming
            if (DisableStreaming)
            {
                FullyLoadResource(resource);
            }

            return resource;
        }
        
        /// <inheritdoc />
        void ITexturesStreamingProvider.FullyLoadTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            lock (resources)
            {
                // Get streaming object
                var resource = CreateStreamingTexture(obj, ref imageDescription, ref storageHeader);

                // Stream resource to the maximum level
                FullyLoadResource(resource);

                // Release streaming object
                resource.Release();
            }
        }

        /// <inheritdoc />
        void ITexturesStreamingProvider.RegisterTexture(Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader)
        {
            lock (resources)
            {
                CreateStreamingTexture(obj, ref imageDescription, ref storageHeader);
            }
        }

        /// <inheritdoc />
        void ITexturesStreamingProvider.UnregisterTexture(Texture obj)
        {
            Debug.Assert(obj != null);

            lock (resources)
            {
                var resource = Get(obj);
                resource?.Release();
            }
        }

        /// <inheritdoc />
        void IStreamingManager.FullyLoadResource(object obj)
        {
            StreamableResource resource;
            lock (resources)
            {
                resource = Get<StreamableResource>(obj);
            }

            if (resource != null)
                FullyLoadResource(resource);
        }

        private void FullyLoadResource(StreamableResource resource)
        {
            // Disable dynamic streaming for the esource
            resource.ForceFullyLoaded = true;

            // Stream resource to the maximum level
            // Note: this does not care about MaxTasksRunningSimultaneously limit
            var task = StreamAsync(resource, resource.MaxResidency);
            task.Wait();

            // Synchronize
            FlushSync();
        }

        private async Task Update()
        {
            while (!IsDisposed)
            {
                // Perform synchronization
                FlushSync();

#if USE_TEST_MANUAL_QUALITY
                // Temporary testing code used for testing quality changing using K/L keys
                if (((Game)Game).Input.IsKeyPressed(SiliconStudio.Xenko.Input.Keys.K))
                {
                    testQuality = Math.Min(testQuality + 5, 100);
                }
                if (((Game)Game).Input.IsKeyPressed(SiliconStudio.Xenko.Input.Keys.L))
                {
                    testQuality = Math.Max(testQuality - 5, 0);
                }
#endif

                // Update resources
                lock (resources)
                {
                    int resourcesCount = Resources.Count;
                    if (resourcesCount > 0)
                    {
                        var now = DateTime.UtcNow;
                        int resourcesUpdates = Math.Min(MaxResourcesPerUpdate, resourcesCount);
                        int resourcesChecks = resourcesCount;

                        while (resourcesUpdates > 0 && resourcesChecks-- > 0 && HasActiveTaskSlotFree)
                        {
                            // Move forward
                            // Note: we update resources like in a ring buffer
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

                frameIndex++;
                await Task.Delay(ManagerUpdatesInterval);
            }
        }

        private void Update(StreamableResource resource, ref DateTime now)
        {
            Debug.Assert(resource != null && resource.CanBeUpdated);

            // Calculate target quality for that asset
            StreamingQuality targetQuality = StreamingQuality.Mininum;
            if (resource.ForceFullyLoaded)
            {
                targetQuality = StreamingQuality.Maximum;
            }
            else if (resource.LastTimeUsed > 0)
            {
                var lastUsageTimespan = new TimeSpan((frameIndex - resource.LastTimeUsed) * ManagerUpdatesInterval.Ticks);
                if (lastUsageTimespan < ResourceLiveTimeout)
                {
                    targetQuality = StreamingQuality.Maximum;
#if USE_TEST_MANUAL_QUALITY
                    targetQuality = (testQuality / 100.0f); // apply quality scale for testing
#endif
                    // TODO: here we should apply resources group master scale (based on game settings quality level and memory level)
                }
            }
            targetQuality.Normalize();

            // Calculate target residency level (discrete value)
            var currentResidency = resource.CurrentResidency;
            var allocatedResidency = resource.AllocatedResidency;
            var targetResidency = resource.CalculateTargetResidency(targetQuality);
            Debug.Assert(allocatedResidency >= currentResidency && allocatedResidency >= 0);

            // Update target residency smoothing
            // TODO: use move quality samples and use max or avg value - make that input it smooth - or use PID
            //resource.QualitySamples.Add(targetResidency);
            //targetResidency = resource.QualitySamples.Maximum();

            // Assign last update time
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
                // Calculate residency level to stream in (resources may want to incease/decrease it's quality in steps rather than at once)
                var requestedResidency = resource.CalculateRequestedResidency(targetResidency);

                // Create streaming task (resource type specific)
                StreamAsync(resource, requestedResidency);
            }
        }

        private Task StreamAsync(StreamableResource resource, int residency)
        {
            activeStreaming.Add(resource);
            var task = resource.StreamAsyncInternal(residency);
            task.Start();

            return task;
        }

        private void FlushSync()
        {
            for (int i = 0; i < activeStreaming.Count; i++)
            {
                var resource = activeStreaming[i];
                if (resource.IsTaskActive)
                    continue;

                resource.FlushSync();
                activeStreaming.RemoveAt(i);
                i--;
            }
        }

        /// <summary>
        /// Called when render mesh is submitted to rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        public void StreamResources(RenderMesh renderMesh)
        {
            if (renderMesh.MaterialPass != null)
            {
                StreamResources(renderMesh.MaterialPass.Parameters);
            }
        }

        /// <summary>
        /// Called when material parameters are submitted to rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="parameters">The material parameters.</param>
        public void StreamResources(ParameterCollection parameters)
        {
            if (parameters.ObjectValues == null)
                return;

            // Register all binded textures
            foreach (var e in parameters.ObjectValues)
            {
                if (e is Texture t)
                {
                    var resource = Get(t);
                    if (resource != null)
                    {
                        resource.LastTimeUsed = frameIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Called when texture is submitted to rendering. Registers referenced resources to stream them.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public void StreamResources(Texture texture)
        {
            if (texture == null)
                return;

            var resource = Get(texture);
            if (resource != null)
            {
                resource.LastTimeUsed = frameIndex;
            }
        }
        
        /// <summary>
        /// Called when render mesh is submitted to rendering. Registers referenced resources to stream them to the maximum quality level.
        /// </summary>
        /// <param name="renderMesh">The render mesh.</param>
        public void StreamResourcesFully(RenderMesh renderMesh)
        {
            if (renderMesh.MaterialPass != null)
            {
                StreamResourcesFully(renderMesh.MaterialPass.Parameters);
            }
        }

        /// <summary>
        /// Called when material parameters are submitted to rendering. Registers referenced resources to stream them to the maximum quality level.
        /// </summary>
        /// <param name="parameters">The material parameters.</param>
        public void StreamResourcesFully(ParameterCollection parameters)
        {
            if (parameters.ObjectValues == null)
                return;

            // Register all binded textures
            foreach (var e in parameters.ObjectValues)
            {
                if (e is Texture t)
                {
                    var resource = Get(t);
                    if (resource != null)
                    {
                        resource.ForceFullyLoaded = true;
                        resource.LastTimeUsed = frameIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Called when texture is submitted to rendering. Registers referenced resources to stream them to the maximum quality level.
        /// </summary>
        /// <param name="texture">The texture.</param>
        public void StreamResourcesFully(Texture texture)
        {
            if (texture == null)
                return;

            var resource = Get(texture);
            if (resource != null)
            {
                resource.ForceFullyLoaded = true;
                resource.LastTimeUsed = frameIndex;
            }
        }
    }
}
