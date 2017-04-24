// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SiliconStudio.BuildEngine
{
    public static class PathExt
    {
        public static bool IsValidPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // ReSharper disable ReturnValueOfPureMethodIsNotUsed
                Path.GetFullPath(path);
                // ReSharper restore ReturnValueOfPureMethodIsNotUsed
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetRelativePath(string absoluteRoot, string absolutePath)
        {
            var path = new StringBuilder(260); // MAX_PATH
            if (string.IsNullOrWhiteSpace(absolutePath))
                return absolutePath;

            string sourcePath = new DirectoryInfo(absolutePath).FullName;
            absoluteRoot = new DirectoryInfo(absoluteRoot).FullName;
            return PathRelativePathTo(path, absoluteRoot, FILE_ATTRIBUTE_DIRECTORY, sourcePath, FILE_ATTRIBUTE_DIRECTORY) == 0 ? sourcePath : path.ToString();
        }

        // ReSharper disable InconsistentNaming
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;
        //private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        // ReSharper restore InconsistentNaming

        [DllImport("shlwapi.dll", SetLastError = true)]
        private static extern int PathRelativePathTo(StringBuilder pszPath, string pszFrom, int dwAttrFrom, string pszTo, int dwAttrTo);

    }
}
