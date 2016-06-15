using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SimpleTerrain
{
    /// <summary>
    /// A container that represents HeightData which have the indexer for getting and setting height for the specific point, and gets the size of the data.
    /// </summary>
    public class HeightMap
    {
        private readonly float heightScale;

        private readonly float[] data;

        public readonly int Size;

        public float MedianHeight { get; private set; }

        private HeightMap(int size, float scale)
        {
            if (!MathUtil.IsPow2(size))
                throw new ArgumentException("Size of Terrain must be a power of two");

            data = new float[size * size];
            Size = size;
            heightScale = scale;
        }

        /// <summary>
        /// Get the height of the map of the provided position.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="z">The y coordinate</param>
        /// <returns></returns>
        public float GetHeight(int x, int z)
        {
            return heightScale * this[x, z];
        }
        
        /// <summary>
        /// Creates an height map using the Fault Formation algorithm.
        /// Produces smooth terrain by adding a random line to a blank height field, and add random height to one of the two sides.
        /// </summary>
        /// <param name="size">Size of square HeightData to be created</param>
        /// <param name="numberIteration">The number of iteration of the algorithm. More iteration yields more spaces</param>
        /// <param name="minHeight">Min value of height produced from the algorithm</param>
        /// <param name="maxHeight">Max value of height produced from the algorithm</param>
        /// <param name="scaleHeight"></param>
        /// <param name="filter"></param>
        /// <returns>HeightData created with Fault Formation algorithm that has "size" of size</returns>
        public static HeightMap GenerateFaultFormation(int size, int numberIteration, float minHeight, float maxHeight, float scaleHeight, float filter)
        {
            var heightMap = new HeightMap(size, scaleHeight);

            var random = new Random();

            for (var i = 0; i < numberIteration; ++i)
            {
                // Calculate height for this iteration
                var height = maxHeight - (maxHeight - minHeight) * i / numberIteration;
                var currentPassHeight = height * ((float)random.NextDouble() - 0.1f);

                // Find the line mark a half space for this iteration
                var point1 = new Point(random.Next(size), random.Next(size));
                var point2 = new Point(random.Next(size), random.Next(size));

                var halfSpaceLineVector = new Vector2(point2.X - point1.X, point2.Y - point1.Y);

                for (var iX = 0; iX < size; ++iX)
                {
                    for (var iZ = 0; iZ < size; ++iZ)
                    {
                        var currentPointLine = new Vector2(iX - point1.X, iZ - point1.Y);

                        float sign;
                        Vector2.Dot(ref halfSpaceLineVector, ref currentPointLine, out sign);

                        if (sign > 0) heightMap[iX, iZ] += currentPassHeight;
                    }
                }

                heightMap.FilterHeightField(filter);
            }

            heightMap.NormalizeHeightMap();
            heightMap.CalculateMedian();

            return heightMap;
        }

        private float this[int x, int z]
        {
            get { return data[x * Size + z]; }
            set { data[x * Size + z] = value; }
        }

        private void NormalizeHeightMap()
        {
            var maxHeight = float.MinValue;
            var minHeight = float.MaxValue;

            for (var i = 0; i < data.Length; ++i)
            {
                if (maxHeight < data[i]) maxHeight = data[i];
                if (minHeight > data[i]) minHeight = data[i];
            }

            maxHeight -= minHeight;

            for (var i = 0; i < data.Length; ++i)
                data[i] = (data[i] - minHeight) / maxHeight;
        }

        private void CalculateMedian()
        {
            MedianHeight = heightScale*(data.Min() + data.Max())/2;
        }

        private void FilterHeightField(float filter)
        {
            // Erode left to right   
            for (var i = 0; i < Size; i++)
                FilterHeightBand(Size * i, 1, Size, filter);

            //erode right to left   
            for (var i = 0; i < Size; i++)
                FilterHeightBand(Size * i + Size - 1, -1, Size, filter);

            //erode top to bottom   
            for (var i = 0; i < Size; i++)
                FilterHeightBand(i, Size, Size, filter);

            //erode from bottom to top   
            for (var i = 0; i < Size; i++)
                FilterHeightBand(Size * (Size - 1) + i, -Size, Size, filter);
        }

        /// <summary>
        /// Filters HeightData using band-based filter. by "Jason Shankel"
        /// It simulates terrain erosion.
        /// </summary>
        private void FilterHeightBand(int startIndex, int stride, int count, float filter)
        {
            var v = data[startIndex];
            var j = stride;

            //go through the height band and apply the erosion filter   
            for (var i = 0; i < count - 1; ++i)
            {
                data[startIndex + j] = filter * v + (1 - filter) * data[startIndex + j];

                v = data[startIndex + j];
                j += stride;
            }
        }
    }
}
