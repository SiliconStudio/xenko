// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under BSD 2-Clause License. See LICENSE.md for details.
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.LZ4.Services
{
    internal abstract class NativeLz4Base
    {
        static NativeLz4Base()
        {
            NativeLibrary.PreloadLibrary(NativeLibrary.LibraryName);
        }

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected static extern unsafe int LZ4_uncompress(byte* source, byte* dest, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected static extern unsafe int LZ4_uncompress_unknownOutputSize(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected static extern unsafe int LZ4_compress_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected static extern unsafe int LZ4_compressHC_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);
    }
}
