// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.VisualStudio.Package.Tests
{
    public class Module
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            // Override search path since we are in a unit test directory
            DirectoryHelper.PackageDirectoryOverride = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..");
        }
    }
}
