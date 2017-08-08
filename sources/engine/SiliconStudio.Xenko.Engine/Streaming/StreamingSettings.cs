// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <userdoc>
        /// Enable the streaming of resources. If disabled all the other settings are ignored.
        /// </userdoc>
        [DataMember]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        /// <inheritdoc cref="StreamingManager.ManagerUpdatesInterval"/>
        /// <userdoc>
        /// The interval between updates of the streaming manager.
        /// </userdoc>
        [DataMember]
        [DataMemberRange(0.001, 3)]
        public TimeSpan ManagerUpdatesInterval { get; set; } = TimeSpan.FromMilliseconds(33);

        /// <inheritdoc cref="StreamingManager.MaxResourcesPerUpdate"/>
        /// <userdoc>
        /// The maximum number of resources updated per streaming manager tick. Used to balance performance/streaming speed.
        /// </userdoc>
        [DataMember]
        [DataMemberRange(1, 0)]
        [DefaultValue(8)]
        public int MaxResourcesPerUpdate { get; set; } = 8;

        /// <inheritdoc cref="StreamingManager.ResourceLiveTimeout"/>
        /// <userdoc>
        /// Resources that aren't used for a while are downscaled in quality.
        /// </userdoc>
        [DataMember]
        [DataMemberRange(0, 3)]
        public TimeSpan ResourceLiveTimeout { get; set; } = TimeSpan.FromSeconds(8);

        /// <inheritdoc cref="StreamingManager.TargetedMemoryBudget"/>
        /// <userdoc>
        /// The targeted memory budget of the streaming system in MB. If the memory allocated by the streaming system is under this budget, it will not try to unload resources that are not visible.
        /// </userdoc>
        [DataMember]
        [DataMemberRange(0, 0)]
        [DefaultValue(512)]
        public int TargetedMemoryBudget { get; set; } = 512;
    }
}
