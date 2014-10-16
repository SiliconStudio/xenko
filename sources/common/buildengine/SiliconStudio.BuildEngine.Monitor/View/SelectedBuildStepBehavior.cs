// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using SiliconStudio.BuildEngine.Monitor.ViewModel;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    class SelectedBuildStepBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty BuildSessionProperty = DependencyProperty.Register("BuildSession", typeof(BuildSessionViewModel), typeof(SelectedBuildStepBehavior), new PropertyMetadata());

        public BuildSessionViewModel BuildSession
        {
            get { return (BuildSessionViewModel)GetValue(BuildSessionProperty); }
            set { SetValue(BuildSessionProperty, value); }
        }

        public static readonly DependencyProperty ExecutionIdProperty = DependencyProperty.Register("ExecutionId", typeof(long), typeof(SelectedBuildStepBehavior), new PropertyMetadata());

        public long ExecutionId
        {
            get { return (long)GetValue(ExecutionIdProperty); }
            set { SetValue(ExecutionIdProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseDown += SelectAssociatedObject;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= SelectAssociatedObject;
            base.OnDetaching();
        }

        private void SelectAssociatedObject(object sender, MouseButtonEventArgs e)
        {
            if (BuildSession.SelectedId != ExecutionId)
            {
                BuildSession.SelectBuildStep(ExecutionId);
            }
            else
            {
                BuildSession.SelectBuildStep(-1);
            }
        }
    }
}
