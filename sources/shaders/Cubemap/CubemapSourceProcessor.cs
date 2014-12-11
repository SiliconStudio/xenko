// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Cubemap
{
    /// <summary>
    /// Keeps tracks of the active cubemaps.
    /// </summary>
    public sealed class CubemapSourceProcessor : EntityProcessor<CubemapSourceComponent>
    {
        #region Private members

        private readonly GraphicsDevice graphicsDevice;

        #endregion

        #region Public properties

        /// <summary>
        /// The active cubemaps.
        /// </summary>
        public Dictionary<Entity, CubemapSourceComponent> Cubemaps
        {
            get
            {
                return this.enabledEntities;
            }
        }

        #endregion

        #region Constructor

        public CubemapSourceProcessor(GraphicsDevice device)
            : base(new PropertyKey[] { CubemapSourceComponent.Key })
        {
            graphicsDevice = device;
        }

        #endregion

        #region Protected methods

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, CubemapSourceComponent data)
        {
            base.OnEntityAdding(entity, data);
            if (data.IsDynamic)
            {
                // TODO: move texture creation when it is actually needed (first time use)?
                data.Texture = Texture.NewCube(graphicsDevice, data.Size, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            }
        }

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, CubemapSourceComponent data)
        {
            base.OnEntityRemoved(entity, data);
            // TODO: remove texture?
        }

        /// <inheritdoc/>
        protected override CubemapSourceComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<CubemapSourceComponent>();
        }

        #endregion
    }
}