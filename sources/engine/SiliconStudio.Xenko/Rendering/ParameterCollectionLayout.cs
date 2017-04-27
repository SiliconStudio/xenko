// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Collections;

namespace SiliconStudio.Xenko.Rendering
{
    public class ParameterCollectionLayout
    {
        public FastListStruct<ParameterKeyInfo> LayoutParameterKeyInfos = new FastListStruct<ParameterKeyInfo>(0);
        public int ResourceCount;
        public int BufferSize;
    }
}
