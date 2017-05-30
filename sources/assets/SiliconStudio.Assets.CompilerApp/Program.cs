// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

using SiliconStudio.Core;

namespace SiliconStudio.Assets.CompilerApp
{
    class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                // Set the SiliconStudioXenkoDir environment variable
                var installDir = DirectoryHelper.GetInstallationDirectory("Xenko");
                Environment.SetEnvironmentVariable("SiliconStudioXenkoDir", installDir);

                var packageBuilder = new PackageBuilderApp();
                var returnValue =  packageBuilder.Run(args);

                return returnValue;
            }
            catch (Exception)
            {
                // Console.WriteLine("Unexpected exception in AssetCompiler: {0}", ex);
                return 1;
            }
            finally
            {
                // Free all native library loaded from the process
                // We cannot free native libraries are some of them are loaded from static module initializer
                // NativeLibrary.UnLoadAll();
            }
        }
    }
}
