// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A camera renderer.
    /// </summary>
    [DataContract("SceneCameraRenderer")]
    [Display("Render Camera")]
    public sealed class SceneCameraRenderer : SceneRendererViewportBase
    {
        /// <summary>
        /// Property key to access the current <see cref="SceneCameraRenderer"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneCameraRenderer> Current = new PropertyKey<SceneCameraRenderer>("SceneCameraRenderer.Current", typeof(SceneCameraRenderer));

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneCameraRenderer"/> class.
        /// </summary>
        public SceneCameraRenderer()
        {
            Mode = new CameraRendererModeForward();
            PreRenderers = new SafeList<IGraphicsRenderer>();
            PostRenderers = new SafeList<IGraphicsRenderer>();
            CullingMask = EntityGroupMask.All;
            CullingMode = CameraCullingMode.Frustum;
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        /// <userdoc>The type of rendering to  perform</userdoc>
        [DataMember(10)]
        [NotNull]
        public CameraRendererMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the camera.
        /// </summary>
        /// <value>The camera.</value>
        /// <userdoc>The camera to use to render the scene.</userdoc>
        [DataMember(20)]
        public SceneCameraSlotIndex Camera { get; set; } = new SceneCameraSlotIndex(0);

        // TODO GRAPHICS REFACTOR should we mark this obsolete? should we hide this from editor?
        /// <summary>
        /// Gets or sets the culling mask.
        /// </summary>
        /// <value>The culling mask.</value>
        /// <userdoc>The groups of entities that should be rendered by the renderer.</userdoc>
        [DataMember(30)]
        [DefaultValue(EntityGroupMask.All)]
        public EntityGroupMask CullingMask { get; set; }

        /// <summary>
        /// Gets or sets the culling mode.
        /// </summary>
        /// <value>The culling mode.</value>
        /// <userdoc>The type of culling to perform on entities. Culling consist into skipping not visible or insignificant entities during rendering in order to improve performances.</userdoc>
        [DataMember(40)]
        [DefaultValue(CameraCullingMode.Frustum)]
        public CameraCullingMode CullingMode { get; set; }

        /// <summary>
        /// Gets the pre-renderers attached to this instance that are called before rendering this camera.
        /// </summary>
        /// <value>The pre renderers.</value>
        [DataMemberIgnore]
        public SafeList<IGraphicsRenderer> PreRenderers { get; private set; }

        /// <summary>
        /// Gets the post-renderers attached to this instance that are called after rendering this camera.
        /// </summary>
        /// <value>The post renderers.</value>
        [DataMemberIgnore]
        public SafeList<IGraphicsRenderer> PostRenderers { get; private set; }

        public override void Collect(RenderContext context)
        {
            base.Collect(context);

            // Early exit if some properties are null
            if (Mode == null)
            {
                return;
            }

            // Gets the current camera state from the slot
            var camera = context.GetCameraFromSlot(Camera);

            //TODO camera can be null but we still push it as null... review me please.
            //TODO this is needed or if we have no camera we don't render anything e.g. UI only

            // Draw this camera.
            using (context.PushTagAndRestore(Current, this))
            using (context.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                Mode.Collect(context);
            }
        }

        protected override void DrawCore(RenderDrawContext context, RenderFrame output)
        {
            // Early exit if some properties are null
            if (Mode == null)
            {
                return; 
            }

            // Gets the current camera state from the slot
            var camera = context.RenderContext.GetCameraFromSlot(Camera);

            //TODO camera can be null but we still push it as null... review me please.
            //TODO this is needed or if we have no camera we don't render anything e.g. UI only

            // Draw this camera.
            using (context.RenderContext.PushTagAndRestore(Current, this))
            using (context.RenderContext.PushTagAndRestore(CameraComponentRendererExtensions.Current, camera))
            {
                // Run all pre-renderers
                foreach (var renderer in PreRenderers)
                {
                    renderer.Draw(context);
                }

                // Draw the scene based on its drawing mode (e.g. implementation forward or deferred... etc.)
                Mode.Draw(context);

                // Run all post-renderers
                foreach (var renderer in PostRenderers)
                {
                    renderer.Draw(context);
                }
            }
        }
    }
}
