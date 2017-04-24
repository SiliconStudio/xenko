// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    public class SetFocusOnLoadBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            if (AssociatedObject.IsLoaded)
                AssociatedObject.Focus();
            else
                AssociatedObject.Loaded += OnHostLoaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnHostLoaded;
        }

        private void OnHostLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Focus();
        }
    }
}
