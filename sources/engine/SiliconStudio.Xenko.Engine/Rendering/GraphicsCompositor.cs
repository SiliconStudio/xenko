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
        /// <summary>
        /// The list of render stages.
        /// </summary>
        public List<RenderStage> RenderStages { get; } = new List<RenderStage>();

        /// <summary>
        /// The list of render features.
        /// </summary>
        public List<RootRenderFeature> RenderFeatures { get; } = new List<RootRenderFeature>();

        /// <summary>
        /// The code and values defined by this graphics compositor.
        /// </summary>
        public GraphicsCompositorCode Code { get; set; }
    }
}