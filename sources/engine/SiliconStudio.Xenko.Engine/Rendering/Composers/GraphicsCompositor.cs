using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataSerializerGlobal(typeof(ReferenceSerializer<GraphicsCompositor>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializerWithReuse<GraphicsCompositor>))]
    [DataContract]
    public class GraphicsCompositor
    {
        [Obsolete]
        public ISceneGraphicsCompositor Instance { get; set; }

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
        public IGraphicsCompositorTopPart TopLevel { get; set; }
    }
}