// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows;
using System.Windows.Controls;
using SiliconStudio.Presentation.Commands;

namespace SiliconStudio.Presentation.Controls
{
    public class TagControl : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="CloseTagCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseTagCommandProperty =
            DependencyProperty.Register("CloseTagCommand", typeof(ICommandBase), typeof(TagControl));

        public ICommandBase CloseTagCommand
        {
            get { return (ICommandBase)GetValue(CloseTagCommandProperty); }
            set { SetValue(CloseTagCommandProperty, value); }
        }

        static TagControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TagControl), new FrameworkPropertyMetadata(typeof(TagControl)));
        }
    }
}
