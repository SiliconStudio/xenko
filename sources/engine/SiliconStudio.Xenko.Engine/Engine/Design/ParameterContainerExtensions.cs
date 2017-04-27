// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Engine.Design
{
    public class ParameterContainerExtensions
    {
        public static SerializerSelector DefaultSceneSerializerSelector;

        static ParameterContainerExtensions()
        {
            DefaultSceneSerializerSelector = new SerializerSelector("Default", "Content");
        }
    }
}
