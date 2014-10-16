// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under BSD 2-Clause License. See LICENSE.md for details.
using System;

namespace SiliconStudio.Core.LZ4.Services
{
    internal class Native32LZ4Service : NativeLZ4Base, ILZ4Service
    {
        public string CodecName
        {
            get { return string.Format("NativeMode {0}", IntPtr.Size == 4 ? "32" : "64"); }
        }

        public unsafe int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength, bool knownOutputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                if (knownOutputLength)
                {
                    I32_LZ4_uncompress(pInput + inputOffset, pOutput + outputOffset, outputLength);

                    return outputLength;
                }

                return I32_LZ4_uncompress_unknownOutputSize(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }

        public unsafe int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                return I32_LZ4_compress_limitedOutput(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }

        public unsafe int EncodeHC(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            fixed (byte* pInput = input)
            fixed (byte* pOutput = output)
            {
                return I32_LZ4_compressHC_limitedOutput(pInput + inputOffset, pOutput + outputOffset, inputLength, outputLength);
            }
        }
    }
}