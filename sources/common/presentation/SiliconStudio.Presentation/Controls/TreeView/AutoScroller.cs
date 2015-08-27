using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows.Input;

namespace System.Windows.Controls
{
    class AutoScroller : InputSubscriberBase
    {
        public const uint scrollBorderSize = 10;
        public const double scrollDelta = 30;
        public const uint scrollDelay = 50;
        Timer timer;

        internal bool IsEnabled { get; set; }

        internal override void OnMouseMove(Input.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!IsEnabled) return;
            if (!IsLeftButtonDown) return;
            if (timer != null) return;
            Point position = e.GetPosition(TreeView);
            if (position.Y < scrollBorderSize)
            {
                //scroll down
                ScrollContinously(-scrollDelta);
            }
            else if ((TreeView.RenderSize.Height - position.Y) < scrollBorderSize)
            {
                //scroll up
                ScrollContinously(scrollDelta);
            }
        }

        private void ScrollContinously(double delta)
        {
            timer = new Timer(200);
            timer.AutoReset = true;
            timer.Elapsed += OnTimerElapsed;
            timer.Start(); //starts scrolling after given time
        }

        void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            TreeView.Dispatcher.Invoke(new Action(() =>
            {
                if (Mouse.LeftButton == MouseButtonState.Released)
                {
                    timer.Elapsed -= OnTimerElapsed;
                    timer = null;
                    return;
                }

                Point mousePosition = GetMousePosition();
                if (mousePosition.Y < scrollBorderSize)
                {
                    //scroll down
                    Scroll(-scrollDelta);
                }
                else if ((TreeView.RenderSize.Height - mousePosition.Y) < scrollBorderSize)
                {
                    //scroll up
                    Scroll(scrollDelta);
                }
                else
                {
                    timer.Elapsed -= OnTimerElapsed;
                    timer = null;
                }
            }));
        }

        internal void Scroll(double delta)
        {
            TreeView.ScrollViewer.ScrollToVerticalOffset(TreeView.ScrollViewer.VerticalOffset + delta);
        }
    }
}
