// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under BSD 2-Clause License. See LICENSE.md for details.
using System.Runtime.InteropServices;

namespace SiliconStudio.Core.LZ4.Services
{
    internal abstract class NativeLZ4Base
    {
        static NativeLZ4Base()
        {
            NativeLibrary.PreloadLibrary(NativeLibrary.LibraryName);
        }

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I32_LZ4_uncompress(byte* source, byte* dest, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I32_LZ4_uncompress_unknownOutputSize(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I32_LZ4_compress_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I32_LZ4_compressHC_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I64_LZ4_uncompress(byte* source, byte* dest, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I64_LZ4_uncompress_unknownOutputSize(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I64_LZ4_compress_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);

        [DllImport(NativeLibrary.LibraryName, CallingConvention = NativeLibrary.CallConvention)]
        protected unsafe static extern int I64_LZ4_compressHC_limitedOutput(byte* source, byte* dest, int inputSize, int maxOutputSize);

    }
}