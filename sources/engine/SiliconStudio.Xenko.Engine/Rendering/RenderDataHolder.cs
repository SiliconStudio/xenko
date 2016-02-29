using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Rendering
{
    public partial struct RenderDataHolder
    {
        // storage for properties (struct of arrays)
        private FastListStruct<DataArray> dataArrays;
        private Dictionary<object, int> dataArraysByDefinition;
        private Func<DataType, int> computeDataArrayExpectedSize;

        public void Initialize(Func<DataType, int> computeDataArrayExpectedSize)
        {
            this.computeDataArrayExpectedSize = computeDataArrayExpectedSize;
            dataArraysByDefinition = new Dictionary<object, int>();
            dataArrays = new FastListStruct<DataArray>(8);
        }

        public void SwapRemoveItem(DataType dataType, int source, int dest)
        {
            for (int i = 0; i < dataArrays.Count; ++i)
            {
                var dataArray = dataArrays[i];
                if (dataArray.Info.Type == dataType)
                    dataArray.Info.SwapRemoveItem(dataArray.Array, source, dest);
            }
        }

        public void PrepareDataArrays()
        {
            for (int i = 0; i < dataArrays.Count; ++i)
            {
                var dataArrayInfo = dataArrays[i].Info;
                var expectedSize = computeDataArrayExpectedSize(dataArrayInfo.Type);

                dataArrayInfo.EnsureSize(ref dataArrays.Items[i].Array, expectedSize);
            }
        }
    }
}