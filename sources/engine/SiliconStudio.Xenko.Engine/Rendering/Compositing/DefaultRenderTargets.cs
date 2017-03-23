using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public class ColorTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class NormalTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class VelocityTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = null;
    }

    public class OctaNormalSpecColorTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputNormalSpec";
    }

    public class EnvlightRoughnessTargetSemantic : IRenderTargetSemantic
    {
        public string ShaderClass { get; } = "GBufferOutputIblRoughness";
    }
}
