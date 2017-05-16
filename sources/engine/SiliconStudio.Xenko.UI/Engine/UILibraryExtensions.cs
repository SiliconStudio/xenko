// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Engine
{
    public static class UILibraryExtensions
    {
        /// <summary>
        /// Instantiates a copy of the element of the library identified by <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <param name="library">The library.</param>
        /// <param name="name">The name of the element in the library.</param>
        /// <returns></returns>
        public static TElement InstantiateElement<TElement>(this UILibrary library, string name)
            where TElement : UIElement
        {
            if (library == null) throw new ArgumentNullException(nameof(library));

            UIElement source;
            if (library.UIElements.TryGetValue(name, out source))
            {
                return UICloner.Clone(source) as TElement;
            }
            return null;
        }
    }
}
