// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Streamable resources content management service.
    /// </summary>
    public class ContentStreamingService : IDisposable
    {
        private readonly List<ContentStorage> containers = new List<ContentStorage>();

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

        public void Dispose()
        {
            lock (containers)
            {
                containers.Clear();
            }
        }
    }
}
