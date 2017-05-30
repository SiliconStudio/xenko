// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.VirtualReality;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    [DataContract]
    public class VROverlayRenderer
    {
        [DataMember(10)]
        public Texture Texture;

        [DataMember(20)]
        public Vector3 LocalPosition;

        [DataMember(30)]
        public Quaternion LocalRotation = Quaternion.Identity;

        [DataMember(40)]
        public Vector2 SurfaceSize = Vector2.One;

        [DataMember(50)]
        public bool FollowsHeadRotation;

        [DataMemberIgnore]
        public VROverlay Overlay;
    }
}
