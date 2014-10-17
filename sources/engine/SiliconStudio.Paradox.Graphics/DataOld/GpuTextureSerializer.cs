using System;
using Paradox.Framework.Diagnostics;
using Paradox.Framework.Serialization;
using Paradox.Framework.Serialization.Contents;

namespace Paradox.Framework.Graphics.Data
{
    public class GpuTextureSerializer : ContentSerializerBase<Texture>
    {
        private readonly GraphicsDevice graphicsDevice;

        public GpuTextureSerializer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, ref Texture texture, ref object intermediateData)
        {
            if (context.RootContext.ArchiveMode == ArchiveMode.Serialize)
                throw new NotImplementedException();

            var textureData = context.RootContext.AssetManager.Load<Image>(context.Url);
            try
            {
                texture = Texture.New(graphicsDevice, textureData);
            }
            catch (Exception ex)
            {
                Logger.GetLogger("GPUTexture").Error("Unable to load Texture {0}. See Debug for more information", ex, context.Url);
            }
            finally
            {
                context.RootContext.AssetManager.Unload(textureData);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return null;
        }
    }
}