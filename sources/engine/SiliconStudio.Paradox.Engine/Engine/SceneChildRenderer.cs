// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A renderer for a child scene defined by a <see cref="ChildSceneComponent"/>.
    /// </summary>
    [DataContract("SceneChildRenderer")]
    [Display("Render Child Scene")]
    public sealed class SceneChildRenderer : SceneRendererBase
    {
        private EntityManager currentEntityManager;
        private SceneChildProcessor sceneChildProcessor;

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
        [DataMember(10)]
        public ChildSceneComponent ChildScene { get; set; }

        /// <summary>
        /// Gets or sets the graphics compositor override, allowing to override the composition of the scene.
        /// </summary>
        /// <value>The graphics compositor override.</value>
        [DataMemberIgnore]
        public ISceneGraphicsCompositor GraphicsCompositorOverride { get; set; } // Overrides are accessible only at runtime

        protected override void InitializeCore()
        {
            base.InitializeCore();
            currentEntityManager = Context.Tags.Get(SceneInstance.Current);
        }

        protected override void Destroy()
        {
            if (GraphicsCompositorOverride != null)
            {
                GraphicsCompositorOverride.Dispose();
                GraphicsCompositorOverride = null;
            }

            base.Destroy();
        }

        protected override void DrawCore(RenderContext context, RenderFrame output)
        {
            if (ChildScene == null || !ChildScene.Enabled)
            {
                return;
            }

            sceneChildProcessor = sceneChildProcessor  ?? currentEntityManager.GetProcessor<SceneChildProcessor>();

            if (sceneChildProcessor == null)
            {
                return;
            }

            SceneInstance sceneInstance = sceneChildProcessor.GetSceneInstance(ChildScene);
            if (sceneInstance != null)
            {
                sceneInstance.Draw(context, output, GraphicsCompositorOverride);
            }
        }
    }
}