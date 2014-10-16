using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SiliconStudio.Presentation.Core;

namespace SiliconStudio.BuildEngine.Editor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ThemeSelector.SetTheme(
                Application.Current,
                ThemeSelector.ToUri("SiliconStudio.Presentation", "ExpressionDark/Theme.xaml"),
                ThemeSelector.ToUri("SiliconStudio.Presentation.TreeView", "ExpressionDark/Theme.xaml"));
        }
    }
}
