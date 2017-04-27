// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(Image))]
    public class ImageExtension : MarkupExtension
    {
        private readonly ImageSource source;
        private readonly int width;
        private readonly int height;
        private readonly BitmapScalingMode scalingMode;

        public ImageExtension(ImageSource source)
        {
            this.source = source;
            width = -1;
            height = -1;
        }

        public ImageExtension(ImageSource source, int width, int height)
            : this(source, width, height, BitmapScalingMode.Unspecified)
        {
        }

        public ImageExtension(ImageSource source, int width, int height, BitmapScalingMode scalingMode)
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));
            this.source = source;
            this.width = width;
            this.height = height;
            this.scalingMode = scalingMode;
        }

        [NotNull]
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var image = new Image { Source = source };
            RenderOptions.SetBitmapScalingMode(image, scalingMode);
            if (width >= 0 && height >= 0)
            {
                image.Width = width;
                image.Height = height;
            }
            return image;
        }
    }
}
