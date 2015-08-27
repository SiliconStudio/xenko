using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System.Windows.Controls.DragNDrop
{
    public class DragContent : INotifyPropertyChanged
    {
        private bool canDrop;
        private bool canInsert;
        private int insertIndex;
        private List<object> draggedItems;

        public DragContent()
        {
            draggedItems = new List<object>();
        }

        public void Add(object draggedItem)
        {
            draggedItems.Add(draggedItem);
            RaisePropertyChanged("Count");
        }

        public bool CanInsert
        {
            get { return canInsert; }
            set
            {
                if (canInsert != value)
                {
                    canInsert = value;
                    RaisePropertyChanged("CanInsert");
                }
            }
        }
        
        public IEnumerable<object> Items { get { return draggedItems.AsEnumerable<object>(); } }

        public int Count { get { return draggedItems.Count; } }

        public bool CanDrop
        {
            get { return canDrop; }
            set
            {
                if (canDrop != value)
                {
                    canDrop = value;
                    RaisePropertyChanged("CanDrop");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public int InsertIndex
        //{
        //    get { return insertIndex; }
        //    set
        //    {
        //        if (insertIndex != value)
        //        {
        //            insertIndex = value;
        //            RaisePropertyChanged("InsertIndex");
        //        }
        //    }
        //}
    }
}
