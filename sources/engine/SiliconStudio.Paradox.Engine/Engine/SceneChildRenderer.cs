// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A renderer for a child scene defined by a <see cref="SceneChildComponent"/>.
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
        public SceneChildRenderer()
        {
            Output = new CurrentRenderFrameProvider();
        }

        /// <summary>
        /// Gets or sets the scene.
        /// </summary>
        /// <value>The scene.</value>
        [DataMember(10)]
        public SceneChildComponent SceneChild { get; set; }

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>The output.</value>
        [DataMember(20)]
        [NotNull]
        public IRenderFrameOutput Output { get; set; }

        public override void Load(RenderContext context)
        {
            base.Load(context);
            currentEntityManager = context.Tags.Get(SceneInstance.Current);
        }

        protected override void DrawCore(RenderContext context)
        {
            if (Output == null)
            {
                return;
            }

            sceneChildProcessor = sceneChildProcessor  ?? currentEntityManager.GetProcessor<SceneChildProcessor>();

            if (sceneChildProcessor == null)
            {
                return;
            }

            var renderFrame = Output.GetRenderFrame(context);

            if (renderFrame == null)
            {
                return;
            }

            foreach (var sceneInstance in sceneChildProcessor.Scenes)
            {
                sceneInstance.Draw(context, renderFrame);
            }
        }
    }
}