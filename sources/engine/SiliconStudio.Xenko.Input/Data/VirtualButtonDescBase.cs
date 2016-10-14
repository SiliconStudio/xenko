// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Xenko.Input.Data
{
    [ContentSerializer(typeof(DataContentSerializer<VirtualButtonDescBase>))]
    [DataContract]
    public abstract class VirtualButtonDescBase
    {
        [DataMember]
        public bool Inverted;
        [DataMember]
        public float Sensitivity = 1.0f;
    }
}
