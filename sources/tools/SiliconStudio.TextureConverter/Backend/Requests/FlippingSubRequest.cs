// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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