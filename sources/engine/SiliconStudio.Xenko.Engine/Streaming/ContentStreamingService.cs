// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Streamable resources content management service.
    /// </summary>
    public class ContentStreamingService : IDisposable
    {
        private readonly List<ContentStorage> containers = new List<ContentStorage>();

        //public static DatabaseFileProvider FileProvider => ContentManager.FileProvider ?? GetCustomFileProvider?.Invoke();

        //public static Func<DatabaseFileProvider> GetCustomFileProvider { get; set; }

        internal Func<Task<IDisposable>> MountDatabase { get; set; }

        internal ContentStreamingService()
        {
            MountDatabase = () => Task.FromResult((IDisposable)null);
        }

        /// <summary>
        /// Gets the storage container.
        /// </summary>
        /// <param name="storageHeader">The storage header.</param>
        /// <returns>Content Storage container.</returns>
        public ContentStorage GetStorage(ContentStorageHeader storageHeader)
        {
            ContentStorage result;

            lock (containers)
            {
                // TODO: make it faster using hash
                result = containers.Find(e => e.Url == storageHeader.DataUrl);
                if (result == null)
                {
                    result = new ContentStorage(this, storageHeader);
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
