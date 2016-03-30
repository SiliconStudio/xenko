// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Shaders.Convertor
{
    /// <summary>
    /// Enum to specify pipeline stage.
    /// </summary>
    public enum PipelineStage
    {
        Vertex = 0,
        Hull = 1,
        Domain = 2,
        Geometry = 3,
        Pixel = 4,
        Compute = 5,
        None = 6,
    }

    /// <summary>
    /// Helper functions for <see cref="PipelineStage"/>.
    /// </summary>
    public static class PipelineStageHelper
    {
        /// <summary>
        /// Parse a pipeline stage from string.
        /// </summary>
        /// <param name="stage">The stage in string form.</param>
        /// <exception cref="ArgumentException">If stage string is an invalid string.</exception>
        /// <returns>A PipelineStage value.</returns>
        public static PipelineStage FromString(string stage)
        {
            switch (stage.ToLowerInvariant())
            {
                case "vs":
                case "vertex":
                    return PipelineStage.Vertex;
                case "ps":
                case "pixel":
                    return PipelineStage.Pixel;
                case "gs":
                case "geometry":
                    return PipelineStage.Geometry;
            }

            throw new ArgumentException("stage is invalid. Must be vs/vertex, ps/pixel, gs/geometry.", "stage");
        }
    }
}
