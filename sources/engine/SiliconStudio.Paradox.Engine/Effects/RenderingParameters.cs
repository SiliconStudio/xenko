// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{

    // TODO: Remove this class
    [DataContract]
    public partial class RenderingParameters : ShaderMixinParameters
    {
        public static readonly ParameterKey<RenderGroups> RenderGroup = ParameterKeys.New(RenderGroups.All);
        public static readonly ParameterKey<RenderGroups> ActiveRenderGroup = ParameterKeys.New(RenderGroups.All);
    };
}
