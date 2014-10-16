// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Extensions
{
    public static class VisualExtensions
    {
        public static Visual FindAdornable(this Visual source)
        {
            return FindAdornableOfType<Visual>(source);
        }

        public static T FindAdornableOfType<T>(this Visual source) where T : Visual
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (AdornerLayer.GetAdornerLayer(source) != null && source is T)
                return (T)source;

            int childCount = VisualTreeHelper.GetChildrenCount(source);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(source, i) as T;
                if (child != null)
                {
                    var test = child.FindAdornableOfType<T>();
                    if (test != null)
                        return test;
                }
            }

            return null;
        }

        public static AdornerLayer FindAdornerLayer(this Visual source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var adornerLayer = AdornerLayer.GetAdornerLayer(source);
            if (adornerLayer != null)
                return adornerLayer;

            int childCount = VisualTreeHelper.GetChildrenCount(source);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(source, i) as Visual;
                if (child != null)
                {
                    var test = child.FindAdornerLayer();
                    if (test != null)
                        return test;
                }
            }

            return null;
        }

    }
}
