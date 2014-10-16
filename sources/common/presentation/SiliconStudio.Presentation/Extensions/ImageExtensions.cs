// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SiliconStudio.Presentation.Extensions
{
    public static class ImageExtensions
    {
        public static void SetSource(this Image image, Uri uri)
        {
            if (image == null)
                throw new ArgumentNullException("image");
            if (uri == null)
                throw new ArgumentNullException("uri");

            image.Source = ImageSourceFromFile(uri);
        }

        public static void SetSource(this Image image, string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Invalid 'uri' argument.");

            SetSource(image, new Uri(uri));
        }

        public static ImageSource ImageSourceFromFile(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            var source = new BitmapImage();
            source.BeginInit();
            source.UriSource = uri;
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.EndInit();

            return source;
        }

        public static ImageSource ImageSourceFromFile(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                throw new ArgumentException("Invalid 'uri' argument.");

            return ImageSourceFromFile(new Uri(uri));
        }
    }
}
