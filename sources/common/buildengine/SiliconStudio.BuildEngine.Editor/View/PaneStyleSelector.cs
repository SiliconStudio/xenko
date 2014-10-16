using System.Windows;
using System.Windows.Controls;

using SiliconStudio.BuildEngine.Editor.ViewModel;

namespace SiliconStudio.BuildEngine.Editor.View
{
    class PaneStyleSelector : StyleSelector
    {
        public Style BuildStepStyle { get; set; }
        public Style AnchorableStyle { get; set; }
         
        public override Style SelectStyle(object item, DependencyObject container)
        {
            return item is BuildSessionViewModel ? BuildStepStyle : AnchorableStyle;
        }
    }
}
