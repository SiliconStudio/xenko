// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Xenko.Updater;

namespace SiliconStudio.Xenko.Animations
{
    public abstract class AnimationCurveEvaluatorGroup
    {
        public abstract Type ElementType { get; }

        public abstract void Evaluate(CompressedTimeSpan newTime, IntPtr data, UpdateObjectData[] objects);

        public virtual void Cleanup()
        {
        }
    }
}
