// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
