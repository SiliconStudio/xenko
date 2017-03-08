// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko
{
    /// <summary>
    /// Automatically copy Direct3D11 assemblies at the top level so that tools can find them.
    /// Note: we could use "probing" but it turns out to be slow (esp. on ExecServer where it slows down startup from almost 0 to 0.8 sec!)
    /// </summary>
    static class ToolAssemblyResolveModuleInitializer
    {
        // List of folders to copy
        private static readonly Dictionary<string, string> SearchPaths = new Dictionary<string, string>
        {
            { @"Direct3D11", @"." },
        };

        // Should execute before almost everything else
        [ModuleInitializer(-100000)]
        internal static void Setup()
        {
            foreach (var searchPath in SearchPaths)
            {
                var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchPath.Key);
                var destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, searchPath.Value);

                // Make sure output directory exist
                Directory.CreateDirectory(destPath);

                // Search source files
                foreach (var filename in Directory.EnumerateFiles(sourcePath, "*.*", SearchOption.AllDirectories))
                {
                    var sourceFile = new FileInfo(filename);
                    var destFile = new FileInfo(filename.Replace(sourcePath, destPath));

                    // Only copy if doesn't exist or newer
                    if (!destFile.Exists || sourceFile.LastWriteTime > destFile.LastWriteTime)
                    {
                        try
                        {
                            // now you can safely overwrite it
                            sourceFile.CopyTo(destFile.FullName, true);
                        }
                        catch
                        {
                            // Mute exceptions
                            // Not ideal, but better than crashing
                            // Let's see when it happens...
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();
                        }
                    }
                }
            }
        }
    }
}