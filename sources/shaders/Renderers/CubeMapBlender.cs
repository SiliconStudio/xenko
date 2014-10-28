// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules.Processors;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    public class CubeMapBlender : Renderer
    {
        #region Static members

        /// <summary>
        /// The key to set each cubemap parameter.
        /// </summary>
        public static ParameterKey<ShaderMixinParameters[]> Cubemaps = ParameterKeys.New<ShaderMixinParameters[]>();

        /// <summary>
        /// The key of the cubemap.
        /// </summary>
        public static ParameterKey<ParameterKey> CubemapKey = ParameterKeys.New<ParameterKey>();

        /// <summary>
        /// The number of cubemap in the shader.
        /// </summary>
        public static ParameterKey<int> CubemapCount = ParameterKeys.New<int>();

        /// <summary>
        /// Flag to enable multiple render target.
        /// </summary>
        public static ParameterKey<bool> UseMultipleRenderTargets = ParameterKeys.New<bool>();

        #endregion

        #region Private members
        
        /// <summary>
        /// The blend effects.
        /// </summary>
        private Dictionary<int, Effect> cubemapBlendEffects;

        /// <summary>
        /// The post effect draw quad.
        /// </summary>
        private PostEffectQuad drawQuad;

        /// <summary>
        /// A flag to use multiple render target to blend the cubemaps in one call.
        /// </summary>
        private bool useMRT;

        #endregion

        #region Constructor

        public CubeMapBlender(IServiceRegistry services)
            : base(services)
        {
            cubemapBlendEffects = new Dictionary<int, Effect>();
            useMRT = false;
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Load()
        {
            useMRT = GraphicsDevice.Features.Profile >= GraphicsProfile.Level_10_0;

            for (var maxBlendCount = 2; maxBlendCount < 5; ++maxBlendCount)
            {
                var compilerParameter = new CompilerParameters();
                var compilerParameterChild = new ShaderMixinParameters[maxBlendCount];
                for (var i = 0; i < maxBlendCount; ++i)
                {
                    var param = new ShaderMixinParameters();
                    param.Add(CubeMapBlender.CubemapKey, GetTextureCubeKey(i));
                    compilerParameterChild[i] = param;
                }
                compilerParameter.Set(CubeMapBlender.Cubemaps, compilerParameterChild);
                compilerParameter.Set(CubeMapBlender.CubemapCount, maxBlendCount);
                compilerParameter.Set(CubeMapBlender.UseMultipleRenderTargets, useMRT);
                cubemapBlendEffects.Add(maxBlendCount, EffectSystem.LoadEffect("CubemapBlendEffect", compilerParameter));
            }
            drawQuad = new PostEffectQuad(GraphicsDevice, cubemapBlendEffects[2]);

            Pass.StartPass += OnRender;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            Pass.StartPass -= OnRender;
        }

        #endregion

        #region Protected methods

        protected void OnRender(RenderContext context)
        {
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var cubemapBlendProcessor = entitySystem.GetProcessor<CubemapBlendProcessor>();
            if (cubemapBlendProcessor == null)
                return;

            var cubemapSourceProcessor = entitySystem.GetProcessor<CubemapSourceProcessor>();
            if (cubemapSourceProcessor == null)
                return;

            var textureCubes = cubemapSourceProcessor.Cubemaps;

            var selectedCubemaps = new List<Tuple<TextureCube, float>>();
            var parameters = new ParameterCollection();

            foreach (var cubemap in cubemapBlendProcessor.Cubemaps)
            {
                if (cubemap.Value.Texture == null)
                    continue;

                var maxCubemapBlend = cubemap.Value.MaxBlendCount;
                var position = cubemap.Key.Transformation.Translation;

                maxCubemapBlend = maxCubemapBlend > textureCubes.Count ? textureCubes.Count : maxCubemapBlend;
                
                // TODO: better use a list?
                Effect cubemapBlendEffect = null;
                while (maxCubemapBlend > 1)
                {
                    if (cubemapBlendEffects.TryGetValue(maxCubemapBlend, out cubemapBlendEffect) && cubemapBlendEffect != null)
                        break;
                    --maxCubemapBlend;
                }

                if (cubemapBlendEffect == null)
                    continue;

                // TODO: take the k most important cubemaps
                selectedCubemaps.Clear();
                //FindClosestCubemaps(textureCubes, position, maxCubemapBlend, selectedCubemaps);
                FindMostInfluencialCubemaps(textureCubes, position, maxCubemapBlend, selectedCubemaps);

                // compute blending indices & set parameters
                // TODO: change weight computation
                maxCubemapBlend = maxCubemapBlend > selectedCubemaps.Count ? selectedCubemaps.Count : maxCubemapBlend;
                var totalWeight = 0f;
                for (var i = 0; i < maxCubemapBlend; ++i)
                    totalWeight += selectedCubemaps[i].Item2;
                var blendIndices = new float[maxCubemapBlend];
                parameters.Clear();
                for (var i = 0; i < maxCubemapBlend; ++i)
                {
                    blendIndices[i] = selectedCubemaps[i].Item2 / totalWeight;
                    parameters.Set(GetTextureCubeKey(i), selectedCubemaps[i].Item1);
                }
                parameters.Set(CubemapBlenderBaseKeys.BlendIndices, blendIndices);

                // clear target
                // TODO: custom clear color?
                GraphicsDevice.Clear(cubemap.Value.Texture.ToRenderTarget(ViewType.Full, 0, 0), Color.Black);

                // set states
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

                // render each face
                if (useMRT)
                {
                    GraphicsDevice.SetRenderTargets(
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 0, 0),
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 1, 0),
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 2, 0),
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 3, 0),
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 4, 0),
                        cubemap.Value.Texture.ToRenderTarget(ViewType.Single, 5, 0));
                    cubemapBlendEffect.Apply(parameters);
                    drawQuad.Draw();
                }
                else
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        // set the render targets
                        GraphicsDevice.SetRenderTarget(cubemap.Value.Texture.ToRenderTarget(ViewType.Single, i, 0));
                        parameters.Set(CubemapBlenderKeys.ViewIndex, i);
                        cubemapBlendEffect.Apply(parameters);
                        drawQuad.Draw();
                    }
                }
            }
        }

        private void FindClosestCubemaps(Dictionary<Entity, CubemapSourceComponent> textureCubes, Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            foreach (var tex in textureCubes)
            {
                var d = (tex.Key.Transformation.Translation - position).LengthSquared();
                var influence = 1.0f / (d + 1);
                var insertIndex = 0;
                for (; insertIndex < maxCubemapBlend; ++insertIndex)
                {
                    if (insertIndex >= selectedCubemaps.Count || influence > selectedCubemaps[insertIndex].Item2)
                        break;
                }
                if (insertIndex < maxCubemapBlend)
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(tex.Value.Texture, influence));
            }
        }

        private void FindMostInfluencialCubemaps(Dictionary<Entity, CubemapSourceComponent> textureCubes, Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            foreach (var tex in textureCubes)
            {
                // compute influence
                // TODO: other profile than linear?
                var d = (tex.Key.Transformation.Translation - position).Length();
                var influence = 1 - (d / tex.Value.InfluenceRadius);
                if (influence > 1)
                    influence = 1;
                else if (influence <= 0)
                    continue;

                var insertIndex = 0;
                for (; insertIndex < maxCubemapBlend; ++insertIndex)
                {
                    if (insertIndex >= selectedCubemaps.Count || influence > selectedCubemaps[insertIndex].Item2)
                        break;
                }
                if (insertIndex < maxCubemapBlend)
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(tex.Value.Texture, influence));
            }
        }

        // Sebastien Largarde's weights
        // http://seblagarde.wordpress.com/2012/09/29/image-based-lighting-approaches-and-parallax-corrected-cubemap/
        private void FindMostInfluencialCubemaps2(Dictionary<Entity, CubemapSourceComponent> textureCubes, Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            var inflSum = 0f;
            var invInflSum = 0f;
            var influences = new Dictionary<Entity, float>();
            var blendValues = new Dictionary<Entity, float>();

            foreach (var tex in textureCubes)
            {
                // TODO: add boxes and inner boundaries
                var d = (tex.Key.Transformation.Translation - position).Length();
                inflSum += d;
                invInflSum += 1 - d;
                influences.Add(tex.Key, d);
            }

            var sumBlend = 0f;
            var n = textureCubes.Count;
            foreach (var influence in influences)
            {
                var infl = influence.Value;
                var blendFactor = (1 - (infl / inflSum)) / (n - 1);
                blendFactor *= (1 - infl) / invInflSum;
                sumBlend += blendFactor;
                blendValues.Add(influence.Key, blendFactor);
            }

            if (sumBlend == 0f)
                sumBlend = 1f;

            var invSumBlend = 1 / sumBlend;
            foreach (var tex in textureCubes)
            {
                var influence = blendValues[tex.Key] * invSumBlend;

                var insertIndex = 0;
                for (; insertIndex < maxCubemapBlend; ++insertIndex)
                {
                    if (insertIndex >= selectedCubemaps.Count || influence > selectedCubemaps[insertIndex].Item2)
                        break;
                }
                if (insertIndex < maxCubemapBlend)
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(tex.Value.Texture, influence));
            }
        }

        #endregion

        #region Helpers

        private ParameterKey<Texture> GetTextureCubeKey(int i)
        {
            switch (i)
            {
                case 0:
                    return TexturingKeys.TextureCube0;
                case 1:
                    return TexturingKeys.TextureCube1;
                case 2:
                    return TexturingKeys.TextureCube2;
                case 3:
                    return TexturingKeys.TextureCube3;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        #endregion
    }
}