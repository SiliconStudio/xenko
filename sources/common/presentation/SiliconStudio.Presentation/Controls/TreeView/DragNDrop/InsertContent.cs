using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Controls.DragNDrop
{
    class InsertContent
    {
        public bool Before { get; set; }

        public Point Position { get; set; }

        public Media.Brush InsertionMarkerBrush { get; set; }

        public TreeViewExItem Item { get; set; }
    }
}
