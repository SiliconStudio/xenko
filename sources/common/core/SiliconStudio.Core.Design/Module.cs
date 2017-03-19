// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.TypeConverters;

namespace SiliconStudio.Core
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Make sure that this assembly is registered
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
            TypeDescriptor.AddAttributes(typeof(Color), new TypeConverterAttribute(typeof(ColorConverter)));
            TypeDescriptor.AddAttributes(typeof(Color3), new TypeConverterAttribute(typeof(Color3Converter)));
            TypeDescriptor.AddAttributes(typeof(Color4), new TypeConverterAttribute(typeof(Color4Converter)));
            TypeDescriptor.AddAttributes(typeof(Half), new TypeConverterAttribute(typeof(HalfConverter)));
            TypeDescriptor.AddAttributes(typeof(Half2), new TypeConverterAttribute(typeof(Half2Converter)));
            TypeDescriptor.AddAttributes(typeof(Half3), new TypeConverterAttribute(typeof(Half3Converter)));
            TypeDescriptor.AddAttributes(typeof(Half4), new TypeConverterAttribute(typeof(Half4Converter)));
            TypeDescriptor.AddAttributes(typeof(Matrix), new TypeConverterAttribute(typeof(MatrixConverter)));
            TypeDescriptor.AddAttributes(typeof(Quaternion), new TypeConverterAttribute(typeof(QuaternionConverter)));
            TypeDescriptor.AddAttributes(typeof(Vector2), new TypeConverterAttribute(typeof(Vector2Converter)));
            TypeDescriptor.AddAttributes(typeof(Vector3), new TypeConverterAttribute(typeof(Vector3Converter)));
            TypeDescriptor.AddAttributes(typeof(Vector4), new TypeConverterAttribute(typeof(Vector4Converter)));
        }
    }
}
