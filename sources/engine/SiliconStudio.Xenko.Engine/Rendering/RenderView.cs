// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines a view used during rendering. This is usually a frustum and some camera parameters.
    /// </summary>
    public class RenderView
    {
        /// <summary>
        /// The part of the view specific to a given <see cref="RootRenderFeature"/>.
        /// </summary>
        public List<RenderViewFeature> Features = new List<RenderViewFeature>();

        /// <summary>
        /// List of data sepcific to each <see cref="RenderStage"/> for this <see cref="RenderView"/>.
        /// </summary>
        public List<RenderViewStage> RenderStages = new List<RenderViewStage>();

        /// <summary>
        /// List of visible render objects.
        /// </summary>
        public List<RenderObject> RenderObjects = new List<RenderObject>();

        /// <summary>
        /// Index in <see cref="RenderSystem.Views"/>.
        /// </summary>
        public int Index = -1;

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

        // TODO GRAPHICS REFACTOR we might want to remove some of the following data
        /// <summary>
        /// The camera for this view. 
        /// </summary>
        public CameraComponent Camera;

        public SceneInstance SceneInstance;

        public SceneCameraRenderer SceneCameraRenderer;

        public SceneCameraSlotCollection SceneCameraSlotCollection;

        public override string ToString()
        {
            return $"RenderView ({Features.Sum(x => x.ViewObjectNodes.Count)} objects, {Features.Sum(x => x.RenderNodes.Count)} render nodes, {RenderStages.Count} stages)";
        }

        public void UpdateCameraToRenderView()
        {
            // TODO: Currently set up during Collect/Prepare/Draw. Should be initialized before
            if (SceneCameraRenderer == null)
                return;

            Camera = SceneCameraSlotCollection.GetCamera(SceneCameraRenderer.Camera);

            if (Camera == null)
                return;

            // Setup viewport size
            var currentViewport = SceneCameraRenderer.ComputedViewport;
            var aspectRatio = currentViewport.AspectRatio;

            if (Camera.UseCustomAspectRatio && !Camera.AddLetterboxPillarbox)
            {
                aspectRatio = Camera.AspectRatio;
            }

            // If the aspect ratio is calculated automatically from the current viewport, update matrices here
            Camera.Update(aspectRatio);

            View = Camera.ViewMatrix;
            Projection = Camera.ProjectionMatrix;

            Matrix.Multiply(ref View, ref Projection, out ViewProjection);
        }
    }
}
