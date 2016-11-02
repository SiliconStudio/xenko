// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if !SILICONSTUDIO_PLATFORM_UWP
using System.IO;
using System.Runtime.CompilerServices;

namespace SiliconStudio.Core.IO
{
    public class NativeFile
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool FileExists(string name)
        {
            return File.Exists(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileDelete(string name)
        {
            File.Delete(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FileMove(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long FileSize(string name)
        {
            var fileInfo = new FileInfo(name);
            return fileInfo.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DirectoryExists(string name)
        {
            return Directory.Exists(name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DirectoryCreate(string path)
        {
            Directory.CreateDirectory(path);
        }
    }
}
#endif