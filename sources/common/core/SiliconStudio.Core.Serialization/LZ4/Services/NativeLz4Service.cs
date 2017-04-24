// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Native;

namespace SiliconStudio.Core.LZ4.Services
{
    internal class NativeLz4Service : NativeLz4Base, ILZ4Service
    {
        public string CodecName => $"NativeMode {(IntPtr.Size == 4 ? "32" : "64")}";

        public unsafe int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                if (knownOutputLength)
                {
                    LZ4_uncompress(pInput + inputOffset, pOutput + outputOffset, outputLength);

                    return outputLength;
                }

                return LZ4_uncompress_unknownOutputSize(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }

        public unsafe int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                return LZ4_compress_limitedOutput(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }

        public unsafe int EncodeHC(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                return LZ4_compressHC_limitedOutput(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }
    }
}
