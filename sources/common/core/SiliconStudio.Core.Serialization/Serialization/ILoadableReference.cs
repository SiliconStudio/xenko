using System;

namespace SiliconStudio.Core.Serialization
{
    public interface ILoadableReference
    {
        string Location { get; }

        Type Type { get; }
    }
}