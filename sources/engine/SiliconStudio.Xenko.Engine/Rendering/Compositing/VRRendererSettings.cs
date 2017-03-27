using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
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

    [DataContract]
    public class VRRendererSettings
    {
        [DataMember(10)]
        public bool Enabled { get; set; }

        [DataMember(20)]
        [DefaultValue(true)]
        public bool IgnoreCameraRotation { get; set; } = true;

        [DataMember(30)]
        public List<VRApi> RequiredApis { get; } = new List<VRApi>();

        [DataMember(40)]
        public float ResolutionScale { get; set; } = 1.0f;

        [DataMember(50)]
        public List<VROverlayRenderer> Overlays { get; } = new List<VROverlayRenderer>();

        [DataMemberIgnore]
        public RenderView[] RenderViews = { new RenderView(), new RenderView() };

        [DataMemberIgnore]
        public VRDevice VRDevice;

        [DataMemberIgnore]
        public ImageScaler MirrorScaler = new ImageScaler();
    }
}