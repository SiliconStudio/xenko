// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_VULKAN
using System;
using SharpVulkan;

namespace SiliconStudio.Xenko.Graphics
{
    public partial class QueryPool
    {
        internal SharpVulkan.QueryPool NativeQueryPool;

        public unsafe bool TryGetData(long[] dataArray)
        {
            fixed (long* dataPointer = &dataArray[0])
            {
                // Read back all results
                var result = GraphicsDevice.NativeDevice.GetQueryPoolResults(NativeQueryPool, 0, (uint)QueryCount, QueryCount * 8, new IntPtr(dataPointer), 8, QueryResultFlags.Is64Bits);

                // Some queries are not ready yet
                if (result == Result.NotReady)
                    return false;
            }

            return true;
        }

        private unsafe void Recreate()
        {
            var createInfo = new QueryPoolCreateInfo
            {
                StructureType = StructureType.QueryPoolCreateInfo,
                QueryCount = (uint)QueryCount,
            };

            switch (QueryType)
            {
                case QueryType.Timestamp:
                    createInfo.QueryType = SharpVulkan.QueryType.Timestamp;
                    break;

                default:
                    throw new NotImplementedException();
            }

            NativeQueryPool = GraphicsDevice.NativeDevice.CreateQueryPool(ref createInfo);
        }

        /// <inheritdoc/>
        protected internal override void OnDestroyed()
        {
            GraphicsDevice.Collect(NativeQueryPool);
            NativeQueryPool = SharpVulkan.QueryPool.Null;

            base.OnDestroyed();
        }
    }
}
#endif
