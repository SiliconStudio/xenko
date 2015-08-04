// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.DataOld
{
    public class GpuRasterizerStateSerializer : ContentSerializerBase<RasterizerState>
    {
        public GraphicsDevice graphicsDevice;

        public GpuRasterizerStateSerializer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, RasterizerState rasterizerState)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                var rasterizerStateDescription = default(RasterizerStateDescription);
                stream.Serialize(ref rasterizerStateDescription, context.Mode);
                rasterizerState = RasterizerState.New(graphicsDevice, rasterizerStateDescription);
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
