// Copyright (c) 2014-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.TextureConverter.Backend.Requests
{
    class InvertYUpdateRequest : IRequest
    {
        public override RequestType Type { get { return RequestType.InvertYUpdate; } }

        public TexImage NormalMap { get; set; }
    }
}
