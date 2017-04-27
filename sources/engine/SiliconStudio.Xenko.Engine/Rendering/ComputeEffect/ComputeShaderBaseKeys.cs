// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect
{
    public class ComputeShaderBaseKeys
    {   
        public static readonly ValueParameterKey<Int3> ThreadGroupCountGlobal = ParameterKeys.NewValue<Int3>();
    }
}
