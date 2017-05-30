// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Defines how a <see cref="RenderObject"/> gets assigned to specific <see cref="RenderStage"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class RenderStageSelector
    {
        public abstract void Process(RenderObject renderObject);
    }
}
