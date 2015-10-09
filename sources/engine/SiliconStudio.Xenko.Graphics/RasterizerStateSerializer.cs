// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// Serializer for <see cref="RasterizerState"/>.
    /// </summary>
    [DataSerializerGlobal(typeof(RasterizerStateSerializer))]
    public class RasterizerStateSerializer : DataSerializer<RasterizerState>
    {
        public override void Serialize(ref RasterizerState rasterizerState, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Serialize)
            {
                var rasterizerStateDescription = rasterizerState.Description;
                stream.Serialize(ref rasterizerStateDescription, mode);
            }
            else
            {
                // If we have a graphics context, we will instantiate GPU state, otherwise a CPU one.
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services != null ? services.GetSafeServiceAs<IGraphicsDeviceService>() : null;

                var rasterizerStateDescription = RasterizerStateDescription.Default;
                stream.Serialize(ref rasterizerStateDescription, mode);
                rasterizerState = graphicsDeviceService != null
                    ? RasterizerState.New(graphicsDeviceService.GraphicsDevice, rasterizerStateDescription)
                    : RasterizerState.NewFake(rasterizerStateDescription);
            }
        }
    }
}