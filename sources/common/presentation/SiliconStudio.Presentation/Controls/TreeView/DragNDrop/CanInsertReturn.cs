using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Controls.DragNDrop
{
    class CanInsertReturn
    {
        public CanInsertReturn(string format, int index, bool before)
        {
            Format = format;
            Index = index;
            Before = before;
        }

        public string Format { get; set; }

        public int Index { get; set; }
        
        public bool Before { get; set; }
    }
}
