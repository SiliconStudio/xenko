// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Behaviors
{
    public class ChangeCursorOnSliderThumbBehavior : DeferredBehaviorBase<Slider>
    {
        protected override void OnAttachedAndLoaded()
        {
            var thumb = AssociatedObject.FindVisualChildOfType<Thumb>();
            if (thumb != null)
                thumb.Cursor = Cursors.SizeWE;

            base.OnAttachedAndLoaded();
        }
    }
}
