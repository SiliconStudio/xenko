// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine.Graphics
{
    /// <summary>
    /// A forward rendering mode.
    /// </summary>
    [DataContract("CameraRendererModeForward")]
    [Display("Forward")]
    public class CameraRendererModeForward : CameraRendererMode
    {
        // TODO: Do we need a special instance for this class? Check this with the implem of a Deferred renderer

        public override string GetMainModelEffect()
        {
            return "ParadoxBaseShader";
        }
    }
}