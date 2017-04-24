// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
