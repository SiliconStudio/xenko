namespace System.Windows.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows.Input;

    internal class InputEventRouter : IDisposable
    {
        protected TreeViewEx treeView;

        private List<InputSubscriberBase> inputSubscribers;

        private bool isLeftMouseButtonDown;

        private Point mouseDownPoint;

        public InputEventRouter(TreeViewEx treeView)
        {
            inputSubscribers = new List<InputSubscriberBase>(2);
            this.treeView = treeView;

            treeView.MouseDown += OnMouseDown;
            treeView.MouseMove += OnMouseMove;
            treeView.MouseUp += OnMouseUp;
            if (treeView.ScrollViewer != null)
            {
                treeView.ScrollViewer.ScrollChanged += OnScrollChanged;
            }
        }

        void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            Call("OnScrollChanged", e);
        }

        internal void Add(InputSubscriberBase inputSubscriber)
        {
            inputSubscriber.TreeView = treeView;
            inputSubscribers.Add(inputSubscriber);
            inputSubscriber.Initialized();
        }

        internal void Remove(InputSubscriberBase inputSubscriber)
        {
            inputSubscribers.Remove(inputSubscriber);
            inputSubscriber.Detached();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(treeView, CaptureMode.SubTree);
            isLeftMouseButtonDown = true;
            mouseDownPoint = e.GetPosition(treeView);
            Call("OnMouseDown", e);
        }

        private void Call(string methodName, object e)
        {
            foreach (var inputSubscriber in inputSubscribers)
            {
                // initialize provider
                inputSubscriber.IsLeftButtonDown = isLeftMouseButtonDown;

                // Debug.WriteLine("Calling " + methodName + " on " + inputSubscriber.GetType());
                MethodInfo methodInfo = typeof(InputSubscriberBase).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                methodInfo.Invoke(inputSubscriber, new object[] { e });

                RoutedEventArgs re = e as RoutedEventArgs;
                if (re != null && re.Handled) break;
            }

            // Debug.WriteLine(DateTime.Now.Millisecond + " -----------------------------");
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            isLeftMouseButtonDown = e.LeftButton == MouseButtonState.Pressed;
            Call("OnMouseMove", e);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            Call("OnMouseUp", e);
            isLeftMouseButtonDown = false;
            Mouse.Capture(null);
        }

        public void Dispose()
        {
            if (treeView != null)
            {
                treeView.MouseDown -= OnMouseDown;
                treeView.MouseMove -= OnMouseMove;
                treeView.MouseUp -= OnMouseUp;
                if (treeView.ScrollViewer != null)
                {
                    treeView.ScrollViewer.ScrollChanged -= OnScrollChanged;
                }

                treeView = null;
            }

            if (inputSubscribers != null)
            {
                inputSubscribers.Clear();
                inputSubscribers = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
