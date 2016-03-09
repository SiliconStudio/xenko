// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Images
{
    [DataContract("Bloom")]
    public class Bloom : ImageEffect
    {
        private GaussianBlur blur;

        private ImageMultiScaler multiScaler;

        private Afterimage afterimage;

        private Vector2 distortion;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bloom"/> class.
        /// </summary>
        public Bloom()
        {
            Radius = 10;
            Amount = 0.3f;
            DownScale = 1;
            SigmaRatio = 3.5f;
            Distortion = new Vector2(1);
            afterimage = new Afterimage { Enabled = false };
        }

        /// <summary>
        /// Radius of the bloom.
        /// </summary>
        /// <userdoc>The range of the bloom effect around bright regions. Note that high values can import performances</userdoc>
        [DataMember(10)]
        [DefaultValue(10)]
        [DataMemberRange(1.0, 100.0, 1.0, 10.0, 1)]
        public float Radius { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>The amount.</value>
        /// <userdoc>The strength of the bloom effect.</userdoc>
        [DataMember(20)]
        [DefaultValue(0.3f)]
        public float Amount { get; set; }

        /// <summary>
        /// Gets or sets the sigma ratio.
        /// </summary>
        /// <value>The ratio</value>
        /// <userdoc>The standard deviation used for the blur of the effect. This impact the fall-off of the bloom.</userdoc>
        [DataMember(30)]
        [DefaultValue(3.5f)]
        public float SigmaRatio { get; set; }

        /// <summary>
        /// Vertical or horizontal distortion to apply.
        /// (1, 2) means the bloom will be stretched twice longer horizontally than vertically.
        /// </summary>
        /// <userdoc>Apply vertical of horizontal distortion on the effect</userdoc>
        [DataMember(40)]
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
        /// Gets the afterimage effect/>
        /// </summary>
        /// <userdoc>When enabled, it simulates some persistence effects of the light points (trails) on the next frames. </userdoc>
        [DataMember(50)]
        public Afterimage Afterimage
        {
            get
            {
                return afterimage;
            }
        }

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

        protected override void InitializeCore()
        {
            base.InitializeCore();

            multiScaler = ToLoadAndUnload(new ImageMultiScaler());
            blur = ToLoadAndUnload(new GaussianBlur());
            afterimage = ToLoadAndUnload(afterimage);
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var input = GetInput(0);
            var output = GetOutput(0) ?? input;

            if (input == null)
            {
                return;
            }

            // If afterimage is active, add some persistence to the brightness
            if (afterimage.Enabled)
            {
                var persistenceBrightness = NewScopedRenderTarget2D(input.Description);
                afterimage.SetInput(0, input);
                afterimage.SetOutput(persistenceBrightness);
                ((RendererBase)afterimage).Draw(context);
                input = persistenceBrightness;
            }

            // A distortion can be applied to the bloom effect to simulate anamorphic lenses
            if (Distortion.X > 1f || Distortion.Y > 1f)
            {
                int distortedWidth  = (int)Math.Max(1, input.Description.Width  / Distortion.X);
                int distortedHeight = (int)Math.Max(1, input.Description.Height / Distortion.Y);
                var anamorphicInput = NewScopedRenderTarget2D(distortedWidth, distortedHeight, input.Format);
                Scaler.SetInput(input);
                Scaler.SetOutput(anamorphicInput);
                Scaler.Draw(context, "Anamorphic distortion");
                input = anamorphicInput;
            }

            // ----------------------------------------
            // Downscale / 4
            // ----------------------------------------
            const int DownScaleBasis = 1;
            var nextSize = input.Size.Down2(DownScaleBasis);
            var inputTextureDown4 = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, input.Format);
            Scaler.SetInput(input);
            Scaler.SetOutput(inputTextureDown4);
            Scaler.Draw(context, "Down/4");

            var blurTexture = inputTextureDown4;

            // TODO: Support automatic additional downscales based on a quality parameter instead
            // Additional downscales 
            if (DownScale > 0)
            {
                nextSize = nextSize.Down2(DownScale);
                blurTexture = NewScopedRenderTarget2D(nextSize.Width, nextSize.Height, input.Format);

                multiScaler.SetInput(inputTextureDown4);
                multiScaler.SetOutput(blurTexture);
                ((RendererBase)multiScaler).Draw(context);
            }

            // Max blur size no more than 1/4 of input size
            var inputMaxBlurRadiusInPixels = 0.25 * Math.Max(input.Width, input.Height) * Math.Pow(2, -DownScaleBasis - DownScale);
            blur.Radius = Math.Max(1, (int)MathUtil.Lerp(1, inputMaxBlurRadiusInPixels, Math.Max(0, Radius / 100.0f)));
            blur.SigmaRatio = Math.Max(1.0f, SigmaRatio);
            blur.SetInput(blurTexture);
            blur.SetOutput(blurTexture);
            ((RendererBase)blur).Draw(context);

            // TODO: Support automatic additional downscales 
            if (DownScale > 0)
            {
                multiScaler.SetInput(blurTexture);
                multiScaler.SetOutput(inputTextureDown4);
                ((RendererBase)multiScaler).Draw(context);
            }

            // Copy the input texture to the output
            if (ShowOnlyMip || ShowOnlyBloom)
            {
                context.CommandList.Clear(output, Color.Black);
            }

            // Switch to additive
            Scaler.BlendState = BlendStates.Additive;

            Scaler.Color = new Color4(Amount);
            Scaler.SetInput(inputTextureDown4);
            Scaler.SetOutput(output);
            ((RendererBase)Scaler).Draw(context);
            Scaler.Reset();

            Scaler.BlendState = BlendStates.Default;
        }
    }
}