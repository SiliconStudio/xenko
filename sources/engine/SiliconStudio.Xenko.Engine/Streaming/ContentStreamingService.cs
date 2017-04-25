// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Streaming
{
    public class ContentStreamingService : GameSystemBase
    {
        public ContentStreamingService(IServiceRegistry registry) : base(registry)
        {
            registry.AddService(typeof(ContentStreamingService), this);
        }
    }
}
