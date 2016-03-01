using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    public class RenderViewStage
    {
        public readonly RenderStage RenderStage;

        /// <summary>
        /// List of render nodes. It might cover multiple RenderStage and RootRenderFeature. RenderStages contains RenderStage range information.
        /// Used mostly for sorting and rendering.
        /// </summary>
        public readonly List<RenderNodeFeatureReference> RenderNodes = new List<RenderNodeFeatureReference>();

        public RenderNodeFeatureReference[] SortedRenderNodes;

        public RenderViewStage(RenderStage renderStage)
        {
            RenderStage = renderStage;
        }

        public static implicit operator RenderViewStage(RenderStage renderStage)
        {
            return new RenderViewStage(renderStage);
        }
    }

    /// <summary>
    /// Defines a view used during rendering. This is usually a frustum and some camera parameters.
    /// </summary>
    public class RenderView
    {
        /// <summary>
        /// The part of the view specific to a given <see cref="RootRenderFeature"/>.
        /// </summary>
        public List<RenderViewFeature> Features = new List<RenderViewFeature>();

        public List<RenderViewStage> RenderStages = new List<RenderViewStage>();

        /// <summary>
        /// List of visible render objects.
        /// </summary>
        public List<RenderObject> RenderObjects = new List<RenderObject>();

        /// <summary>
        /// The camera for this view. 
        /// </summary>
        public CameraComponent Camera;

        public SceneInstance SceneInstance;

        public SceneCameraRenderer SceneCameraRenderer;

        public SceneCameraSlotCollection SceneCameraSlotCollection;

        internal float MinimumDistance;

        internal float MaximumDistance;

        /// <summary>
        /// The view matrix for this view.
        /// </summary>
        public Matrix View = Matrix.Identity;

        /// <summary>
        /// The projection matrix for this view.
        /// </summary>
        public Matrix Projection = Matrix.Identity;

        /// <summary>
        /// The view projection matrix for this view.
        /// </summary>
        public Matrix ViewProjection;

        /// <summary>
        /// Index in <see cref="NextGenRenderSystem.Views"/>.
        /// </summary>
        public int Index = -1;

        public override string ToString()
        {
            return $"RenderView ({Features.Sum(x => x.ViewObjectNodes.Count)} objects, {Features.Sum(x => x.RenderNodes.Count)} render nodes, {RenderStages.Count} stages)";
        }
    }
}