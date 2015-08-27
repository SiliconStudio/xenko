namespace System.Windows.Controls
{
	#region

	using System.Threading;
	using System.Windows;
	using System.Windows.Threading;
	using System.Diagnostics;

	#endregion

	/// <summary>
	/// Helper methods to focus.
	/// </summary>
	public static class FocusHelper
	{
		#region Public Methods

		public static void Focus(EditTextBox element)
		{
			//System.Diagnostics.Debug.WriteLine("Focus textbox with helper:" + element.Text);
			FocusCore(element);
		}

		public static void Focus(TreeViewExItem element)
		{
			// System.Diagnostics.Debug.WriteLine("Focus with helper item: " + element.DataContext);
			FocusCore(element);
		}

		public static void Focus(TreeViewEx element)
		{
			//System.Diagnostics.Debug.WriteLine("Focus Tree with helper");
			FocusCore(element);
		}

		private static void FocusCore(FrameworkElement element)
		{
			// System.Diagnostics.Debug.WriteLine("Focus core: " + element.DataContext);
			if (!element.Focus())
			{
				element.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => element.Focus()));
			}

#if DEBUG
			// no good idea, seems to block sometimes
			int i = 0;
			while (i < 5)
			{
				if (element.IsFocused) return;
				Thread.Sleep(100);
				i++;
			}
			if (i >= 5)
			{
			}
#endif
		}

		#endregion
	}
}