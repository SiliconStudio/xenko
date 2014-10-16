// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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