// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.MicroThreading
{
    internal interface IMicroThreadSynchronizationContext
    {
        MicroThread MicroThread { get; }
    }
}
