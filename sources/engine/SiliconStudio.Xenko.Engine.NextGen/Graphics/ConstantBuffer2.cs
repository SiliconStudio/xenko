using System;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Graphics
{
    // TODO: Integrate this logic directly in D3D11 renderer (not needed for D3D11.1)
    /// <summary>
    /// Helper to manipulate cbuffer just like if they supported D3D11.1+ new features of binding them with offset and size.
    /// </summary>
    public class ConstantBuffer2
    {
        public int Size;
        public IntPtr Data;

        public ConstantBuffer2(int size)
        {
            Size = size;
            Data = Marshal.AllocHGlobal(size);
        }
    }
}