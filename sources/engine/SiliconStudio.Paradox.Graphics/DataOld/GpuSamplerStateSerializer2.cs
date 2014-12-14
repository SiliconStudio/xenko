// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.Data
{
    public class GpuSamplerStateSerializer2 : ContentSerializerBase<SamplerState>
    {
        public GraphicsDevice graphicsDevice;

        public GpuSamplerStateSerializer2(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, SamplerState samplerState)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                var samplerStateDescription = default(SamplerStateDescription);
                stream.Serialize(ref samplerStateDescription, context.Mode);
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