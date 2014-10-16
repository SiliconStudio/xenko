// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
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
        public override string GetAbsolutePath(string path)
        {
            return ConvertUrlToFullPath(path);
        }

        /// <inheritdoc/>
        public override Stream OpenStream(string url, VirtualFileMode mode, VirtualFileAccess access, VirtualFileShare share = VirtualFileShare.Read, StreamFlags streamFlags = StreamFlags.None)
        {
            if (localBasePath != null && url.Split(VirtualFileSystem.DirectorySeparatorChar, VirtualFileSystem.AltDirectorySeparatorChar).Contains(".."))
                throw new InvalidOperationException("Relative path is not allowed in FileSystemProvider.");
            return new FileStream(ConvertUrlToFullPath(url), (FileMode)mode, (FileAccess)access, (FileShare)share);
        }

        /// <inheritdoc/>
        public override string[] ListFiles(string url, string searchPattern, VirtualSearchOption searchOption)
        {
            return Directory.GetFiles(ConvertUrlToFullPath(url), searchPattern, (SearchOption)searchOption).Select(ConvertFullPathToUrl).ToArray();
        }

        /// <inheritdoc/>
        public override void FileMove(string sourceUrl, string destinationUrl)
        {
            File.Move(ConvertUrlToFullPath(sourceUrl), ConvertUrlToFullPath(destinationUrl));
        }

        /// <inheritdoc/>
        public override void FileMove(string sourceUrl, IVirtualFileProvider destinationProvider, string destinationUrl)
        {
            var fsProvider = destinationProvider as FileSystemProvider;
            if (fsProvider != null)
            {
                destinationProvider.CreateDirectory(destinationUrl.Substring(0, destinationUrl.LastIndexOf(VirtualFileSystem.DirectorySeparatorChar)));
                File.Move(ConvertUrlToFullPath(sourceUrl), fsProvider.ConvertUrlToFullPath(destinationUrl));
            }
            else
            {
                using (Stream sourceStream = OpenStream(sourceUrl, VirtualFileMode.Open, VirtualFileAccess.Read),
                    destinationStream = destinationProvider.OpenStream(destinationUrl, VirtualFileMode.CreateNew, VirtualFileAccess.Write))
                {
                    sourceStream.CopyTo(destinationStream);
                }
                FileDelete(sourceUrl);
            }
        }

        public override DateTime GetLastWriteTime(string url)
        {
            return File.GetLastWriteTime(ConvertUrlToFullPath(url));
        }
    }
}
#endif