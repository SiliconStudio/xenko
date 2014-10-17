using System;
using Paradox.Framework.Serialization;
using Paradox.Framework.Serialization.Contents;

namespace Paradox.Framework.Graphics.Data
{
    public class GpuSamplerStateSerializer : ContentSerializerBase<SamplerState>
    {
        public GraphicsDevice graphicsDevice;

        public GpuSamplerStateSerializer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, ref SamplerState samplerState, ref object intermediateData)
        {
            if (context.RootContext.ArchiveMode == ArchiveMode.Deserialize)
            {
                var samplerStateDescription = default(SamplerStateDescription);
                context.SerializationStream.Serialize(ref samplerStateDescription, context.RootContext.ArchiveMode);
                samplerState = SamplerState.New(graphicsDevice, samplerStateDescription);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return null;
        }
    }
}