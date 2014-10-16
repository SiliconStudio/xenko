// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Converters;

namespace SiliconStudio.Paradox.Graphics.Data
{
    public class BufferDataConverter : DataConverter<BufferData, Buffer>
    {
        public override void ConvertFromData(ConverterContext converterContext, BufferData data, ref Buffer buffer)
        {
            var services = converterContext.Tags.Get(ServiceRegistry.ServiceRegistryKey);
            var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

            buffer = Buffer.New(graphicsDeviceService.GraphicsDevice, data.Content, data.StructureByteStride, data.BufferFlags, PixelFormat.None, data.Usage);

            var assetManager = (AssetManager)services.GetServiceAs<IAssetManager>();

            // Setup reload callback (reload from asset manager)
            string url;
            if (assetManager.TryGetAssetUrl(data, out url))
            {
                buffer.Reload = (graphicsResource) =>
                {
                    // TODO: Avoid loading/unloading the same data
                    var loadedBufferData = assetManager.Load<BufferData>(url);
                    ((Buffer)graphicsResource).Recreate(loadedBufferData.Content);
                    assetManager.Unload(loadedBufferData);
                };
            }
        }

        public override void ConvertToData(ConverterContext converterContext, ref BufferData data, Buffer obj)
        {
            data = new BufferData { Content = obj.GetData<byte>(), BufferFlags = obj.Description.BufferFlags, StructureByteStride = obj.Description.StructureByteStride, Usage = obj.Description.Usage };
        }
    }
}