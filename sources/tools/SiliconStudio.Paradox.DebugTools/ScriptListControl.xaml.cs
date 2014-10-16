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
using SiliconStudio.Paradox.Extensions;
using SiliconStudio.Paradox.Framework.MicroThreading;
using SiliconStudio.Paradox.DebugTools.ViewModels;

namespace SiliconStudio.Paradox.DebugTools
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
