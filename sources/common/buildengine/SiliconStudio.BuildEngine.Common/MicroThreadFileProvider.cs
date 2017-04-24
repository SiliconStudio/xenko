// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.IO;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.MicroThreading;

namespace SiliconStudio.BuildEngine
{
    public class MicroThreadFileProvider : VirtualFileProviderBase
    {
        public MicroThreadLocal<IVirtualFileProvider> ThreadLocal = new MicroThreadLocal<IVirtualFileProvider>(() =>
                {
                    throw new InvalidOperationException("No VirtualFileProvider set for this microthread.");
                });

        public MicroThreadFileProvider(string rootPath) : base(rootPath)
        {
        }

        public override string GetAbsolutePath(string path)
        {
            return ThreadLocal.Value.GetAbsolutePath(path);
        }

        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamFlags = StreamFlags.None)
        {
            return ThreadLocal.Value.OpenStream(url, mode, access, share, streamFlags);
        }

        public override bool FileExists(string url)
        {
            return ThreadLocal.Value.FileExists(url);
        }

        public override long FileSize(string url)
        {
            return ThreadLocal.Value.FileSize(url);
        }
    }
}
