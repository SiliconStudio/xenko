using System;

namespace SiliconStudio.Core.Serialization.Contents
{
    public interface ILoadableReference
    {
        string Location { get; }

        Type Type { get; }
    }
}