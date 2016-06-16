using System;

namespace SiliconStudio.Core
{
    public class UnmanagedArray<T> : IDisposable where T : struct
    {
        private readonly int sizeOfT;
        private readonly bool dontFree;

        public UnmanagedArray(int length)
        {
            Length = length;
            sizeOfT = Utilities.SizeOf<T>();
            var finalSize = length * sizeOfT;
            Pointer = Utilities.AllocateMemory(finalSize);
            dontFree = false;
        }

        public UnmanagedArray(int length, IntPtr unmanagedDataPtr)
        {
            Length = length;
            sizeOfT = Utilities.SizeOf<T>();
            Pointer = unmanagedDataPtr;
            dontFree = true;
        }

        public void Dispose()
        {
            if (!dontFree)
            {
                Utilities.FreeMemory(Pointer);
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                var res = new T();

                unsafe
                {
                    var bptr = (byte*)Pointer;
                    bptr += index*sizeOfT;                   
                    Interop.Read<T>(bptr, ref res);
                }

                return res;
            }
            set
            {
                if (index >= Length)
                {
                    throw new ArgumentOutOfRangeException();
                }

                unsafe
                {
                    var bptr = (byte*)Pointer;
                    bptr += index * sizeOfT;
                    Interop.Write<T>(bptr, ref value);
                }
            }
        }

        public void Read(T[] destination, int offset = 0)
        {
            if (offset + destination.Length > Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            unsafe
            {
                Interop.Read((void*)Pointer, destination, offset, destination.Length);
            }        
        }

        public void Read(T[] destination, int pointerByteOffset, int arrayOffset, int arrayLen)
        {
            if (arrayOffset + arrayLen > Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            unsafe
            {
                var ptr = (byte*)Pointer;
                ptr += pointerByteOffset;
                Interop.Read(ptr, destination, arrayOffset, arrayLen);
            }
        }

        public void Write(T[] source, int offset = 0)
        {
            if (offset + source.Length > Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            unsafe
            {
                Interop.Write((void*)Pointer, source, offset, source.Length);
            }
        }

        public void Write(T[] source, int pointerByteOffset, int arrayOffset, int arrayLen)
        {
            if (arrayOffset + arrayLen > Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            unsafe
            {
                var ptr = (byte*)Pointer;
                ptr += pointerByteOffset;
                Interop.Write(ptr , source, arrayOffset, arrayLen);
            }
        }

        public IntPtr Pointer { get; }

        public int Length { get; }
    }
}
