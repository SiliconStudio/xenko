using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiliconStudio.Core
{
    public class UnmanagedArray<T> : IDisposable where T : struct
    {
        private readonly int sizeOfT;

        public UnmanagedArray(int length)
        {
            this.Length = length;
            sizeOfT = Utilities.SizeOf<T>();
            var finalSize = length * sizeOfT;
            Pointer = Utilities.AllocateMemory(finalSize);
        }

        public UnmanagedArray(int length, IntPtr unmanagedDataPtr)
        {
            this.Length = length;
            sizeOfT = Utilities.SizeOf<T>();
            Pointer = unmanagedDataPtr;
        }

        public void Dispose()
        {
            Utilities.FreeMemory(Pointer);
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
                    var bptr = (byte*)Pointer.ToPointer();
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
                    var bptr = (byte*)Pointer.ToPointer();
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
                Interop.Read(Pointer.ToPointer(), destination, offset, destination.Length);
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
                Interop.Write(Pointer.ToPointer(), source, offset, source.Length);
            }
        }

        public IntPtr Pointer { get; }

        public int Length { get; }
    }
}
