// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core.Serialization.Serializers;

namespace SiliconStudio.Core.Serialization.Contents
{
    [DataSerializer(typeof(EmptyDataSerializer<ContentData>))]
    [DataContract(Inherited = true)]
    public abstract class ContentData : IContentData
    {
        [DataMemberIgnore]
        public string Url { get; set; }
    }
}
