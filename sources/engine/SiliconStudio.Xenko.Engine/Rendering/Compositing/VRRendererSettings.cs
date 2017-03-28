using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.VirtualReality;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    [DataContract]
    public class VRDeviceDescription
    {
        [DataMember(10)]
        public VRApi Api { get; set; }

        [DataMember(20)]
        public float ResolutionScale { get; set; } = 1.0f;
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
        public List<VRDeviceDescription> RequiredApis { get; } = new List<VRDeviceDescription>();

        [DataMember(40)]
        public List<VROverlayRenderer> Overlays { get; } = new List<VROverlayRenderer>();

        [DataMemberIgnore]
        public RenderView[] RenderViews = { new RenderView(), new RenderView() };

        [DataMemberIgnore]
        public VRDevice VRDevice;

        [DataMemberIgnore]
        public ImageScaler MirrorScaler = new ImageScaler();
    }
}