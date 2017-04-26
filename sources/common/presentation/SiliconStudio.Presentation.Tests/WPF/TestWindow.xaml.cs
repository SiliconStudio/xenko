// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.Presentation.Tests.WPF
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow
    {
        public TestWindow(string title)
            : this()
        {
            Title = title;
        }

        public TestWindow()
        {
            InitializeComponent();
        }

        public event EventHandler<EventArgs> Shown;

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            Shown?.Invoke(this, EventArgs.Empty);
        }
    }
}
