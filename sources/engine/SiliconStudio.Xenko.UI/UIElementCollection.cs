// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.UI
{
    /// <summary>
    /// A collection of UIElements.
    /// </summary>
    [DataContract(nameof(UIElementCollection))]
    public class UIElementCollection : TrackingCollection<UIElement>
    {
    }
}
