// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.IO
{
    [Flags]
    public enum VirtualWatcherChangeTypes
    {
        Created = 1,

        Deleted = 2,

        Changed = 4,

        Renamed = 8,

        All = 15,
    }
}