using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace System.Windows.Controls
{
	public class SelectionChangedCancelEventArgs : CancelEventArgs
	{
		public IEnumerable<object> ItemsToUnSelect { get; private set; }
		public IEnumerable<object> ItemsToSelect { get; private set; }

		public SelectionChangedCancelEventArgs(IEnumerable<object> itemsToSelect, IEnumerable<object> itemsToUnSelect)
		{
			ItemsToSelect = itemsToSelect;
            ItemsToUnSelect = itemsToUnSelect;
		}
	}
}
