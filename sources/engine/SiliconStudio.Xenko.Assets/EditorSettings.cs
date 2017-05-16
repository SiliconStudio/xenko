// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Assets
{
    [DataContract]
    [Display("Editor Settings")]
    public class EditorSettings : Configuration
    {
        public EditorSettings()
        {
            OfflineOnly = true;
        }

        /// <userdoc>
        /// This setting applies to thumbnails and asset previews
        /// </userdoc>
        [DataMember(0)]
        public RenderingMode RenderingMode = RenderingMode.HDR;

        /// <userdoc>
        /// The framerate at which Xenko displays animations. Animation data itself isn't affected.
        /// </userdoc>
        [DataMember(10)]
        [DefaultValue(30)]
        [DataMemberRange(1, 0)]
        public uint AnimationFrameRate = 30;
    }
}
