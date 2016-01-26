using System.Collections.Generic;

namespace RenderArchitecture
{
    /// <summary>
    /// Compute and upload skinning info.
    /// </summary>
    public class SkinningRenderFeature : SubRenderFeature
    {
        class CachePerMeshInfo
        {
            // Copied during Extract
            public object NodePositions;
            // Computed during Prepare
            public object BlendIndices;
        }

        /// <inheritdoc/>
        public override void Extract()
        {
            // Copy node infos to internal structures
        }

        /// <inheritdoc/>
        public override void Prepare()
        {
            // (Evaluate anim curves?)
            // Compute matrices
        }

        /// <inheritdoc/>
        public override void Draw(RenderView renderView, RenderViewStage renderViewStage, int startIndex, int endIndex)
        {
            // Upload data to CB
        }
    }
}