// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Streaming;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Streaming
{
    public class ContentStreamingService : GameSystemBase
    {
        private readonly List<ContentStorage> containers = new List<ContentStorage>();

        public ContentStreamingService(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(ContentStreamingService), this);
        }

        protected override void Destroy()
        {
            lock (containers)
            {
                containers.Clear();
            }

            if (Services.GetService(typeof(ContentStreamingService)) == this)
            {
                Services.RemoveService(typeof(ContentStreamingService));
            }

            base.Destroy();
        }

        public ContentStorage GetStorage(ContentStorageHeader storageHeader)
        {
            ContentStorage result;

            lock (containers)
            {
                // TODO: make it faster using hash
                result = containers.Find(e => e.Url == storageHeader.DataUrl);
                if (result == null)
                {
                    result = new ContentStorage(storageHeader);
                    containers.Add(result);
                }
            }

            return result;
        }
    }
}
