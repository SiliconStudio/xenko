// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Images.Dither
{
    [DataContract("Dither")]
    public class Dither : ColorTransform
    {
        public Dither() : base("Dither")
        {
        }

        public override void UpdateParameters(ColorTransformContext context)
        {
            base.UpdateParameters(context);

            Parameters.Set(DitherKeys.Time, (float)(context.RenderContext.Time.Total.TotalSeconds));
        }
    }
}