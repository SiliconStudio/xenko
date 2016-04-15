using System.Collections.Generic;
using SiliconStudio.Presentation.Dirtiables;

namespace SiliconStudio.Presentation.Tests.Dirtiables
{
    public class SimpleDirtiable : IDirtiable
    {
        public bool IsDirty { get; private set; }

        public IEnumerable<IDirtiable> Yield()
        {
            yield return this;
        }

        public void UpdateDirtiness(bool value)
        {
            IsDirty = value;
        }
    }
}
