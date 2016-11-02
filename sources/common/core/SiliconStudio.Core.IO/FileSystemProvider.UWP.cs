// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_UWP
using System;
using System.Diagnostics;
using System.IO;
using SharpDX.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// A file system implementation for IVirtualFileProvider.
    /// </summary>
    public partial class FileSystemProvider
    {
        public override string GetAbsolutePath(string path)
        {
            return ConvertUrlToFullPath(path);
        }

        /// <inheritdoc/>
        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamFlags = StreamFlags.None)
        {
            if (localBasePath != null && url.Split(VirtualFileSystem.DirectorySeparatorChar, VirtualFileSystem.AltDirectorySeparatorChar).Contains(".."))
                throw new InvalidOperationException("Relative path is not allowed in FileSystemProvider.");

            var rawAccess = (NativeFileAccess) 0;
            if ((access & VirtualFileAccess.Read) != 0)
                rawAccess |= NativeFileAccess.Read;
            if ((access & VirtualFileAccess.Write) != 0)
                rawAccess |= NativeFileAccess.Write;

            return new NativeFileStream(ConvertUrlToFullPath(url), (NativeFileMode)mode, rawAccess);
        }

        /// <inheritdoc/>
        public override string[] ListFiles(string url, string searchPattern, VirtualSearchOption searchOption)
        {
            throw new NotImplementedException();
        }
    }
}
#endif