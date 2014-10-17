// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SiliconStudio.Presentation.MarkupExtensions
{
    [MarkupExtensionReturnType(typeof(Image))]
    public class ImageExtension : MarkupExtension
    {
        private readonly BitmapSource source;
        private readonly int width;
        private readonly int height;

        private readonly BitmapScalingMode scalingMode;

        public ImageExtension(BitmapSource source)
        {
            this.source = source;
            width = -1;
            height = -1;
        }
        
        public ImageExtension(BitmapSource source, int width, int height)
            : this(source, width, height, BitmapScalingMode.Unspecified)
        {
        }

        public ImageExtension(BitmapSource source, int width, int height, BitmapScalingMode scalingMode)
        {
            if (width < 0) throw new ArgumentOutOfRangeException("width");
            if (height < 0) throw new ArgumentOutOfRangeException("height");
            this.source = source;
            this.width = width;
            this.height = height;
            this.scalingMode = scalingMode;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var image = new Image { Source = source };
            if (width >= 0 && height >= 0)
            {
                image.Width = width;
                image.Height = height;
            }
            return image;
        }
    }
}