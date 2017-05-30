// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.TextureConverter.Requests
{
    internal class FlippingSubRequest : FlippingRequest
    {
        public override RequestType Type { get { return RequestType.FlippingSub; } }

        /// <summary>
        /// The index of the sub-image to flip.
        /// </summary>
        public int SubImageIndex;

        public FlippingSubRequest(int index, Orientation orientation)
            : base(orientation)
        {
            SubImageIndex = index;
        }
    }
}
