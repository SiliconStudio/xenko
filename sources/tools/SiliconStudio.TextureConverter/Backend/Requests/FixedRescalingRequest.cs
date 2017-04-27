// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.TextureConverter.Requests
{
    /// <summary>
    /// Request a texture to be resized to the requested width and height.
    /// </summary>
    internal class FixedRescalingRequest : RescalingRequest
    {

        /// <summary>
        /// The width
        /// </summary>
        private readonly int width;


        /// <summary>
        /// The height
        /// </summary>
        private readonly int height;

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedRescalingRequest"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="filter">The filter.</param>
        public FixedRescalingRequest(int width, int height, Filter.Rescaling filter) : base(filter)
        {
            this.width = width;
            this.height = height;
        }

        public override int ComputeWidth(TexImage texImage)
        {
            return width;
        }

        public override int ComputeHeight(TexImage texImage)
        {
            return height;
        }

    }
}
