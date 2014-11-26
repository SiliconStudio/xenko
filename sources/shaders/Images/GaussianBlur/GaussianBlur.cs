// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class GaussianBlur : ImageEffectBase
    {
        internal static readonly ParameterKey<int> RadiusKey = ParameterKeys.New<int>();
        internal static readonly ParameterKey<bool> VerticalBlurKey  = ParameterKeys.New<bool>();
        internal static readonly ParameterKey<bool> UseSigma3Key = ParameterKeys.New<bool>();

        private readonly ImageEffect blurH;
        private readonly ImageEffect blurV;

        public GaussianBlur(ImageEffectContext context)
            : base(context)
        {
            blurH = new ImageEffect(context, "GaussianBlurEffect").DisposeBy(this);
            blurH.Parameters.Set(VerticalBlurKey, false);

            blurV = new ImageEffect(context, "GaussianBlurEffect").DisposeBy(this);
            blurH.Parameters.Set(VerticalBlurKey, true);

            Radius = 4;
            UseSigma3 = false;
        }

        public int Radius { get; set; }

        public bool UseSigma3 { get; set; }

        protected override void DrawCore()
        {
            var inputTexture = GetSafeInput(0);
            var outputHorizontal = Context.GetTemporaryRenderTarget2D(inputTexture.Description);

            // Horizontal pass
            blurH.SetInput(inputTexture);
            blurH.SetOutput(outputHorizontal);
            blurH.Parameters.Set(RadiusKey, Radius);
            blurH.Parameters.Set(UseSigma3Key, UseSigma3);

            // TODO: this is alocating a string, we should try to not allocate here.
            var size = Radius * 2 + 1;
            blurH.Draw(string.Format("GaussianBlurH{0}x{0}", size));

            // Vertical pass
            blurV.SetInput(outputHorizontal);
            blurV.SetOutput(GetSafeOutput(0));
            blurV.Parameters.Set(RadiusKey, Radius);
            blurV.Parameters.Set(UseSigma3Key, UseSigma3);
            blurV.Draw(string.Format("GaussianBlurV{0}x{0}", size));

            Context.ReleaseTemporaryTexture(outputHorizontal);            
        }
    }
}