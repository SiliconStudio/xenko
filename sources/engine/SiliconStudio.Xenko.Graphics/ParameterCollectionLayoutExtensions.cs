// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    public static class ParameterCollectionLayoutExtensions
    {
        public static void ProcessResources(this ParameterCollectionLayout parameterCollectionLayout, DescriptorSetLayoutBuilder layout)
        {
            foreach (var layoutEntry in layout.Entries)
            {
                parameterCollectionLayout.LayoutParameterKeyInfos.Add(new ParameterKeyInfo(layoutEntry.Key, parameterCollectionLayout.ResourceCount++));
            }
        }

        public static void ProcessConstantBuffer(this ParameterCollectionLayout parameterCollectionLayout, ShaderConstantBufferDescription constantBuffer)
        {
            foreach (var member in constantBuffer.Members)
            {
                parameterCollectionLayout.LayoutParameterKeyInfos.Add(new ParameterKeyInfo(member.Param.Key, parameterCollectionLayout.BufferSize + member.Offset, member.Count));
            }
            parameterCollectionLayout.BufferSize += constantBuffer.Size;
        }
    }
}