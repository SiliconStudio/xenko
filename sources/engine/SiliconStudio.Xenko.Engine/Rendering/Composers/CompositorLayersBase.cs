// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    [DataContract]
    public abstract class CompositorLayersBase : RendererBase
    {
        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        /// <userdoc>The sequence of graphic layers to incorporate into the pipeline</userdoc>
        [DataMember(20)]
        [Category]
        [MemberCollection(CanReorderItems = true)]
        [NotNullItems]
        public SceneGraphicsLayerCollection Layers { get; private set; }

        /// <summary>
        /// Gets the master layer.
        /// </summary>
        /// <value>The master layer.</value>
        /// <userdoc>The main layer of the pipeline. Its output is the window back buffer.</userdoc>
        [DataMember(30)]
        [Category]
        public SceneGraphicsLayer Master { get; private set; }

        protected CompositorLayersBase()
        {
            Layers = new SceneGraphicsLayerCollection();
            Master = new SceneGraphicsLayer
            {
                Output = new MasterRenderFrameProvider(),
                IsMaster = true
            };
        }

        protected override void Unload()
        {
            Layers.Dispose();
            Master.Dispose();

            base.Unload();
        }
    }
}