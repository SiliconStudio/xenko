// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public class CompositingProfilingKeys
    {
        public static readonly ProfilingKey Compositing = new ProfilingKey("Compositing");

        public static readonly ProfilingKey Opaque = new ProfilingKey(Compositing, "Opaque");

        public static readonly ProfilingKey Transparent = new ProfilingKey(Compositing, "Transparent");

        public static readonly ProfilingKey MsaaResolve = new ProfilingKey(Compositing, "MSAA Resolve");

        public static readonly ProfilingKey LightShafts = new ProfilingKey(Compositing, "LightShafts");

        public static readonly ProfilingKey GBuffer = new ProfilingKey(Compositing, "GBuffer");
    }
}
