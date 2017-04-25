// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace SiliconStudio.TextureConverter
{
    /// <summary>
    /// Request type, used internally, to check whether a library can handle a request.
    /// </summary>
    internal enum RequestType
    {
        Loading,
        Rescaling,
        Compressing,
        Converting,
        SwitchingChannels,
        Flipping,
        FlippingSub,
        Swapping,
        Export,
        Decompressing,
        MipMapsGeneration,
        ExportToXenko,
        NormalMapGeneration,
        GammaCorrection,
        PreMultiplyAlpha,
        ColorKey,
        AtlasCreation,
        AtlasExtraction,
        ArrayCreation,
        ArrayExtraction,
        AtlasUpdate,
        ArrayUpdate,
        ArrayInsertion,
        ArrayElementRemoval,
        CubeCreation,
        InvertYUpdate
    }
}
