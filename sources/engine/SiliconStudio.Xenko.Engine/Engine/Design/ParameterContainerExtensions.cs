// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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