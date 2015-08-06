// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Assets.CompilerApp
{
    class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var packageBuilder = new PackageBuilderApp();
                return packageBuilder.Run(args);
            }
            finally
            {
                // Free all native library loaded from the process
                NativeLibrary.UnLoadAll();
            }
        }
    }
}
