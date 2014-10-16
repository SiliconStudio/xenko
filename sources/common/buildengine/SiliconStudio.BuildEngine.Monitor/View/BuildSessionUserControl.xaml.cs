// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;

using SiliconStudio.BuildEngine.Monitor.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    /// <summary>
    /// Interaction logic for BuildSessionUserControl.xaml
    /// </summary>
    public partial class BuildSessionUserControl
    {
        public static readonly DependencyProperty BuildSessionProperty = DependencyProperty.Register("BuildSession", typeof(BuildSessionViewModel), typeof(BuildSessionUserControl));

        public BuildSessionUserControl()
        {
            InitializeComponent();
        }

    }
}
