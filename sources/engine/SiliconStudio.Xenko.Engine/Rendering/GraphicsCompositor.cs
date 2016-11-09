using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering
{
    [DataSerializerGlobal(typeof(ReferenceSerializer<GraphicsCompositor>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<GraphicsCompositor>))]
    [DataContract]
    public class GraphicsCompositor
    {
        public List<RenderStage> RenderStages { get; } = new List<RenderStage>();

        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();
    }
}