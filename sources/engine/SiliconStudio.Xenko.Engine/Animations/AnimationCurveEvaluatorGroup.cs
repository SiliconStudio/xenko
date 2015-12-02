// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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