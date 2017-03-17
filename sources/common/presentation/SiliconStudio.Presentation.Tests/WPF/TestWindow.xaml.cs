using System;

namespace SiliconStudio.Presentation.Tests.WPF
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow
    {
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
