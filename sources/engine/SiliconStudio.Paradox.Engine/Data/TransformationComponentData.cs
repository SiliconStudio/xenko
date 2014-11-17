// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Data;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Engine.Data
{
    public partial class TransformationComponentData
    {
        public TransformationComponentData()
        {
            UseTRS = true;
            Scaling = Vector3.One;
            Children = new FastCollection<EntityComponentReference<TransformationComponent>>();
        }
    }
}