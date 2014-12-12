// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class Bloom : ImageEffect
    {
        private readonly GaussianBlur blur;

        private readonly ColorCombiner blurCombine;
        private readonly List<Texture> resultList = new List<Texture>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Bloom"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public Bloom(ImageEffectContext context)
            : base(context)
        {
            blurCombine = new ColorCombiner(Context);
            blur = new GaussianBlur(context);

            Radius = 3;
            Amount = 1.0f;
            DownScale = 5;
        }

        public int Radius { get; set; }

        public float Amount { get; set; }

        public bool ShowOnlyBloom { get; set; }

        public bool ShowOnlyMip { get; set; }

        public int MipIndex { get; set; }

        public int DownScale { get; set; }

        public int UpperMip
        {
            get { return Math.Max(0, MaxMip - 1); }
        }

        private int MaxMip { get; set; }

        protected override void DrawCore()
        {
            var inputTexture = GetInput(0);
            var output = GetOutput(0) ?? inputTexture;

            if (inputTexture == null)
            {
                return;
            }

            // ----------------------------------------
            // Downscale / 2
            // ----------------------------------------
            var nextSize = inputTexture.Size.Down2();
            var startRenderTarget = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, inputTexture.ViewFormat);
            Scaler.SetInput(inputTexture);
            Scaler.SetOutput(startRenderTarget);
            Scaler.Draw("Down/2");

            // ----------------------------------------
            // Downscale / 4 up to Downscale / xx
            // ----------------------------------------
            var previousRenderTarget = startRenderTarget;
            // Create other rendertargets upto lastMinSize max
            resultList.Clear();
            var tempList = new List<Texture>();
            var sizeDown4 = nextSize.Down2();

            //var radius = (float)power;
            var maxInputSize = Math.Max(inputTexture.Size.Width, inputTexture.Size.Height);
            var maxLevel = (int)(Math.Max(1, Math.Floor(Math.Log(maxInputSize, 2.0))) - 2);

            for (int mip = 0; mip < DownScale; mip++)
            {
                nextSize = nextSize.Down2();
                var nextRenderTarget = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, inputTexture.ViewFormat);

                // Downscale
                Scaler.SetInput(previousRenderTarget);
                Scaler.SetOutput(nextRenderTarget);
                Scaler.Draw("Down/2");

                // Blur it
                blur.Radius = Radius;
                blur.SetInput(nextRenderTarget);
                blur.SetOutput(nextRenderTarget);
                blur.Draw();

                // TODO: Use the MultiScaler for this part instead of recoding it here
                GraphicsDevice.BeginProfile(Color.Green, "UpGroup");
                // Only blur after 2nd downscale
                if (resultList.Count > 0)
                {
                    // Upscale back to /4
                    var sourceUpScale = nextRenderTarget;
                    for (int i = tempList.Count - 1; i >= 0; i--)
                    {
                        Scaler.SetInput(sourceUpScale);
                        Scaler.SetOutput(tempList[i]);
                        Scaler.Draw("Upx2");
                        sourceUpScale = tempList[i];
                    }
                    tempList.Add(nextRenderTarget);

                    var result = NewScopedRenderTarget2D(sizeDown4.Width, sizeDown4.Height, inputTexture.ViewFormat);
                    Scaler.SetInput(tempList[0]);
                    Scaler.SetOutput(result);
                    Scaler.Draw("Upx2");

                    resultList.Add(result);
                }
                else
                {
                    resultList.Add(nextRenderTarget);
                }
                GraphicsDevice.EndProfile();

                previousRenderTarget = nextRenderTarget;
            }

            MaxMip = resultList.Count;

            // Copy the input texture to the output
            if (ShowOnlyMip || ShowOnlyBloom)
            {
                GraphicsDevice.Clear(output, Color.Black);
            }
            else
            {
                if (inputTexture != output)
                {
                    Scaler.SetInput(inputTexture);
                    Scaler.SetOutput(output);
                    Scaler.Draw();
                }
            }

            // Switch to additive
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Additive);
            if (resultList.Count == 1)
            {
                Scaler.SetInput(resultList[0]);
                Scaler.SetOutput(output);
                Scaler.Draw();
            }
            else if (resultList.Count > 1)
            {
                // Combine the blurred mips
                for (int i = 0; i < resultList.Count; i++)
                {
                    var result = resultList[i];
                    blurCombine.SetInput(i, result);
                    var exponent = (float)Math.Max(0, i) - 4.0f;
                    var level = !ShowOnlyMip || i == MipIndex ? (float)Math.Pow(2.0f, exponent) : 0.0f;
                    level *= Amount;
                    blurCombine.Factors[i] = level;
                }

                blurCombine.SetOutput(GetSafeOutput(0));
                blurCombine.Draw();
            }
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
        }
    }
}