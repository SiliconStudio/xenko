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
        private readonly HashSet<StreamableResource> _resources = new HashSet<StreamableResource>();

        /// <summary>
        /// Gets the content streaming service.
        /// </summary>
        public ContentStreamingService ContentStreaming { get; }

        /// <summary>
        /// List with all registered streamable resources.
        /// </summary>
        public ICollection<StreamableResource> Resources => _resources;

        public StreamingManager(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(StreamingManager), this);
            registry.AddService(typeof(ITexturesStreamingProvider), this);

            ContentStreaming = new ContentStreamingService(registry);
        }

        protected override void Destroy()
        {
            if (Services.GetService(typeof(ContentStreamingService)) == this)
            {
                Services.RemoveService(typeof(ContentStreamingService));
            }
            if (Services.GetService(typeof(ITexturesStreamingProvider)) == this)
            {
                Services.RemoveService(typeof(ITexturesStreamingProvider));
            }

            base.Destroy();
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

        /// <inheritdoc />
        void ITexturesStreamingProvider.RegisterTexture(Texture obj, ContentStorageHeader storageHeader)
        {
            Debug.Assert(obj != null && storageHeader != null);
            
            // Get content storage container
            var storage = Services.GetSafeServiceAs<ContentStreamingService>().GetStorage(storageHeader);
            if (storage == null)
            {
                // TODO: send error to log?
                return;
            }

            // Create streamable resource
            var resource = new StreamingTexture(this, storage, obj);
        }
    }
}
