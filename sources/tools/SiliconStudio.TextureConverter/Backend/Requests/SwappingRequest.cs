// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.TextureConverter.Requests
{
    internal class SwappingRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.Swapping; } }

        /// <summary>
        /// The first face.
        /// </summary>
        public int FirstSubImageIndex { get; set; }

        /// <summary>
        /// The second face.
        /// </summary>
        public int SecondSubImageIndex { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SwappingRequest"/> class.
        /// </summary>
        public SwappingRequest(int i, int j)
        {
            FirstSubImageIndex = i;
            SecondSubImageIndex = j;
        }
    }
}
