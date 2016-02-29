// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics.Data
{
    /// <summary>
    /// Serializer for <see cref="Buffer"/>.
    /// </summary>
    public class BufferSerializer : DataSerializer<Buffer>
    {
        public override void PreSerialize(ref Buffer buffer, ArchiveMode mode, SerializationStream stream)
        {
            // Do not create object during preserialize (OK because not recursive)
        }

        public override void Serialize(ref Buffer buffer, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                var bufferData = stream.Read<BufferData>();

                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                if (services == null)
                {
                    buffer.SetSerializationData(bufferData);
                }
                else
                {
                    var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

                    buffer.AttachToGraphicsDevice(graphicsDeviceService.GraphicsDevice);
                    buffer.InitializeFrom(bufferData.Content, bufferData.StructureByteStride, bufferData.BufferFlags, PixelFormat.None, bufferData.Usage);

                    // Setup reload callback (reload from asset manager)
                    var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
                    if (contentSerializerContext != null)
                    {
                        var assetManager = contentSerializerContext.ContentManager;
                        var url = contentSerializerContext.Url;

                        buffer.Reload = (graphicsResource) =>
                        {
                            // TODO: Avoid loading/unloading the same data
                            var loadedBufferData = assetManager.Load<BufferData>(url);
                            ((Buffer)graphicsResource).Recreate(loadedBufferData.Content);
                            assetManager.Unload(loadedBufferData);
                        };
                    }
                }
            }
            else
            {
                var bufferData = buffer.GetSerializationData();
                if (bufferData == null)
                    throw new InvalidOperationException("Trying to serialize a Buffer without CPU info.");

                stream.Write(bufferData);
            }
        }
    }
}