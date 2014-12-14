// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Graphics.DataOld
{
    public class GpuBlendStateSerializer : ContentSerializerBase<BlendState>
    {
        public GraphicsDevice graphicsDevice;

        public GpuBlendStateSerializer(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, BlendState blendState)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                var blendStateDescription = default(BlendStateDescription);
                stream.Serialize(ref blendStateDescription, context.Mode);
                blendState = BlendState.New(graphicsDevice, blendStateDescription);
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
