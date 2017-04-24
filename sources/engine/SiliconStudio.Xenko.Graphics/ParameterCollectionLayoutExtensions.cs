// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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

        public static void ProcessConstantBuffer(this ParameterCollectionLayout parameterCollectionLayout, EffectConstantBufferDescription constantBuffer)
        {
            foreach (var member in constantBuffer.Members)
            {
                parameterCollectionLayout.LayoutParameterKeyInfos.Add(new ParameterKeyInfo(member.KeyInfo.Key, parameterCollectionLayout.BufferSize + member.Offset, member.Type.Elements > 0 ? member.Type.Elements : 1));
            }
            parameterCollectionLayout.BufferSize += constantBuffer.Size;
        }
    }
}
