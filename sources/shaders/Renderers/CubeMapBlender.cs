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

        #endregion

        #region Private members
        
        /// <summary>
        /// List of cubemaps and their positions.
        /// </summary>
        private List<Tuple<TextureCube, Vector3>> textureCubes;

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

        #endregion

        #region Constructor

        public CubeMapBlender(IServiceRegistry services)
            : base(services)
        {
            textureCubes = new List<Tuple<TextureCube, Vector3>>();
            pointsOfInterest = new List<Tuple<Vector3, int>>();
            cubemapBlendEffects = new Dictionary<int, Effect>();

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
        public void AddTextureCube(TextureCube texture, Vector3 position)
        {
            textureCubes.Add(Tuple.Create(texture, position));
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
            for (var maxBlendCount = 2; maxBlendCount < 4; ++maxBlendCount)
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
            var closestTextures = new List<Tuple<TextureCube, Vector3, float>>();
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

                // take the k closest textures
                closestTextures.Clear();
                foreach (var tex in textureCubes)
                {
                    var d = (tex.Item2 - poi.Item1).LengthSquared();
                    var insertIndex = 0;
                    for (; insertIndex < maxCubemapBlend; ++insertIndex)
                    {
                        if (insertIndex >= closestTextures.Count || d < closestTextures[insertIndex].Item3)
                            break;
                    }
                    if (insertIndex < maxCubemapBlend)
                        closestTextures.Insert(insertIndex, Tuple.Create(tex.Item1, tex.Item2, d));
                }

                // compute blending indices & set parameters
                // TODO: change weight computation
                var totalWeight = closestTextures.Aggregate(0.0f, (s, t) => s + 1.0f / (t.Item3 + 1));
                var blendIndices = new float[maxCubemapBlend];
                parameters.Clear();
                for (var i = 0; i < maxCubemapBlend; ++i)
                {
                    blendIndices[i] = (1.0f / (closestTextures[i].Item3 + 1)) / totalWeight;
                    parameters.Set(GetTextureCubeKey(i), closestTextures[i].Item1);
                }
                parameters.Set(CubemapBlenderKeys.BlendIndices, blendIndices);

                // clear target
                GraphicsDevice.Clear(targetCubemap.ToRenderTarget(ViewType.Full, 0, 0), Color.Black);

                // set states
                GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);

                // render each face
                for (int i = 0; i < 6; ++i)
                {
                    // set the render targets
                    GraphicsDevice.SetRenderTarget(targetCubemap.ToRenderTarget(ViewType.Single, i, 0));
                    parameters.Set(CubemapFaceDisplayKeys.ViewIndex, i);
                    cubemapBlendEffect.Apply(parameters);
                    drawQuad.Draw();
                }
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