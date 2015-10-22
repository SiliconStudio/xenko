// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from NAudio project. http://naudio.codeplex.com/
// Greetings to Mark Heath.
// -----------------------------------------------------------------------------

using System;
using System.Globalization;
using System.Runtime.InteropServices;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Audio.Wave
{
    /// <summary>
    /// WaveFormatExtensible
    /// http://www.microsoft.com/whdc/device/audio/multichaud.mspx
    /// </summary>
    internal class WaveFormatExtensible : WaveFormat
    {
        private short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0        

        /// <summary>
        /// Guid of the subformat.
        /// </summary>
        public Guid GuidSubFormat;

        /// <summary>
        /// Speaker configuration
        /// </summary>
        public Speakers ChannelMask; // which channels are present in stream


        /// <summary>
        /// Parameterless constructor for marshalling
        /// </summary>
        internal WaveFormatExtensible()
        {
        }

        /// <summary>
        /// Creates a new WaveFormatExtensible for PCM or IEEE
        /// </summary>
        public WaveFormatExtensible(int rate, int bits, int channels)
            : base(rate, bits, channels)
        {
            waveFormatTag = WaveFormatEncoding.Extensible;
            extraSize = 22;
            wValidBitsPerSample = (short)bits;
            int dwChannelMask = 0;
            for (int n = 0; n < channels; n++)
                dwChannelMask |= (1 << n);
            ChannelMask = (Speakers)dwChannelMask;

            // KSDATAFORMAT_SUBTYPE_IEEE_FLOAT // AudioMediaSubtypes.MEDIASUBTYPE_IEEE_FLOAT
            // KSDATAFORMAT_SUBTYPE_PCM // AudioMediaSubtypes.MEDIASUBTYPE_PCM;
            GuidSubFormat = bits == 32 ? new Guid("00000003-0000-0010-8000-00aa00389b71") : new Guid("00000001-0000-0010-8000-00aa00389b71");
        }

        protected override unsafe IntPtr MarshalToPtr()
        {
            var result = Marshal.AllocHGlobal(Utilities.SizeOf<__Native>());
            __MarshalTo(ref *(__Native*)result);
            return result;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 2)]
        internal new struct __Native
        {
            public WaveFormat.__Native waveFormat;

            public short wValidBitsPerSample; // bits of precision, or is wSamplesPerBlock if wBitsPerSample==0

            public Speakers dwChannelMask; // which channels are present in stream

            public Guid subFormat;

            // Method to free native struct
            internal void __MarshalFree()
            {
                waveFormat.__MarshalFree();
            }
        }

        // Method to marshal from native to managed struct
        internal void __MarshalFrom(ref __Native @ref)
        {
            __MarshalFrom(ref @ref.waveFormat);
            wValidBitsPerSample = @ref.wValidBitsPerSample;
            ChannelMask = @ref.dwChannelMask;
            GuidSubFormat = @ref.subFormat;
        }

        // Method to marshal from managed struct tot native
        internal void __MarshalTo(ref __Native @ref)
        {
            __MarshalTo(ref @ref.waveFormat);
            @ref.wValidBitsPerSample = wValidBitsPerSample;
            @ref.dwChannelMask = ChannelMask;
            @ref.subFormat = GuidSubFormat;
        }

        internal static __Native __NewNative()
        {
            {
                __Native temp = default(__Native);
                temp.waveFormat.extraSize = 22;
                return temp;
            }
        }

        /// <summary>
        /// String representation
        /// </summary>
        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "{0} wBitsPerSample:{1} ChannelMask:{2} SubFormat:{3} extraSize:{4}",
                base.ToString(),
                wValidBitsPerSample,
                ChannelMask,
                GuidSubFormat,
                extraSize);
        }
    }
}