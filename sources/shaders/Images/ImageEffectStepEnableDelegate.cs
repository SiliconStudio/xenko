// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Paradox.Effects.Images
{
    public delegate bool ImageEffectStepEnableDelegate(ImageEffectGroup imageEffectGroup, HashSet<ParameterKey> requiredKeys);
}