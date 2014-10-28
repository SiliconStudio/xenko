// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Processors
{
    public class CubemapSourceProcessor : EntityProcessor<CubemapSourceComponent>
    {
        #region Private members

        private readonly GraphicsDevice graphicsDevice;

        #endregion

        public Dictionary<Entity, CubemapSourceComponent> Cubemaps
        {
            get
            {
                return this.enabledEntities;
            }
        }


        public CubemapSourceProcessor(GraphicsDevice device)
            : base(new PropertyKey[] { CubemapSourceComponent.Key })
        {
            graphicsDevice = device;
        }

        #region Protected methods

        /// <inheritdoc/>
        protected override void OnEntityAdding(Entity entity, CubemapSourceComponent data)
        {
            base.OnEntityAdding(entity, data);
            if (data.IsDynamic)
            {
                data.Texture = TextureCube.New(graphicsDevice, data.Size, data.GenerateMips? 0 : 1, PixelFormat.R8G8B8A8_UNorm, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
                // TODO: change this depending on single pass rendering or not
                data.DepthStencil = Texture2D.New(graphicsDevice, data.Size, data.Size, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil, 6).ToDepthStencilBuffer(false);

                // TODO: change this
                var camera = entity.Get<CameraComponent>();
                if (camera == null)
                {
                    camera = new CameraComponent();
                    entity.Add(camera);
                }
                var targetEntity = new Entity() { new TransformationComponent() };
                camera.AspectRatio = 1;
                camera.FarPlane = 0.1f;
                camera.NearPlane = 100;
                camera.VerticalFieldOfView = MathUtil.PiOverTwo;
                camera.Target = targetEntity;
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