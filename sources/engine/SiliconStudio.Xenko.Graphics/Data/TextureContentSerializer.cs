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
                var texturesStreamingProvider = services.GetSafeServiceAs<ITexturesStreamingProvider>();

                var startPosition = stream.NativeStream.Position;
                var magicCode = stream.NativeStream.ReadUInt32();
                if (magicCode == ImageHelper.MagicCode)
                {
                    stream.NativeStream.Position = startPosition;

                    texturesStreamingProvider.UnregisterTexture(texture);

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
                else if (magicCode == TextureSerializationData.MagicCode)
                {
                    var isStreamable = stream.ReadBoolean();

                    // Read image header
                    var imageDescription = new ImageDescription();
                    ImageHelper.ImageDescriptionSerializer.Serialize(ref imageDescription, ArchiveMode.Deserialize, stream);

                    // Read content storage header
                    var storageHeader = ContentStorageHeader.Read(stream);

                    if (isStreamable)
                    {
                        // Register texture for streaming
                        texturesStreamingProvider.RegisterTexture(texture, ref imageDescription, storageHeader);

                        // Note: we don't load texture data here and don't allocate GPU memory
                    }
                    else
                    {
                        // TODO: should we use the new format for non streamable textures?
                        throw new NotImplementedException();
                    }

                    texture.AttachToGraphicsDevice(graphicsDeviceService.GraphicsDevice);

                }
                else
                {
                    throw new NotSupportedException("Unknown texture format version.");
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
