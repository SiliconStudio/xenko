// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Processors;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Composers;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// A renderer for a child scene defined by a <see cref="ChildSceneComponent"/>.
    /// </summary>
    [DataContract("SceneChildRenderer")]
    [Display("Render Child Scene")]
    public sealed class SceneChildRenderer : SceneRendererBase
    {
        private SceneInstance currentSceneInstance;
        private ChildSceneProcessor childSceneProcessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildRenderer"/> class.
        /// </summary>
        public SceneChildRenderer() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneChildRenderer"/> class.
        /// </summary>
        /// <param name="childScene">The scene child.</param>
        public SceneChildRenderer(ChildSceneComponent childScene)
        {
            ChildScene = childScene;
        }

        /// <summary>
        /// Gets or sets the scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <userdoc>The entity of the scene containing the child scene to render.</userdoc>
        [DataMember(10)]
        public ChildSceneComponent ChildScene { get; set; }

        /// <summary>
        /// Gets or sets the graphics compositor override, allowing to override the composition of the scene.
        /// </summary>
        /// <value>The graphics compositor override.</value>
        [DataMemberIgnore]
        public ISceneGraphicsCompositor GraphicsCompositorOverride { get; set; } // Overrides are accessible only at runtime

        protected override void Destroy()
        {
            if (GraphicsCompositorOverride != null)
            {
                GraphicsCompositorOverride.Dispose();
                GraphicsCompositorOverride = null;
            }

            base.Destroy();
        }

        private SceneInstance GetChildSceneInstance()
        {
            if (ChildScene == null || !ChildScene.Enabled)
            {
                return null;
            }

            currentSceneInstance = SceneInstance.GetCurrent(Context);

            childSceneProcessor = childSceneProcessor ?? currentSceneInstance.GetProcessor<ChildSceneProcessor>();

            return childSceneProcessor?.GetSceneInstance(ChildScene);
        }

        protected override void DrawCore(RenderDrawContext context, RenderFrame output)
        {
            var sceneInstance = GetChildSceneInstance();
            if (sceneInstance == null)
                return;

            // Draw scene recursively
            sceneInstance.Draw(context, output, GraphicsCompositorOverride);
        }
    }
}