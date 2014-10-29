// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.EntityModel.Data;

namespace SiliconStudio.Paradox.Data
{
    public interface IEntityComponentReference
    {
        EntityReference Entity { get; }

        PropertyKey Component { get; set; }

        Type ComponentType { get; }

        EntityComponentData Value { get; set; }
    }
}