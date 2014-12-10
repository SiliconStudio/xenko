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
    /// Keeps track of active cubemap blending locations.
    /// </summary>
    public class CubemapBlendProcessor : EntityProcessor<CubemapBlendComponent>
    {
        #region Private members

        private readonly GraphicsDevice graphicsDevice;

        #endregion

        #region Public properties

        /// <summary>
        /// The enabled cubemap blends
        /// </summary>
        public Dictionary<Entity, CubemapBlendComponent> Cubemaps
        {
            get
            {
                return this.enabledEntities;
            }
        }

        #endregion

        #region Constructor

        public CubemapBlendProcessor(GraphicsDevice device)
            : base(new PropertyKey[] { CubemapBlendComponent.Key })
        {
            graphicsDevice = device;
        }

        #endregion

        #region Protected methods

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, CubemapBlendComponent data)
        {
            base.OnEntityAdding(entity, data);
            data.Texture = Texture.NewCube(graphicsDevice, data.Size, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);

            // add parameter to model
            var model = entity.Get<ModelComponent>();
            if (model != null)
                model.Parameters.Set(data.TextureKey ?? TexturingKeys.TextureCube0, data.Texture);
        }

        /// <inheritdoc/>
        protected override void OnEntityRemoved(Entity entity, CubemapBlendComponent data)
        {
            base.OnEntityRemoved(entity, data);
            // TODO: remove texture?
        }

        /// <inheritdoc/>
        protected override CubemapBlendComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get<CubemapBlendComponent>();
        }

        #endregion
    }
}