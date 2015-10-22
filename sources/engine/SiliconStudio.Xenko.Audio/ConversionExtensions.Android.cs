// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_ANDROID

using System;
using Android.Media;

namespace SiliconStudio.Xenko.Audio
{
    internal static class ConversionExtensions
    {
        public static ChannelConfiguration ToChannelConfig(this AudioChannels channelConfig)
        {
            switch (channelConfig)
            {
                case AudioChannels.Mono:
                    return ChannelConfiguration.Mono;
                case AudioChannels.Stereo:
                    return ChannelConfiguration.Stereo;
                default:
                    throw new ArgumentOutOfRangeException("channelConfig");
            }
        }

        public static ChannelOut ToChannelOut(this AudioChannels channelConfig)
        {
            switch (channelConfig)
            {
                case AudioChannels.Mono:
                    return ChannelOut.Mono;
                case AudioChannels.Stereo:
                    return ChannelOut.Stereo;
                default:
                    throw new ArgumentOutOfRangeException("channelConfig");
            }
        }

        public static Encoding ToEncoding(this AudioDataEncoding encoding)
        {
            switch (encoding)
            {
                case AudioDataEncoding.PCM_8Bits:
                    return Encoding.Pcm8bit;
                case AudioDataEncoding.PCM_16Bits:
                    return Encoding.Pcm16bit;
                default:
                    throw new ArgumentOutOfRangeException("encoding");
            }
        }
    }
}

#endif