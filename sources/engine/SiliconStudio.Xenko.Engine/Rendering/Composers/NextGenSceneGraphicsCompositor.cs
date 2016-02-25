// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataContract("NextGenSceneGraphicsCompositor")]
    public sealed class NextGenSceneGraphicsCompositor : RendererBase, ISceneGraphicsCompositor, IPipeline
    {
        /// <summary>
        /// Gets the cameras used by this composition.
        /// </summary>
        /// <value>The cameras.</value>
        /// <userdoc>The list of cameras used in the graphic pipeline</userdoc>
        [DataMember(10)]
        [Category]
        public SceneCameraSlotCollection Cameras { get; } = new SceneCameraSlotCollection();

        /// <summary>
        /// Gets the list of pipelines.
        /// </summary>
        [DataMember(20)]
        [Category]
        public PipelineCollection Pipelines { get; } = new PipelineCollection();

        protected override void DrawCore(RenderDrawContext context)
        {
            using (context.RenderContext.PushTagAndRestore(SceneCameraSlotCollection.Current, Cameras))
            {
                foreach (var pipeline in Pipelines)
                {
                    pipeline.Draw(context);
                }
            }
        }
    }
}