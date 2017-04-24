// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
