using System;
using System.Collections.Generic;

namespace SiliconStudio.ActionStack.Tests.Helpers
{
    public class SimpleDirtiable : IDirtiable
    {
        public bool IsDirty { get; private set; }

        public event EventHandler<DirtinessUpdatedEventArgs> DirtinessUpdated;

        public IEnumerable<IDirtiable> Yield()
        {
            yield return this;
        }

        public void UpdateDirtiness(bool value)
        {
            var previousValue = IsDirty;
            IsDirty = value;
            DirtinessUpdated?.Invoke(this, new DirtinessUpdatedEventArgs(previousValue, IsDirty));
        }
    }
}