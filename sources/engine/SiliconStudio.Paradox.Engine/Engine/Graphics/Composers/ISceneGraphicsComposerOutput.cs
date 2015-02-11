// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Graphics.Composers
{
    /// <summary>
    /// Defines the output of a <see cref="ISceneGraphicsCompositor"/>.
    /// </summary>
    public interface ISceneGraphicsComposerOutput : IDisposable
    {
        RenderFrame GetRenderFrame(RenderContext context);
    }
}