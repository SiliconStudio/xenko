// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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