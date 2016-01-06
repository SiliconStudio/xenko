// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public interface IEntityComponentReference
    {
        Guid EntityId { get; }

        Guid ComponentId { get; }

        EntityComponent Value { get; }
    }
}