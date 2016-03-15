// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect
{
    public class ComputeShaderBaseKeys
    {   
        public static readonly ValueParameterKey<Int3> ThreadGroupCountGlobal = ParameterKeys.NewValue<Int3>();
    }
}