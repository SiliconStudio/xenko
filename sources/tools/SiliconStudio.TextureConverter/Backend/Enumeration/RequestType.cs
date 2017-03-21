// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
