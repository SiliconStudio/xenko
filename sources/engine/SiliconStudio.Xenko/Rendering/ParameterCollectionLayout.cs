// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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