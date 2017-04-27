// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;

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

                // Read header
                var startPosition = stream.NativeStream.Position;
                var version = stream.NativeStream.ReadUInt32();
                switch (version)
                {
                    case 1481919316:
                    {
                        // Note: 1st version was using raw Image without any information about texture streaming options, etc.
                        stream.NativeStream.Position = startPosition;
                        break;
                    }

                    case 2:
                    {
                        var isStreamable = stream.ReadBoolean();
                        
                        if (isStreamable)
                        {
                            // Read image header
                            var imageDescription = new ImageDescription();
                            ImageHelper.ImageDescriptionSerializer.Serialize(ref imageDescription, ArchiveMode.Deserialize, stream);
                            
                            // Read content storage header
                            var storageHeader = ContentStorageHeader.Read(stream);
                            
                            // Register texture for streaming
                            services.GetSafeServiceAs<ITexturesStreamingProvider>().RegisterTexture(texture, storageHeader);
                        }
                        break;
                    }

                    default:
                        throw new NotSupportedException("Unknown texture format version.");
                }
                
                // TODO: Error handling?
                using (var textureData = Image.Load(stream.NativeStream))
                {
                    if(texture.GraphicsDevice != null)
                        texture.OnDestroyed(); //Allows fast reloading todo review maybe?

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

                textureData.Write(stream);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return new Texture();
        }
    }
}
