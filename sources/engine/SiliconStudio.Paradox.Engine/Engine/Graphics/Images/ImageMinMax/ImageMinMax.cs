// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Images;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class ImageMinMax : ImageEffect
    {
        internal static ParameterKey<bool> IsFirstPassKey = ParameterKeys.New<bool>();

        private ImageEffectShader effect;

        private ImageReadback<Vector2> readback;

        public ImageMinMax()
        {
        }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            effect = ToLoadAndUnload(new ImageEffectShader("ImageMinMaxEffect"));
            readback = ToLoadAndUnload(new ImageReadback<Vector2>(Context));
        }

        public bool IsResultAvailable { get; private set; }

        public Vector2 Result { get; private set; }

        protected override void DrawCore(RenderContext context)
        {
            var input = GetSafeInput(0);

            // TODO: Check that input is power of two
            // input.Size.Width 

            Texture fromTexture = input;
            Texture downTexture = null;
            var nextSize = input.Size;
            bool isFirstPass = true;
            while (nextSize.Width > 3 && nextSize.Height > 3)
            {
                nextSize = nextSize.Down2();
                downTexture = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, PixelFormat.R32G32_Float, 1);

                effect.Parameters.Set(ImageMinMaxShaderKeys.TextureMap, fromTexture);
                effect.Parameters.Set(ImageMinMaxShaderKeys.TextureReduction, fromTexture);

                effect.SetOutput(downTexture);
                effect.Parameters.Set(IsFirstPassKey, isFirstPass);
                effect.Draw(context);

                fromTexture = downTexture;

                isFirstPass = false;
            }

            readback.SetInput(downTexture);
            readback.Draw();
            IsResultAvailable = readback.IsResultAvailable;
            if (IsResultAvailable)
            {
                float min = float.MaxValue;
                float max = -float.MaxValue;
                var results = readback.Result;
                foreach (var result in results)
                {
                    min = Math.Min(result.X, min);
                    if (result.Y != 1.0f)
                    {
                        max = Math.Max(result.Y, max);
                    }
                }

                Result = new Vector2(min, max);
            }
        }
    }
}