// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A renderer in charge of displaying some entities of the scene.
    /// </summary>
    public abstract class SceneEntityRenderer : SceneRendererViewportBase
    {
        /// <summary>
        /// Property key to access the current <see cref="SceneCameraRenderer"/> from <see cref="RenderContext.Tags"/>.
        /// </summary>
        public static readonly PropertyKey<SceneEntityRenderer> Current = new PropertyKey<SceneEntityRenderer>("SceneEntityRenderer.Current", typeof(SceneEntityRenderer));

        /// <summary>
        /// Creates a new instance of <see cref="SceneEntityRenderer"/>.
        /// </summary>
        protected SceneEntityRenderer()
        {
            CullingMask = EntityGroup.All;
        }

        /// <summary>
        /// Gets or sets the culling mask.
        /// </summary>
        /// <value>The culling mask.</value>
        [DataMember(30)]
        [DefaultValue(EntityGroup.All)]
        public EntityGroup CullingMask { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the current rendering is for picking or not.
        /// </summary>
        [DataMemberIgnore]
        public bool IsPickingMode { get; set; }
    }
}