using System;
using System.Collections.Generic;
using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Rendering
{
    public partial struct RenderDataHolder
    {
        // storage for properties (struct of arrays)
        private Dictionary<object, int> dataArraysByDefinition;
        private FastListStruct<DataArray> dataArrays;

        public void Initialize()
        {
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

        public void PrepareDataArrays(Func<DataType, int> computeDataArrayExpectedSize)
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