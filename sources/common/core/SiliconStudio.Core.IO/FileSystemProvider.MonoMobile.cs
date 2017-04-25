// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_PLATFORM_MONO_MOBILE
using System;
using System.IO;
using System.Linq;

namespace SiliconStudio.Core.IO
{
    /// <summary>
    /// A file system implementation for IVirtualFileProvider.
    /// </summary>
    public partial class FileSystemProvider
    {
#if SILICONSTUDIO_PLATFORM_IOS
        public bool AutoSetSkipBackupAttribute { get; set; }
#endif

        public override string GetAbsolutePath(string path)
        {
            return ConvertUrlToFullPath(path);
        }

        /// <inheritdoc/>
        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamType = StreamFlags.None)
        {
            if (localBasePath != null && url.Split(VirtualFileSystem.DirectorySeparatorChar, VirtualFileSystem.AltDirectorySeparatorChar).Contains(".."))
                throw new InvalidOperationException("Relative path is not allowed in FileSystemProvider.");
            var filename = ConvertUrlToFullPath(url);
            var result = new FileStream(filename, (FileMode)mode, (FileAccess)access, (FileShare)share);

#if SILICONSTUDIO_PLATFORM_IOS
            if (AutoSetSkipBackupAttribute && (mode == VirtualFileMode.CreateNew || mode == VirtualFileMode.Create || mode == VirtualFileMode.OpenOrCreate))
            {
                Foundation.NSFileManager.SetSkipBackupAttribute(filename, true);
            }
#endif

            return result;
        }

        /// <inheritdoc/>
        public override string[] ListFiles(string url, string searchPattern, VirtualSearchOption searchOption)
        {
            return Directory.GetFiles(ConvertUrlToFullPath(url), searchPattern, (SearchOption)searchOption).Select(ConvertFullPathToUrl).ToArray();
        }
    }
}
#endif
