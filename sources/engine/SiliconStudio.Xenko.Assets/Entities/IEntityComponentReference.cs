// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public interface IEntityComponentReference
    {
        EntityReference Entity { get; }

        Guid Id { get; }

        EntityComponent Value { get; }
    }
}