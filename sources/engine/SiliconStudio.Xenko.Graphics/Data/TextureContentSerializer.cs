// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Streaming;

namespace SiliconStudio.Xenko.Graphics.Data
{
    internal class TextureContentSerializer : ContentSerializerBase<Texture>
    {
        public delegate void DeserializeTextureDelegate(IServiceRegistry services, Texture obj, ref ImageDescription imageDescription, ref ContentStorageHeader storageHeader);

        public static DeserializeTextureDelegate DeserializeTexture;

        /// <inheritdoc/>
        public override void Serialize(ContentSerializerContext context, SerializationStream stream, Texture texture)
        {
            Serialize(context.Mode, stream, texture, context.AllowContentStreaming);
        }

        internal static void Serialize(ArchiveMode mode, SerializationStream stream, Texture texture, bool allowContentStreaming)
        {
            if (mode == ArchiveMode.Deserialize)
            {
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();
                var texturesStreamingProvider = services.GetServiceAs<ITexturesStreamingProvider>();

                var isStreamable = stream.ReadBoolean();
                if (!isStreamable)
                {
                    texturesStreamingProvider?.UnregisterTexture(texture);

                    // TODO: Error handling?
                    using (var textureData = Image.Load(stream.NativeStream))
                    {
                        if (texture.GraphicsDevice != null)
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
                                var textureDataReloaded = assetManager.Load<Image>(url);
                                ((Texture)graphicsResource).Recreate(textureDataReloaded.ToDataBox());
                                assetManager.Unload(textureDataReloaded);
                            };
                        }
                    }
                }
                else
                {
                    texture.AttachToGraphicsDevice(graphicsDeviceService.GraphicsDevice);
                    texture.Reload = null;

                    // Read image header
                    var imageDescription = new ImageDescription();
                    ImageHelper.ImageDescriptionSerializer.Serialize(ref imageDescription, ArchiveMode.Deserialize, stream);

                    // Read content storage header
                    ContentStorageHeader storageHeader;
                    ContentStorageHeader.Read(stream, out storageHeader);

                    // Check if streaming service is available
                    if (texturesStreamingProvider != null)
                    {
                        if (allowContentStreaming)
                        {
                            // Register texture for streaming
                            texturesStreamingProvider.RegisterTexture(texture, ref imageDescription, ref storageHeader);

                            // Note: here we don't load texture data and don't allocate GPU memory
                        }
                        else
                        {
                            // Request texture loading (should be fully loaded)
                            texturesStreamingProvider.FullyLoadTexture(texture, ref imageDescription, ref storageHeader);
                        }
                    }
                    else
                    {
                        // Deserialize whole texture without streaming feature
                        DeserializeTexture(services, texture, ref imageDescription, ref storageHeader);
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

    // Previously Textures were serializated to Image format.
    internal class DeprecatedTextureContentSerializer : ContentSerializerBase<Texture>
    {
        public override Type SerializationType => typeof(Image);

        /// <inheritdoc/>
        public override void Serialize(ContentSerializerContext context, SerializationStream stream, Texture texture)
        {
            if (context.Mode == ArchiveMode.Deserialize)
            {
                var services = stream.Context.Tags.Get(ServiceRegistry.ServiceRegistryKey);
                var graphicsDeviceService = services.GetSafeServiceAs<IGraphicsDeviceService>();
                var texturesStreamingProvider = services.GetService(typeof(ITexturesStreamingProvider)) as ITexturesStreamingProvider;

                texturesStreamingProvider?.UnregisterTexture(texture);

                // TODO: Error handling?
                using (var textureData = Image.Load(stream.NativeStream))
                {
                    if (texture.GraphicsDevice != null)
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

                textureData.Image.Save(stream.NativeStream, ImageFileType.Xenko);
            }
        }

        public override object Construct(ContentSerializerContext context)
        {
            return new Texture();
        }
    }
}
