// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    [DataContract("Bloom")]
    public class Bloom : ImageEffect
    {
        private GaussianBlur blur;

        private ColorCombiner blurCombine;
        private ImageMultiScaler multiScaler;
        private readonly List<Texture> resultList = new List<Texture>();

        private Vector2 distortion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bloom"/> class.
        /// </summary>
        public Bloom()
        {
            Radius = 3f / 1280f;
            Amount = 1.0f;
            DownScale = 3;
            Distortion = new Vector2(1);
            ModulateColor = new Vector3(1);
        }

        /// <summary>
        /// Radius of the bloom.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(3f / 1280f)]
        [DataMemberRange(1f / 1280f, 0.5f)]
        public float Radius { get; set; }

        /// <summary>
        /// Vertical or horizontal distortion to apply.
        /// (1, 2) means the bloom will be stretched twice longer horizontally than vertically.
        /// </summary>
        [DataMember(20)]
        public Vector2 Distortion
        {
            get
            {
                return distortion;
            }

            set
            {
                distortion = value;
                if (distortion.X < 1f) distortion.X = 1f;
                if (distortion.Y < 1f) distortion.Y = 1f;
            }
        }

        /// <summary>
        /// Modulate the bloom by a certain color.
        /// </summary>
        public Vector3 ModulateColor { get; set; }

        [DataMemberIgnore]
        public float Amount { get; set; }

        [DataMemberIgnore]
        public bool ShowOnlyBloom { get; set; }

        [DataMemberIgnore]
        public bool ShowOnlyMip { get; set; }

        [DataMemberIgnore]
        public int MipIndex { get; set; }

        [DataMemberIgnore]
        public int DownScale { get; set; }

        [DataMemberIgnore]
        public int UpperMip
        {
            get { return Math.Max(0, MaxMip - 1); }
        }

        private int MaxMip { get; set; }

        public override void Initialize(RenderContext context)
        {
            base.Initialize(context);

            blurCombine = ToLoadAndUnload(new ColorCombiner());
            multiScaler = ToLoadAndUnload(new ImageMultiScaler());
            blur = ToLoadAndUnload(new GaussianBlur());
        }

        protected override void DrawCore(RenderContext context)
        {
            var input = GetInput(0);
            var output = GetOutput(0) ?? input;

            if (input == null)
            {
                return;
            }

            // A distortion can be applied to the bloom effect to simulate anamorphic lenses
            if (Distortion.X > 1f || Distortion.Y > 1f)
            {
                int distortedWidth  = (int)Math.Max(1, input.Description.Width  / Distortion.X);
                int distortedHeight = (int)Math.Max(1, input.Description.Height / Distortion.Y);
                var anamorphicInput = NewScopedRenderTarget2D(distortedWidth, distortedHeight, input.Format);
                Scaler.SetInput(input);
                Scaler.SetOutput(anamorphicInput);
                Scaler.Draw(context, name: "Anamorphic distortion");
                input = anamorphicInput;
            }

            // ----------------------------------------
            // Downscale / 2
            // ----------------------------------------
            var nextSize = input.Size.Down2();
            var startRenderTarget = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, input.Format);
            Scaler.SetInput(input);
            Scaler.SetOutput(startRenderTarget);
            Scaler.Draw(context, name: "Down/2");

            // ----------------------------------------
            // Downscale / 4 up to Downscale / xx
            // ----------------------------------------
            var previousRenderTarget = startRenderTarget;
            // Create other rendertargets upto lastMinSize max
            resultList.Clear();
            var upscaleSize = nextSize; //nextSize.Down2();

            //var radius = (float)power;
            var maxInputSize = Math.Max(input.Size.Width, input.Size.Height);
            var maxLevel = (int)(Math.Max(1, Math.Floor(Math.Log(maxInputSize, 2.0))) - 2);

            for (int mip = 0; mip < DownScale; mip++)
            {
                nextSize = nextSize.Down2();
                var nextRenderTarget = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, input.Format);

                // Downscale
                Scaler.SetInput(previousRenderTarget);
                Scaler.SetOutput(nextRenderTarget);
                Scaler.Draw(context, name: "Down/2");

                // Blur it
                blur.Radius = Math.Max(1, (int)(Radius * input.Width));
                blur.SetInput(nextRenderTarget);
                blur.SetOutput(nextRenderTarget);
                blur.Draw(context);

                // TODO: Use the MultiScaler for this part instead of recoding it here
                // Only blur after 2nd downscale
                var renderTargetToCombine = nextRenderTarget;
                if (mip > 0)
                {
                    renderTargetToCombine = NewScopedRenderTarget2D(upscaleSize.Width, upscaleSize.Height, input.Format);
                    multiScaler.SetInput(nextRenderTarget);
                    multiScaler.SetOutput(renderTargetToCombine);
                    multiScaler.Draw(context);
                }
                resultList.Add(renderTargetToCombine);
                previousRenderTarget = nextRenderTarget;
            }

            MaxMip = DownScale - 1;

            // Copy the input texture to the output
            if (ShowOnlyMip || ShowOnlyBloom)
            {
                GraphicsDevice.Clear(output, Color.Black);
            }

            // Switch to additive
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Additive);
            if (resultList.Count == 1)
            {
                Scaler.SetInput(resultList[0]);
                Scaler.SetOutput(output);
                Scaler.Draw(context);
            }
            else if (resultList.Count > 1)
            {
                // Combine the blurred mips
                blurCombine.Reset();
                for (int i = 0; i < resultList.Count; i++)
                {
                    var result = resultList[i];
                    blurCombine.SetInput(i, result);
                    var exponent = (float)Math.Max(0, i) - 4.0f;
                    var level = !ShowOnlyMip || i == MipIndex ? (float)Math.Pow(2.0f, exponent) : 0.0f;
                    level *= Amount;
                    blurCombine.Factors[i] = level;
                    blurCombine.ModulateRGB[i] = ModulateColor;
                }

                blurCombine.SetOutput(output);
                blurCombine.Draw(context);
            }
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
        }
    }
}