// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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
