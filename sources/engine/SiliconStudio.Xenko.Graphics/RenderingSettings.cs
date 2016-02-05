// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Graphics
{
    [DataContract]
    [Display("Rendering Settings")]
    public class RenderingSettings : Configuration
    {
        [DataMember(0)]
        public int DefaultBackBufferWidth = 1280;

        [DataMember(10)]
        public int DefaultBackBufferHeight = 720;

        [DataMember(20)]
        public GraphicsProfile DefaultGraphicsProfile = GraphicsProfile.Level_10_0;

        [DataMember(30)]
        public ColorSpace ColorSpace = ColorSpace.Linear;
    }
}