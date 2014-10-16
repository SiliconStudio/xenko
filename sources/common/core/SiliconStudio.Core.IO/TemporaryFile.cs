// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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