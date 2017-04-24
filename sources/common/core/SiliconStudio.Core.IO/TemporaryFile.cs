// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.IO;

namespace SiliconStudio.Core.IO
{
    internal partial class TemporaryFile : IDisposable
    {
        private bool isDisposed;
        private string path;

        public TemporaryFile()
        {
            path = VirtualFileSystem.GetTempFileName();
        }


        public string Path
        {
            get { return path; }
        }

#if !NETFX_CORE
        ~TemporaryFile()
        {
            Dispose(false);
        }
#endif

        public void Dispose()
        {
            Dispose(false);
#if !NETFX_CORE
            GC.SuppressFinalize(this);
#endif
        }

        private void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                isDisposed = true;
                TryDelete();
            }
        }

        private void TryDelete()
        {
            try
            {
                VirtualFileSystem.FileDelete(path);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
