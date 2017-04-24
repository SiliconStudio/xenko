// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SiliconStudio.Xenko.Extensions;
using SiliconStudio.Xenko.Framework.MicroThreading;
using SiliconStudio.Xenko.DebugTools.ViewModels;

namespace SiliconStudio.Xenko.DebugTools
{
    /// <summary>
    /// Interaction logic for ScriptListControl.xaml
    /// </summary>
    public partial class ScriptListControl : UserControl
    {
        private EngineContext engineContext;

        public ScriptListControl()
        {
            InitializeComponent();
        }

        internal void Initialize(EngineContext engineContext)
        {
            this.engineContext = engineContext;

            this.DataContext = new RootViewModel(engineContext, processInfoRenderer);
            processInfoRendererScroller.ScrollToRightEnd();
        }
    }
}
