// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Streaming
{
    [DataContract]
    [Display("Streaming")]
    public sealed class StreamingSettings : Configuration
    {
        /// <inheritdoc cref="StreamingManager.StreamingEnabled"/>
        [DataMember]
        public bool Enabled { get; set; } = true;

        /// <inheritdoc cref="StreamingManager.ManagerUpdatesInterval"/>
        [DataMember]
        [DataMemberRange(0.001, 3)]
        public TimeSpan ManagerUpdatesInterval { get; set; } = TimeSpan.FromMilliseconds(33);

        /// <inheritdoc cref="StreamingManager.ResourceLiveTimeout"/>
        [DataMember]
        [DataMemberRange(1.0, 0)]
        public TimeSpan ResourceLiveTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <inheritdoc cref="StreamingManager.MaxResourcesPerUpdate"/>
        [DataMember]
        [DataMemberRange(1.0, 0)]
        public int MaxResourcesPerUpdate { get; set; } = 8;
    }
}
