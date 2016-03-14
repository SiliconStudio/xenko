// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Graphics.Data
{
    internal class TextureContentSerializer : ContentSerializerBase<Texture>
    {
        public override Type SerializationType
        {
            get { return typeof(Image); }
        }

        public override void Serialize(ContentSerializerContext context, SerializationStream stream, Texture texture)
        {
            // TODO: This is the same as TextureConverter. Use DataContentSerializer for both Texture and Image?
            if (context.Mode == ArchiveMode.Deserialize)
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

        public override object Construct(ContentSerializerContext context)
        {
            return new Texture();
        }
    }
}
