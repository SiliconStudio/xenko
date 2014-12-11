// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.Data
{
    public class GpuTextureSerializer2 : ContentSerializerBase<Texture>
    {
        private readonly GraphicsDevice graphicsDevice;

        public override Type SerializationType
        {
            get
            {
                return typeof(Image);
            }
        }

        public GpuTextureSerializer2(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, ref Texture texture)
        {
            if (context.Mode == ArchiveMode.Serialize)
                throw new NotImplementedException();

            var assetManager = context.AssetManager;
            var url = context.Url;

            using (var textureData = Image.Load(stream.NativeStream))
            {
                try
                {
                    texture = Texture.New(graphicsDevice, textureData);

                    // Setup reload callback (reload from asset manager)
                    texture.Reload = (graphicsResource) =>
                    {
                        // TODO: Avoid loading/unloading the same data
                        var textureDataReloaded = assetManager.Load<Image>(url);
                        ((Texture)graphicsResource).Recreate(textureDataReloaded.ToDataBox());
                        assetManager.Unload(textureDataReloaded);
                    };
                }
                catch (Exception ex)
                {
                    GlobalLogger.GetLogger("GPUTexture").Error("Unable to load Texture {0}. See Debug for more information", ex, context.Url);
                }
            }
        }

        public override object Construct(SiliconStudio.Core.Serialization.Contents.ContentSerializerContext context)
        {
            return null;
        }
    }
}