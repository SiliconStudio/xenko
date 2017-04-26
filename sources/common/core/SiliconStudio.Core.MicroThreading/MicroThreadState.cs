// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Core.MicroThreading
{
    public enum MicroThreadState : int
    {
        None,
        Starting,
        Running,
        Completed,
        Canceled,
        Failed,
    }
}
