// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics
{
    internal class EffectConverter : DataConverter<EffectBytecode, Effect>
    {
        public override void ConvertFromData(ConverterContext converterContext, EffectBytecode data, ref Effect obj)
        {
            var services = converterContext.Tags.Get(ServiceRegistry.ServiceRegistryKey);
            var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

            obj = new Effect(graphicsDeviceService.GraphicsDevice, data);
        }

        public override void ConvertToData(ConverterContext converterContext, ref EffectBytecode data, Effect obj)
        {
            throw new System.NotImplementedException();
        }
    }
}