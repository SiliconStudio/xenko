// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Graphics.Internals
{
    internal class ParameterConstantBuffer : ComponentBase
    {
        ConstantBufferData[] constantBufferDatas;
        public SiliconStudio.Paradox.Graphics.Buffer Buffer { get; private set; }
        DataPointer[] dataStreams;
        internal ShaderConstantBufferDescription ConstantBufferDesc;
        private bool forceDataChanged = false;

#if SILICONSTUDIO_PARADOX_GRAPHICS_API_OPENGL
        private static readonly bool UsingMap = false;
#else
        private static readonly bool UsingMap = true;
#endif

        public ParameterConstantBuffer(GraphicsDevice device, string constantBufferName, ShaderConstantBufferDescription constantBufferDesc)
        {
            ConstantBufferDesc = constantBufferDesc;
            constantBufferDatas = new ConstantBufferData[GraphicsDevice.ThreadCount];
            dataStreams = new DataPointer[GraphicsDevice.ThreadCount];

            for (uint i = 0; i < GraphicsDevice.ThreadCount; ++i)
            {
                constantBufferDatas[i] = new ConstantBufferData(constantBufferDesc);
                dataStreams[i] = new DataPointer(constantBufferDatas[i].Data, constantBufferDesc.Size);
            }

            Buffer = SiliconStudio.Paradox.Graphics.Buffer.New(device, constantBufferDatas[0].Desc.Size, BufferFlags.ConstantBuffer, UsingMap ? GraphicsResourceUsage.Dynamic : GraphicsResourceUsage.Default);
            
            // We want to clear flags
            // TODO: Should be later replaced with either an internal field on GraphicsResourceBase, or a reset counter somewhere?
            Buffer.Reload = Reload;
        }

        private void Reload(GraphicsResourceBase graphicsResourceBase)
        {
            forceDataChanged = true;
            
            // Force recreation
            graphicsResourceBase.OnRecreate();
        }

        public void Update(GraphicsDevice graphicsDevice, EffectParameterCollectionGroup parameterCollectionGroup)
        {
            var threadIndex = graphicsDevice.ThreadIndex;
            bool dataChanged = constantBufferDatas[threadIndex].Update(parameterCollectionGroup);

            // Check if update is really needed
            if (forceDataChanged)
                forceDataChanged = false;
            else if (!dataChanged)
                return;

            // Upload data to constant buffer
            Buffer.SetData(graphicsDevice, dataStreams[threadIndex]);
        }
    }
}