// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("SceneGraphicsCompositorLayers")]
    [Display("Layers")]
    public sealed class SceneGraphicsCompositorLayers : PipelineCompositorLayers, ISceneGraphicsCompositor, IPipeline
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SceneGraphicsCompositorLayers"/> class.
        /// </summary>
        public SceneGraphicsCompositorLayers()
        {
            Cameras = new SceneCameraSlotCollection();
        }

        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [DataMember(10)]
        [Category]
        [NotNullItems]
        public SceneCameraSlotCollection Cameras { get; private set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            RenderSystem.Initialize(Context);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            using (context.RenderContext.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
            {
                base.DrawCore(context);
            }
        }
    }
}