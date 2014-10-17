// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Effect
{
    /// <summary>
    /// A list of <see cref="IEffectParameterValueGenerator"/>.
    /// </summary>
    [DataContract("!fxparams")]
    public class EffectParameterValueGeneratorCollection : List<IEffectParameterValueGenerator>
    {
    }
}