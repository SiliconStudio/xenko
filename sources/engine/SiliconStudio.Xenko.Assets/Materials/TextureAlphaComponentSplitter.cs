// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

using SiliconStudio.Assets;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Assets;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.Assets.Materials.Nodes;
using SiliconStudio.Xenko.Assets.Materials.Processor.Visitors;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Graphics.Data;
using SiliconStudio.TextureConverter;

namespace SiliconStudio.Xenko.Assets.Materials
{
    /// <summary>
    /// Utility class to split this material texture containing alpha into two texture materials: one containing the rgb component and one containing only the alpha component.
    /// More concretely, the class create the color channel and alpha channel textures from the original color/alpha texture 
    /// and replace the Material Texture Reference nodes containing an alpha component by a sub tree where:
    ///  - the parent is a binary operator <see cref="MaterialBinaryOperand.SubstituteAlpha"/>
    ///  - the left child is a Material Texture reference containing the color
    ///  - the right child is a Material Texture reference containing the alpha
    /// </summary>
    /// <remarks>Currently the output formats of the color and alpha textures are hard coded for android.</remarks>
    public class TextureAlphaComponentSplitter
    {
        public const string SplittedTextureNamePrefix = "__splitted_textures__";
        public const string SplittedColorTextureNameSuffix = "__Color";
        public const string SplittedAlphaTextureNameSuffix = "__Alpha";

        /// <summary>
        /// The session containing all the texture references.
        /// </summary>
        private readonly PackageSession assetSession;
        
        public TextureAlphaComponentSplitter(PackageSession assetSession)
        {
            this.assetSession = assetSession;
        }

        public MaterialDescription Run(MaterialDescription material, UDirectory materialPath, PixelFormat outputFormat = PixelFormat.ETC1)
        {
            if (material == null) throw new ArgumentNullException("material");

            var assetManager = new AssetManager();
            var modifiedMaterial = material.Clone();
            var textureVisitor = new MaterialTextureVisitor(modifiedMaterial);
            var nodeReplacer = new MaterialNodeReplacer(modifiedMaterial);
            var textureNodes = textureVisitor.GetAllModelTextureValues();

            foreach (var textureNode in textureNodes)
            {
                var itemAsset = assetSession.FindAsset(textureNode.TextureReference.Id);
                if(itemAsset == null)
                    throw new InvalidOperationException("The referenced texture is not included in the project session.");

                var textureAsset = (TextureAsset)itemAsset.Asset;
                if (textureAsset.Format != TextureFormat.Compressed || textureAsset.Alpha == AlphaFormat.None)
                    continue; // the texture has no alpha so there is no need to divide the texture into two sub-textures

                var originalLocation = textureNode.TextureReference.Location;

                throw new NotImplementedException("TODO: Need to reimplement this with removed data layer.");
                using (var image = assetManager.Load<Image>(originalLocation))
                {
                    CreateAndSaveSeparateTextures(image, originalLocation, textureAsset.GenerateMipmaps, outputFormat);
                    assetManager.Unload(image); // matching unload to the previous asset manager load call
                }

                // make new tree
                var colorNode = new MaterialTextureNode(GenerateColorTextureURL(originalLocation), textureNode.TexcoordIndex, Vector2.One, Vector2.Zero);
                var alphaNode = new MaterialTextureNode(GenerateAlphaTextureURL(originalLocation), textureNode.TexcoordIndex, Vector2.One, Vector2.Zero);
                var substituteAlphaNode = new MaterialShaderClassNode { MixinReference = new AssetReference<EffectShaderAsset>(Guid.Empty, "ComputeColorSubstituteAlphaWithColor") };
                substituteAlphaNode.CompositionNodes.Add("color1", colorNode);
                substituteAlphaNode.CompositionNodes.Add("color2", alphaNode);

                // set the parameters of the children so that they match the original texture
                var children = new[] { colorNode, alphaNode };
                foreach (var childTexture in children)
                {
                    childTexture.Sampler.AddressModeU = textureNode.Sampler.AddressModeU;
                    childTexture.Sampler.AddressModeV = textureNode.Sampler.AddressModeV;
                    childTexture.Sampler.Filtering = textureNode.Sampler.Filtering;
                    childTexture.Offset = textureNode.Offset;
                    childTexture.Sampler.SamplerParameterKey = textureNode.Sampler.SamplerParameterKey;
                    childTexture.Scale = textureNode.Scale;
                    childTexture.TexcoordIndex = textureNode.TexcoordIndex;
                }

                // copy the parameter key on the color and let the one of the alpha null so that it is set automatically to available value later
                colorNode.Key = textureNode.Key;
                alphaNode.Key = null;

                // update all the material references to the new node
                nodeReplacer.Replace(textureNode, substituteAlphaNode);
            }
            
            return modifiedMaterial;
        }

        public static string GenerateAlphaTextureURL(UFile originalLocation)
        {
            return GenerateSeparateTextureURL(originalLocation, SplittedAlphaTextureNameSuffix);
        }

        public static string GenerateColorTextureURL(UFile originalLocation)
        {
            return GenerateSeparateTextureURL(originalLocation, SplittedColorTextureNameSuffix);
        }

        private static string GenerateSeparateTextureURL(UFile originalLocation, string suffixName)
        {
            return originalLocation.GetDirectory() + "/" + SplittedTextureNamePrefix + originalLocation.GetFileName() + suffixName;
        }

        public static void CreateAndSaveSeparateTextures(Image image, string originalTextureURL, bool shouldGenerateMipMaps, PixelFormat outputFormat = PixelFormat.ETC1)
        {
            using (var texTool = new TextureTool())
            using (var texImage = texTool.Load(image))
            {
                CreateAndSaveSeparateTextures(texTool, texImage, originalTextureURL, shouldGenerateMipMaps, outputFormat);
            }
        }
        public static void CreateAndSaveSeparateTextures(TextureTool texTool, TexImage texImage, string originalTextureURL, bool shouldGenerateMipMaps, PixelFormat outputFormat = PixelFormat.ETC1)
        {
            var assetManager = new AssetManager();
            var alphaTextureURL = GenerateAlphaTextureURL(originalTextureURL);
            var colorTextureURL = GenerateColorTextureURL(originalTextureURL);

            // create a new image containing only the alpha component
            texTool.Decompress(texImage, texImage.Format.IsSRgb());
            using (var alphaImage = texTool.CreateImageFromAlphaComponent(texImage))
            {
                // generate the mip-maps for the alpha component if required
                if (shouldGenerateMipMaps)
                    texTool.GenerateMipMaps(alphaImage, Filter.MipMapGeneration.Box);

                // save the alpha component
                texTool.Compress(alphaImage, outputFormat);
                using (var outputImage = texTool.ConvertToXenkoImage(alphaImage))
                    assetManager.Save(alphaTextureURL, outputImage.ToSerializableVersion());
            }

            // save the color component
            texTool.Decompress(texImage, texImage.Format.IsSRgb());
            texTool.Compress(texImage, outputFormat);
            using (var outputImage = texTool.ConvertToXenkoImage(texImage))
                assetManager.Save(colorTextureURL, outputImage.ToSerializableVersion());
        }
    }
}
