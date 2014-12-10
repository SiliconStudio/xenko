// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Materials.Nodes;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialParametersCreator : MaterialBaseVisitor
    {
        #region

        private string materialUrl;

        #endregion

        #region Public properties

        public ParameterCollectionData Parameters { get; private set; }

        #endregion

        #region Public methods

        public MaterialParametersCreator(MaterialDescription mat, string assetUrl) : base(mat)
        {
            Parameters = new ParameterCollectionData();
            materialUrl = assetUrl;
        }

        /// <summary>
        /// Compute the parameters and store them in the material.
        /// </summary>
        /// <param name="log">The logger.</param>
        /// <returns>A boolean stating that the parameters were incorrectly created.</returns>
        public bool CreateParameterCollectionData(Logger log = null)
        {
            Parameters.Clear();

            var hasErrors = false;

            var materialShaderCreator = new MaterialTreeShaderCreator(Material);
            var shaders = materialShaderCreator.GenerateModelShaders();
            
            if (log != null)
                (materialShaderCreator.Logger).CopyTo(log);

            foreach (var keyValue in Material.Parameters)
            {
                // NOTE: cheap way to activate alpha blending
                Parameters.Set(keyValue.Key, keyValue.Value);
                if (keyValue.Key == MaterialParameters.UseTransparent && Material.GetParameter(MaterialParameters.UseTransparent))
                {
                    // using non premultiply alpha blending
                    var blendStateDescr = new BlendStateDescription(Blend.SourceAlpha, Blend.InverseSourceAlpha);
                    var blendState = new FakeBlendState(blendStateDescr);
                    Parameters.Set(SiliconStudio.Paradox.Graphics.Effect.BlendStateKey, ContentReference.Create((BlendState)blendState));

                    // disable face culling
                    // TODO: make this programmable
                    var rasterizerStateDescr = new RasterizerStateDescription(CullMode.None);
                    var rasterizerState = new FakeRasterizerState(rasterizerStateDescr);
                    Parameters.Set(SiliconStudio.Paradox.Graphics.Effect.RasterizerStateKey, ContentReference.Create((RasterizerState)rasterizerState));

                    // disable depth write
                    var depthStencilStateDescr = new DepthStencilStateDescription(true, false);
                    var depthStencilState = new FakeDepthStencilState(depthStencilStateDescr);
                    Parameters.Set(SiliconStudio.Paradox.Graphics.Effect.DepthStencilStateKey, ContentReference.Create((DepthStencilState)depthStencilState));
                }
                else if (keyValue.Key == MaterialParameters.UseTransparentMask && Material.GetParameter(MaterialParameters.UseTransparentMask))
                {
                    // disable face culling
                    // TODO: make this programmable
                    var rasterizerStateDescr = new RasterizerStateDescription(CullMode.None);
                    var rasterizerState = new FakeRasterizerState(rasterizerStateDescr);
                    Parameters.Set(SiliconStudio.Paradox.Graphics.Effect.RasterizerStateKey, ContentReference.Create((RasterizerState)rasterizerState));

                    // enable depth write
                    var depthStencilStateDescr = new DepthStencilStateDescription(true, true);
                    var depthStencilState = new FakeDepthStencilState(depthStencilStateDescr);
                    Parameters.Set(SiliconStudio.Paradox.Graphics.Effect.DepthStencilStateKey, ContentReference.Create((DepthStencilState)depthStencilState));
                }
            }

            var textureVisitor = new MaterialTextureVisitor(Material);
            var allTextures = textureVisitor.GetAllModelTextureValuesWithGenerics();
            foreach (var texture in allTextures)
            {
                if (texture.TextureReference == null || (texture.TextureReference.Id == Guid.Empty && String.IsNullOrEmpty(texture.TextureReference.Location)))
                {
                    if (log != null)
                        log.Error("[Material] Material {0} is missing a texture", materialUrl);
                    hasErrors = true;
                }
                else
                {
                    Parameters.Set(texture.UsedParameterKey, new ContentReference<Graphics.Texture>(texture.TextureReference.Id, texture.TextureReference.Location));
                    AddSampler(texture.Sampler);
                }
            }

            var allSamplers = textureVisitor.GetAllSamplerValues();
            foreach (var sampler in allSamplers)
                AddSampler(sampler);

            var parameterVisitor = new MaterialParametersVisitor(Material);
            var parameters = parameterVisitor.GetParameters();
            foreach (var keyValue in parameters)
            {
                // The code is separated from the previous code since the key is not generated the same way.
                if (keyValue.Value is MaterialTextureNode)
                {
                    var textureNode = (MaterialTextureNode)keyValue.Value;
                    if (textureNode != null)
                    {
                        if (textureNode.TextureReference == null || textureNode.TextureReference.Id == Guid.Empty || String.IsNullOrEmpty(textureNode.TextureReference.Location))
                        {
                            if (log != null)
                                log.Error("[Material] Material {0} is missing a texture", materialUrl);
                            hasErrors = true;
                        }
                        else
                            Parameters.Set(keyValue.Key, new ContentReference<Graphics.Texture>(textureNode.TextureReference.Id, textureNode.TextureReference.Location));
                    }
                }
                else if (keyValue.Value is NodeParameterSampler)
                {
                    var sampler = (NodeParameterSampler)keyValue.Value;
                    if (sampler.SamplerParameterKey == null && keyValue.Key is ParameterKey<SamplerState>)
                        sampler.SamplerParameterKey = (ParameterKey<SamplerState>)keyValue.Key;
                    AddSampler(sampler);
                }
                else
                    Parameters.Set(keyValue.Key, keyValue.Value);
            }

            // NOTE: this can set the shader uniforms and potentially override what was in Material.SharedParameters
            foreach (var keyValue in shaders)
            {
                if (log != null && (keyValue.Key == MaterialParameters.BumpMap || keyValue.Key == MaterialParameters.EmissiveMap || keyValue.Key == MaterialParameters.ReflectionMap))
                    log.Warning("[Material] Material {0} contains the key {1} which is not yet handled by the engine.", materialUrl, keyValue.Key);
                
                Parameters.Set(keyValue.Key, keyValue.Value);
            }

            return hasErrors;
        }

        #endregion

        #region Private methods

        private void AddSampler(NodeParameterSampler sampler)
        {
            if (sampler != null && sampler.SamplerParameterKey != null)
            {
                var samplerStateDescr = new SamplerStateDescription(sampler.Filtering, sampler.AddressModeU)
                    {
                        AddressV = sampler.AddressModeV,
                        AddressW = TextureAddressMode.Wrap
                    };
                var samplerState = new FakeSamplerState(samplerStateDescr);
                Parameters.Set(sampler.SamplerParameterKey, ContentReference.Create((SamplerState)samplerState));
            }
        }

        #endregion
    }
}
