using System;
using System.Linq;

using SiliconStudio.BuildEngine.Editor.ViewModel;

using Xceed.Wpf.AvalonDock.Layout;

namespace SiliconStudio.BuildEngine.Editor.View
{
    class BuildEditorLayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            var anchorableViewModel = anchorableToShow.Content as AnchorableViewModel;
            if (anchorableToShow.PreviousContainerIndex == -1 && anchorableViewModel != null)
            {
                string defaultPaneName = anchorableViewModel.DefaultPane;
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().Single(d => string.Compare(d.Name, defaultPaneName, StringComparison.OrdinalIgnoreCase) == 0);
                if (toolsPane != null)
                {
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
                    
            }
            return false;
        }

        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {

        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {

        }
    }
}
