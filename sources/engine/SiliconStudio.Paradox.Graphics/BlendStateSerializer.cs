// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Serializer for <see cref="BlendState"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(BlendStateSerializer))]
    public class BlendStateSerializer : DataSerializer<BlendState>
    {
        public override void Serialize(ref BlendState blendState, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var blendStateDescription = blendState.Description;
                stream.Serialize(ref blendStateDescription, mode);
            }
            else
            {
                // If we have a graphics context, we will instantiate GPU state, otherwise a CPU one.
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services != null ? services.GetSafeServiceAs<IGraphicsDeviceService>() : null;

                var blendStateDescription = BlendStateDescription.Default;
                stream.Serialize(ref blendStateDescription, mode);
                blendState = graphicsDeviceService != null
                    ? BlendState.New(graphicsDeviceService.GraphicsDevice, blendStateDescription)
                    : BlendState.NewFake(blendStateDescription);
            }
        }
    }
}