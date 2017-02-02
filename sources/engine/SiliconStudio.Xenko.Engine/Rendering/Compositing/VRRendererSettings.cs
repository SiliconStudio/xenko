using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.VirtualReality;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    [DataContract]
    public class VRRendererSettings
    {
        public bool Enabled { get; set; }

        public List<VRApi> RequiredApis { get; } = new List<VRApi>();

        [DataMemberIgnore]
        public RenderView[] RenderViews = { new RenderView(), new RenderView() };

        [DataMemberIgnore]
        public VRDevice VRDevice;
    }
}