// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
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
        /// List of cubemaps, their positions and influence range.
        /// </summary>
        private List<Tuple<TextureCube, Vector3, float>> textureCubes;

        /// <summary>
        /// List of points of interest and the maximum number of cubemap that can be blended.
        /// </summary>
        private List<Tuple<Vector3, int>> pointsOfInterest;

        /// <summary>
        /// The blend effects.
        /// </summary>
        private Dictionary<int, Effect> cubemapBlendEffects;

        /// <summary>
        /// The target cubemap
        /// </summary>
        private TextureCube targetCubemap;

        /// <summary>
        /// The post effect draw quad.
        /// </summary>
        private PostEffectQuad drawQuad;

        /// <summary>
        /// A flag to use multiple render target to blend the cubemaps in one call.
        /// </summary>
        private bool useMRT;

        #endregion

        #region temporary

        public TextureCube TargetTexture
        {
            get
            {
                return targetCubemap;
            }
        }

        public void UpdatePosition(Vector3 pos)
        {
            var old = pointsOfInterest[0];
            pointsOfInterest[0] = Tuple.Create(pos, old.Item2);
        }

        #endregion

        #region Constructor

        public CubeMapBlender(IServiceRegistry services)
            : base(services)
        {
            textureCubes = new List<Tuple<TextureCube, Vector3, float>>();
            pointsOfInterest = new List<Tuple<Vector3, int>>();
            cubemapBlendEffects = new Dictionary<int, Effect>();
            useMRT = false;

            // TODO: change size
            int cubemapSize = 512;
            targetCubemap = TextureCube.New(GraphicsDevice, cubemapSize, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds a cubemap at the current location.
        /// </summary>
        /// <param name="texture">The cubemap texture.</param>
        /// <param name="position">The position.</param>
        public void AddTextureCube(TextureCube texture, Vector3 position, float range)
        {
            textureCubes.Add(Tuple.Create(texture, position, range));
        }

        /// <summary>
        /// Adds a point of interest (ie. a place where cubemap blend will be computed)
        /// </summary>
        /// <param name="poi">The point of interest position.</param>
        /// <param name="maxCubemapBlend">The maximum number of cubemaps that can be blended.</param>
        public void AddPointOfInterest(Vector3 poi, int maxCubemapBlend)
        {
            pointsOfInterest.Add(Tuple.Create(poi, maxCubemapBlend));
        }

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
            var selectedCubemaps = new List<Tuple<TextureCube, float>>();
            var parameters = new ParameterCollection();

            foreach (var poi in pointsOfInterest)
            {
                var maxCubemapBlend = poi.Item2;
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
                //FindClosestCubemaps(poi.Item1, maxCubemapBlend, selectedCubemaps);
                FindMostInfluencialCubemaps(poi.Item1, maxCubemapBlend, selectedCubemaps);

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
                GraphicsDevice.Clear(targetCubemap.ToRenderTarget(ViewType.Full, 0, 0), Color.Black);

                // set states
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

                // render each face
                if (useMRT)
                {
                    GraphicsDevice.SetRenderTargets(
                        targetCubemap.ToRenderTarget(ViewType.Single, 0, 0),
                        targetCubemap.ToRenderTarget(ViewType.Single, 1, 0),
                        targetCubemap.ToRenderTarget(ViewType.Single, 2, 0),
                        targetCubemap.ToRenderTarget(ViewType.Single, 3, 0),
                        targetCubemap.ToRenderTarget(ViewType.Single, 4, 0),
                        targetCubemap.ToRenderTarget(ViewType.Single, 5, 0));
                    cubemapBlendEffect.Apply(parameters);
                    drawQuad.Draw();
                }
                else
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        // set the render targets
                        GraphicsDevice.SetRenderTarget(targetCubemap.ToRenderTarget(ViewType.Single, i, 0));
                        parameters.Set(CubemapBlenderKeys.ViewIndex, i);
                        cubemapBlendEffect.Apply(parameters);
                        drawQuad.Draw();
                    }
                }
            }
        }

        private void FindClosestCubemaps(Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            foreach (var tex in textureCubes)
            {
                var d = (tex.Item2 - position).LengthSquared();
                var influence = 1.0f / (d + 1);
                var insertIndex = 0;
                for (; insertIndex < maxCubemapBlend; ++insertIndex)
                {
                    if (insertIndex >= selectedCubemaps.Count || influence > selectedCubemaps[insertIndex].Item2)
                        break;
                }
                if (insertIndex < maxCubemapBlend)
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(tex.Item1, influence));
            }
        }

        private void FindMostInfluencialCubemaps(Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            foreach (var tex in textureCubes)
            {
                // compute influence
                // TODO: other profile than linear?
                var d = (tex.Item2 - position).Length();
                var influence = 1 - (d / tex.Item3);
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
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(tex.Item1, influence));
            }
        }

        // Sebastien Largarde's weights
        // http://seblagarde.wordpress.com/2012/09/29/image-based-lighting-approaches-and-parallax-corrected-cubemap/
        private void FindMostInfluencialCubemaps2(Vector3 position, int maxCubemapBlend, List<Tuple<TextureCube, float>> selectedCubemaps)
        {
            var inflSum = 0f;
            var invInflSum = 0f;
            var influences = new List<float>();

            foreach (var tex in textureCubes)
            {
                // TODO: add boxes and inner boundaries
                var d = (tex.Item2 - position).Length();
                inflSum += d;
                invInflSum += 1 - d;
                influences.Add(d);
            }

            var sumBlend = 0f;
            var n = textureCubes.Count;
            for (var i = 0; i < n; ++i)
            {
                var infl = influences[i];
                var blendFactor = (1 - (infl / inflSum)) / (n - 1);
                blendFactor *= (1 - infl) / invInflSum;
                sumBlend += blendFactor;
                influences[i] = blendFactor;
            }

            if (sumBlend == 0f)
                sumBlend = 1f;

            var invSumBlend = 1 / sumBlend;
            for (var i = 0; i < n; ++i)
            {
                var influence = influences[i] * invSumBlend;

                var insertIndex = 0;
                for (; insertIndex < maxCubemapBlend; ++insertIndex)
                {
                    if (insertIndex >= selectedCubemaps.Count || influence > selectedCubemaps[insertIndex].Item2)
                        break;
                }
                if (insertIndex < maxCubemapBlend)
                    selectedCubemaps.Insert(insertIndex, Tuple.Create(textureCubes[i].Item1, influence));
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