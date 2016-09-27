// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NuGet;

namespace SiliconStudio.PackageManager
{
    public interface INugetLogger
    {
        void Log(NugetMessageLevel level, string message);
    }
}
