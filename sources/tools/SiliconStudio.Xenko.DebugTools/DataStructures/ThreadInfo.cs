using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SiliconStudio.Paradox.DebugTools.DataStructures
{
    public class ThreadInfo
    {
        public int Id { get; set; }
        public List<MicroThreadInfo> MicroThreadItems { get; private set; }

        public ThreadInfo()
        {
            MicroThreadItems = new List<MicroThreadInfo>();
        }

        public ThreadInfo Duplicate()
        {
            ThreadInfo duplicate = new ThreadInfo();

            duplicate.Id = Id;
            MicroThreadItems.ForEach(item => duplicate.MicroThreadItems.Add(item.Duplicate()));

            return duplicate;
        }
    }
}
