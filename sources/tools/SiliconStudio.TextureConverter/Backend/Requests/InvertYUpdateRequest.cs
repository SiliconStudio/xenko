// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.TextureConverter.Backend.Requests
{
    class InvertYUpdateRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.InvertYUpdate; } }

        public TexImage NormalMap { get; set; }
    }
}
