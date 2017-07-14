// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Core.Serialization.Contents
{
    public interface ILoadableReference
    {
        string Location { get; }

        [NotNull]
        Type Type { get; }
    }
}
