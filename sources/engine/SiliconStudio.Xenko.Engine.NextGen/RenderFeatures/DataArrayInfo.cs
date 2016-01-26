using System;

namespace RenderArchitecture
{
    public struct DataArray
    {
        public Array Array;
        public readonly DataArrayInfo Info;

        public DataArray(DataArrayInfo info) : this()
        {
            Info = info;
        }
    }

    public abstract class DataArrayInfo
    {
        public DataType Type { get; }

        protected DataArrayInfo(DataType type)
        {
            Type = type;
        }

        /// <summary>
        /// Ensure this array has the given size.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="size"></param>
        public abstract void EnsureSize(ref Array array, int size);

        /// <summary>
        /// Move items around. Source will be reset to default values. No growing of array is expected.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="sourceStart"></param>
        /// <param name="destStart"></param>
        /// <param name="length"></param>
        public abstract void SwapRemoveItems(Array array, int sourceStart, int destStart, int length);
    }

    class DataArrayInfo<T> : DataArrayInfo
    {
        public DataArrayInfo(DataType type) : base(type)
        {
        }

        public override void EnsureSize(ref Array array, int size)
        {
            // TODO: we should probably shrink down if not used anymore during many frames)
            // Array has proper size already
            if (size == 0 || (array != null && size <= array.Length))
                return;

            var arrayT = (T[])array;
            Array.Resize(ref arrayT, size);
            array = arrayT;
        }

        public override void SwapRemoveItems(Array array, int sourceStart, int destStart, int length)
        {
            var arrayT = (T[])array;

            if (sourceStart != destStart)
            {
                for (int i = 0; i < length; ++i)
                    arrayT[destStart + i] = arrayT[sourceStart + i];
            }

            for (int i = 0; i < length; ++i)
                arrayT[sourceStart + i] = default(T);
        }
    }
}