// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Effects.Images
{
    public class GaussianBlur : ImageEffectBase
    {
        internal static readonly ParameterKey<int> RadiusKey;

        internal static readonly ParameterKey<bool> VerticalBlurKey;

        internal static readonly ParameterKey<float> SigmaRatioKey;

        static GaussianBlur()
        {
            var defaultRadius = Enumerable.Range(1, 20).Select(value => (object)value).ToArray();

            RadiusKey = ParameterKeys.New<int>(0, "GaussianBlur.RadiusKey", new ParameterKeyPermutationsMetadata(defaultRadius));
            VerticalBlurKey = ParameterKeys.New<bool>(false, "GaussianBlur.VerticalBlurKey", new ParameterKeyPermutationsMetadata(true, false));
            SigmaRatioKey = ParameterKeys.New<float>(2.0f, "GaussianBlur.SigmaRatioKey", new ParameterKeyPermutationsMetadata(2.0f, 3.0f));
        }

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
            SigmaRatio = 2.0f;
        }

        public int Radius { get; set; }

        public float SigmaRatio { get; set; }

        protected override void DrawCore()
        {
            var inputTexture = GetSafeInput(0);
            var outputHorizontal = Context.GetTemporaryRenderTarget2D(inputTexture.Description);

            // Horizontal pass
            blurH.SetInput(inputTexture);
            blurH.SetOutput(outputHorizontal);
            blurH.Parameters.Set(RadiusKey, Radius);
            blurH.Parameters.Set(SigmaRatioKey, SigmaRatio);

            var size = Radius * 2 + 1;
            blurH.Draw("GaussianBlurH{0}x{0}", size);

            // Vertical pass
            blurV.SetInput(outputHorizontal);
            blurV.SetOutput(GetSafeOutput(0));
            blurV.Parameters.Set(RadiusKey, Radius);
            blurV.Parameters.Set(SigmaRatioKey, SigmaRatio);
            blurV.Draw("GaussianBlurV{0}x{0}", size);

            Context.ReleaseTemporaryTexture(outputHorizontal);            
        }
    }
}