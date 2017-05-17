// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Runtime.Serialization;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Core.Streaming
{
    /// <summary>
    /// Interface for Streaming Manager service.
    /// </summary>
    public interface IStreamingManager
    {
        void FullyLoadResource(object obj);
    }
}
