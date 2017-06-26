// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Rendering.Images
{
    /// <summary>
    /// A collection of <see cref="ColorTransformBase"/>
    /// </summary>
    [DataContract("ColorTransformCollection")]
    public class ColorTransformCollection : SafeList<ColorTransform>
    {
        public T Get<T>() where T : ColorTransform
        {
            foreach (var transform in this)
            {
                if (typeof(T) == transform.GetType())
                {
                    return (T)transform;
                }
            }
            return null;
        }

        public bool IsEnabled<T>() where T : ColorTransform
        {
            foreach (var transform in this)
            {
                if (typeof(T) == transform.GetType())
                {
                    return transform.Enabled;
                }
            }
            return false;
        }
    }
}
