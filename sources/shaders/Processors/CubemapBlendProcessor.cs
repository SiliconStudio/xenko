// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Processors
{
    public class CubemapBlendProcessor : EntityProcessor<CubemapBlendComponent>
    {
        #region Private members

        private readonly GraphicsDevice graphicsDevice;

        #endregion

        public Dictionary<Entity, CubemapBlendComponent> Cubemaps
        {
            get
            {
                return this.enabledEntities;
            }
        }

        public CubemapBlendProcessor(GraphicsDevice device)
            : base(new PropertyKey[] { CubemapBlendComponent.Key })
        {
            graphicsDevice = device;
        }

        #region Public methods
        
        /*
        /// <inheritdoc/>
        public override void Update(GameTime time)
        {
            base.Update(time);

            // TODO: check position changes and only update accordingly
        }
        */

        #endregion

        #region Protected methods

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, CubemapBlendComponent data)
        {
            base.OnEntityAdding(entity, data);
            data.Texture = TextureCube.New(graphicsDevice, data.Size, 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
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

    public struct CubemapBlendInformation
    {
        public CubemapBlendComponent Component;
        public int Size;
    }
}