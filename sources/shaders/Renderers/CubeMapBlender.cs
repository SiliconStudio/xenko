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

        public static ParameterKey<ShaderMixinParameters[]> Cubemaps = ParameterKeys.New<ShaderMixinParameters[]>();
        public static ParameterKey<ParameterKey> CubemapKey = ParameterKeys.New<ParameterKey>();
        public static ParameterKey<int> CubemapCount = ParameterKeys.New<int>();

        #endregion

        #region Private members

        private List<Tuple<TextureCube, Vector3>> textureCubes;
        private List<Vector3> pointsOfInterest;
        private Effect cubemapBlendEffect;

        private TextureCube targetCubemap;

        private PostEffectQuad drawQuad;

        #endregion

        #region Constructor

        public CubeMapBlender(IServiceRegistry services, TextureCube texture0, TextureCube texture1)
            : base(services)
        {
            textureCubes = new List<Tuple<TextureCube, Vector3>>();
            pointsOfInterest = new List<Vector3>();

            int cubemapSize = 512;
            targetCubemap = TextureCube.New(GraphicsDevice, cubemapSize, PixelFormat.R8G8B8A8_UNorm, TextureFlags.RenderTarget | TextureFlags.ShaderResource);
        }

        #endregion

        #region Public methods

        public void AddTextureCube(TextureCube texture, Vector3 position)
        {
            textureCubes.Add(Tuple.Create(texture, position));
        }

        public void AddPointOfInterest(Vector3 poi)
        {
            pointsOfInterest.Add(poi);
        }

        public override void Load()
        {
            // TODO: generate many shaders with different parameters (cubemap count)
            var compilerParameter = new CompilerParameters();
            var compilerParameterChild = new ShaderMixinParameters[2];
            for (var i = 0; i < 2; ++i)
            {
                var param = new ShaderMixinParameters();
                param.Add(CubeMapBlender.CubemapKey, GetTextureCubeKey(i));
                compilerParameterChild[i] = param;
            }
            compilerParameter.Set(CubeMapBlender.Cubemaps, compilerParameterChild);
            compilerParameter.Set(CubeMapBlender.CubemapCount, 2);
            cubemapBlendEffect = EffectSystem.LoadEffect("cubemapBlendEffect", compilerParameter);
            drawQuad = new PostEffectQuad(GraphicsDevice, cubemapBlendEffect);

            Pass.StartPass += OnRender;
        }

        public override void Unload()
        {
            Pass.StartPass -= OnRender;
        }

        #endregion

        #region Protected methods

        protected void OnRender(RenderContext context)
        {
            var maxBlend = 2;
            maxBlend = maxBlend > textureCubes.Count ? textureCubes.Count : maxBlend;
            var closestTextures = new List<Tuple<TextureCube, Vector3, float>>();
            var parameters = new ParameterCollection();

            foreach (var poi in pointsOfInterest)
            {
                // take the k closest textures
                closestTextures.Clear();
                foreach (var tex in textureCubes)
                {
                    var d = (tex.Item2 - poi).LengthSquared();
                    var insertIndex = 0;
                    for (; insertIndex < maxBlend; ++insertIndex)
                    {
                        if (d < closestTextures[insertIndex].Item3)
                            break;
                    }
                    if (insertIndex < maxBlend)
                        closestTextures.Insert(insertIndex, Tuple.Create(tex.Item1, tex.Item2, d));
                }

                // compute blending indices & set parameters
                // TODO: change this
                var totalWeight = closestTextures.Aggregate(0.0f, (s, t) => s + t.Item3);
                var blendIndices = new float[maxBlend];
                parameters.Clear();
                for (var i = 0; i < maxBlend; ++i)
                {
                    blendIndices[i] = closestTextures[i].Item3 / totalWeight;
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
                    GraphicsDevice.SetRenderTarget(targetCubemap.ToRenderTarget(ViewType.Single, 0, 0));
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