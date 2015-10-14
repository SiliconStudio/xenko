// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Animations
{
    public abstract class AnimationCurveEvaluatorGroup
    {
        public abstract void Evaluate(CompressedTimeSpan newTime, IntPtr location);
        public abstract void Evaluate(CompressedTimeSpan newTime, object[] results);
    }
}