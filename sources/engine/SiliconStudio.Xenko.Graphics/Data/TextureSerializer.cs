// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics.Data
{
    /// <summary>
    /// Serializer for <see cref="Texture"/>.
    /// </summary>
    public class TextureSerializer : DataSerializer<Texture>
    {
        public override void PreSerialize(ref Texture texture, ArchiveMode mode, SerializationStream stream)
        {
            // Do not create object during preserialize (OK because not recursive)
        }

        public override void Serialize(ref Texture texture, ArchiveMode mode, SerializationStream stream)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();

                // TODO: Error handling?
                using (var textureData = Image.Load(stream.NativeStream))
                {
                    texture.AttachToGraphicsDevice(graphicsDeviceService.GraphicsDevice);
                    texture.InitializeFrom(textureData.Description, new TextureViewDescription(), textureData.ToDataBox());

                    // Setup reload callback (reload from asset manager)
                    var contentSerializerContext = stream.Context.Get(ContentSerializerContext.ContentSerializerContextProperty);
                    if (contentSerializerContext != null)
                    {
                        var assetManager = contentSerializerContext.ContentManager;
                        var url = contentSerializerContext.Url;

                        texture.Reload = (graphicsResource) =>
                        {
                            // TODO: Avoid loading/unloading the same data
                            var textureDataReloaded = assetManager.Load<Image>(url);
                            ((Texture)graphicsResource).Recreate(textureDataReloaded.ToDataBox());
                            assetManager.Unload(textureDataReloaded);
                        };
                    }
                }
            }
            else
            {
                var textureData = texture.GetSerializationData();
                if (textureData == null)
                    throw new InvalidOperationException("Trying to serialize a Texture without CPU info.");

                textureData.Save(stream.NativeStream, ImageFileType.Xenko);
            }
        }
    }
}