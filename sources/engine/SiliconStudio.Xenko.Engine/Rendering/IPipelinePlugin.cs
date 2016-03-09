// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Automatically register part of the <see cref="RenderSystem"/> configuration.
    /// </summary>
    public interface IPipelinePlugin
    {
        /// <summary>
        /// Loads the plugin.
        /// </summary>
        /// <param name="context"></param>
        void Load(PipelinePluginContext context);

        /// <summary>
        /// Unloads the plugin.
        /// </summary>
        /// <param name="context"></param>
        void Unload(PipelinePluginContext context);
    }
}