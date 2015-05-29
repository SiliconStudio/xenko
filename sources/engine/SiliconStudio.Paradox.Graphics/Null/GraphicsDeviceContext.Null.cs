// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 
using System;

namespace SiliconStudio.Paradox.Graphics
{
    public partial class GraphicsDevice
    {
        protected GraphicsDevice()
        {
        }

        /// <summary>
        /// Gets or sets the 1st viewport.
        /// </summary>
        /// <value>The viewport.</value>
        public void SetViewport(Viewport viewport)
        {
            throw new NotImplementedException();
        }

        /// <summary>	
        /// <p>Set the blend state of the output-merger stage.</p>	
        /// </summary>	
        /// <param name="blendState"><dd>  <p>Pointer to a blend-state interface (see <strong><see cref="SharpDX.Direct3D11.BlendState"/></strong>). Passing in <strong><c>null</c></strong> implies a default blend state. See remarks for further details.</p> </dd></param>	
        /// <remarks>	
        /// <p>Blend state is used by the output-merger stage to determine how to blend together two pixel values. The two values are commonly the current pixel value and the pixel value already in the output render target. Use the <strong>blend operation</strong> to control where the two pixel values come from and how they are mathematically combined.</p><p>To create a blend-state interface, call <strong><see cref="SharpDX.Direct3D11.Device.CreateBlendState"/></strong>.</p><p>Passing in <strong><c>null</c></strong> for the blend-state interface indicates to the runtime to set a default blending state.  The following table indicates the default blending parameters.</p><table> <tr><th>State</th><th>Default Value</th></tr> <tr><td>AlphaToCoverageEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong></td></tr> <tr><td>BlendEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong>[8]</td></tr> <tr><td>SrcBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOp</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>SrcBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOpAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>RenderTargetWriteMask[8]</td><td><see cref="SharpDX.Direct3D11.ColorWriteMaskFlags.All"/>[8]</td></tr> </table><p>?</p><p>A sample mask determines which samples get updated in all the active render targets. The mapping of bits in a sample mask to samples in a multisample render target is the responsibility of an individual application. A sample mask is always applied; it is independent of whether multisampling is enabled, and does not depend on whether an application uses multisample render targets.</p><p> The method will hold a reference to the interfaces passed in. This differs from the device state behavior in Direct3D 10. </p>	
        /// </remarks>	
        public void SetBlendState(BlendState blendState)
        {
            throw new NotImplementedException();
        }

        /// <summary>	
        /// <p>Set the blend state of the output-merger stage.</p>	
        /// </summary>	
        /// <param name="blendState"><dd>  <p>Pointer to a blend-state interface (see <strong><see cref="SharpDX.Direct3D11.BlendState"/></strong>). Passing in <strong><c>null</c></strong> implies a default blend state. See remarks for further details.</p> </dd></param>
        /// <param name="blendFactor"><dd>  <p>Array of blend factors, one for each RGBA component. This requires a blend state object that specifies the <strong><see cref="SharpDX.Direct3D11.BlendOption.BlendFactor"/></strong> option.</p> </dd></param>	
        /// <param name="multiSampleMask"><dd>  <p>32-bit sample coverage. The default value is 0xffffffff. See remarks.</p> </dd></param>	
        /// <remarks>	
        /// <p>Blend state is used by the output-merger stage to determine how to blend together two pixel values. The two values are commonly the current pixel value and the pixel value already in the output render target. Use the <strong>blend operation</strong> to control where the two pixel values come from and how they are mathematically combined.</p><p>To create a blend-state interface, call <strong><see cref="SharpDX.Direct3D11.Device.CreateBlendState"/></strong>.</p><p>Passing in <strong><c>null</c></strong> for the blend-state interface indicates to the runtime to set a default blending state.  The following table indicates the default blending parameters.</p><table> <tr><th>State</th><th>Default Value</th></tr> <tr><td>AlphaToCoverageEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong></td></tr> <tr><td>BlendEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong>[8]</td></tr> <tr><td>SrcBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOp</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>SrcBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOpAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>RenderTargetWriteMask[8]</td><td><see cref="SharpDX.Direct3D11.ColorWriteMaskFlags.All"/>[8]</td></tr> </table><p>?</p><p>A sample mask determines which samples get updated in all the active render targets. The mapping of bits in a sample mask to samples in a multisample render target is the responsibility of an individual application. A sample mask is always applied; it is independent of whether multisampling is enabled, and does not depend on whether an application uses multisample render targets.</p><p> The method will hold a reference to the interfaces passed in. This differs from the device state behavior in Direct3D 10. </p>	
        /// </remarks>	
        public void SetBlendState(BlendState blendState, Mathematics.Color blendFactor, int multiSampleMask = -1)
        {
            throw new NotImplementedException();
        }

        /// <summary>	
        /// <p>Set the blend state of the output-merger stage.</p>	
        /// </summary>	
        /// <param name="blendState"><dd>  <p>Pointer to a blend-state interface (see <strong><see cref="SharpDX.Direct3D11.BlendState"/></strong>). Passing in <strong><c>null</c></strong> implies a default blend state. See remarks for further details.</p> </dd></param>
        /// <param name="blendFactor"><dd>  <p>Array of blend factors, one for each RGBA component. This requires a blend state object that specifies the <strong><see cref="SharpDX.Direct3D11.BlendOption.BlendFactor"/></strong> option.</p> </dd></param>	
        /// <param name="multiSampleMask"><dd>  <p>32-bit sample coverage. The default value is 0xffffffff. See remarks.</p> </dd></param>	
        /// <remarks>	
        /// <p>Blend state is used by the output-merger stage to determine how to blend together two pixel values. The two values are commonly the current pixel value and the pixel value already in the output render target. Use the <strong>blend operation</strong> to control where the two pixel values come from and how they are mathematically combined.</p><p>To create a blend-state interface, call <strong><see cref="SharpDX.Direct3D11.Device.CreateBlendState"/></strong>.</p><p>Passing in <strong><c>null</c></strong> for the blend-state interface indicates to the runtime to set a default blending state.  The following table indicates the default blending parameters.</p><table> <tr><th>State</th><th>Default Value</th></tr> <tr><td>AlphaToCoverageEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong></td></tr> <tr><td>BlendEnable</td><td><strong><see cref="SharpDX.Result.False"/></strong>[8]</td></tr> <tr><td>SrcBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlend</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOp</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>SrcBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.One"/></td></tr> <tr><td>DstBlendAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOption.Zero"/></td></tr> <tr><td>BlendOpAlpha</td><td><see cref="SharpDX.Direct3D11.BlendOperation.Add"/></td></tr> <tr><td>RenderTargetWriteMask[8]</td><td><see cref="SharpDX.Direct3D11.ColorWriteMaskFlags.All"/>[8]</td></tr> </table><p>?</p><p>A sample mask determines which samples get updated in all the active render targets. The mapping of bits in a sample mask to samples in a multisample render target is the responsibility of an individual application. A sample mask is always applied; it is independent of whether multisampling is enabled, and does not depend on whether an application uses multisample render targets.</p><p> The method will hold a reference to the interfaces passed in. This differs from the device state behavior in Direct3D 10. </p>	
        /// </remarks>	
        public void SetBlendState(BlendState blendState, Mathematics.Color blendFactor, uint multiSampleMask = 0xFFFFFFFF)
        {
            throw new NotImplementedException();
        }


        /// <summary>	
        /// Sets the depth-stencil state of the output-merger stage.
        /// </summary>	
        /// <param name="depthStencilState"><dd>  <p>Pointer to a depth-stencil state interface (see <strong><see cref="SharpDX.Direct3D11.DepthStencilState"/></strong>) to bind to the device. Set this to <strong><c>null</c></strong> to use the default state listed in <strong><see cref="SharpDX.Direct3D11.DepthStencilStateDescription"/></strong>.</p> </dd></param>	
        /// <param name="stencilReference"><dd>  <p>Reference value to perform against when doing a depth-stencil test. See remarks.</p> </dd></param>	
        /// <remarks>	
        /// <p>To create a depth-stencil state interface, call <strong><see cref="SharpDX.Direct3D11.Device.CreateDepthStencilState"/></strong>.</p><p> The method will hold a reference to the interfaces passed in. This differs from the device state behavior in Direct3D 10. </p>	
        /// </remarks>	
        public void SetDepthStencilState(DepthStencilState depthStencilState, int stencilReference = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>	
        /// <p>Set the <strong>rasterizer state</strong> for the rasterizer stage of the pipeline.</p>	
        /// </summary>	
        /// <param name="rasterizerState">The rasterizser state to set on this device.</param>	
        public void SetRasterizerState(RasterizerState rasterizerState)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Binds a single scissor rectangle to the rasterizer stage.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        /// <param name="right">The right.</param>
        /// <param name="bottom">The bottom.</param>
        /// <remarks>	
        /// <p>All scissor rects must be set atomically as one operation. Any scissor rects not defined by the call are disabled.</p><p>The scissor rectangles will only be used if ScissorEnable is set to true in the rasterizer state (see <strong><see cref="SharpDX.Direct3D11.RasterizerStateDescription"/></strong>).</p><p>Which scissor rectangle to use is determined by the SV_ViewportArrayIndex semantic output by a geometry shader (see shader semantic syntax). If a geometry shader does not make use of the SV_ViewportArrayIndex semantic then Direct3D will use the first scissor rectangle in the array.</p><p>Each scissor rectangle in the array corresponds to a viewport in an array of viewports (see <strong><see cref="SharpDX.Direct3D11.RasterizerStage.SetViewports"/></strong>).</p>	
        /// </remarks>	
        public void SetScissorRectangles(int left, int top, int right, int bottom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///   Binds a set of scissor rectangles to the rasterizer stage.
        /// </summary>
        /// <param name = "scissorRectangles">The set of scissor rectangles to bind.</param>
        /// <remarks>	
        /// <p>All scissor rects must be set atomically as one operation. Any scissor rects not defined by the call are disabled.</p><p>The scissor rectangles will only be used if ScissorEnable is set to true in the rasterizer state (see <strong><see cref="SharpDX.Direct3D11.RasterizerStateDescription"/></strong>).</p><p>Which scissor rectangle to use is determined by the SV_ViewportArrayIndex semantic output by a geometry shader (see shader semantic syntax). If a geometry shader does not make use of the SV_ViewportArrayIndex semantic then Direct3D will use the first scissor rectangle in the array.</p><p>Each scissor rectangle in the array corresponds to a viewport in an array of viewports (see <strong><see cref="SharpDX.Direct3D11.RasterizerStage.SetViewports"/></strong>).</p>	
        /// </remarks>	
        public unsafe void SetScissorRectangles(params Mathematics.Rectangle[] scissorRectangles)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Clears the state.
        /// </summary>
        public void ClearState()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Marks context as active on the current thread.
        /// </summary>
        public void Begin()
        {
        }

        /// <summary>
        /// Unmarks context as active on the current thread.
        /// </summary>
        public void End()
        {
        }

        /// <summary>
        /// Clears a depth stencil buffer.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="options">Options for clearing a buffer.</param>
        /// <param name="depth">Set this depth value for the Depth buffer.</param>
        /// <param name="stencil">Set this stencil value for the Stencil buffer.</param>
        public void Clear(DepthStencilBuffer depthStencilBuffer, DepthStencilClearOptions options, float depth = 1, byte stencil = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a render target.
        /// </summary>
        /// <param name="renderTarget">The render target.</param>
        /// <param name="color">Set this color value for the RenderTarget buffer.</param>
        public void Clear(RenderTarget renderTarget, Mathematics.Color color)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a structured buffer
        /// </summary>
        /// <param name="buffer">The structured buffer.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Buffer buffer, float value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a structured buffer
        /// </summary>
        /// <param name="buffer">The structured buffer.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Buffer buffer, uint value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a structured buffer
        /// </summary>
        /// <param name="buffer">The structured buffer.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Buffer buffer, int value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a texture with unordered access.
        /// </summary>
        /// <param name="texture">The texture with unordered access.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Texture texture, float value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a texture with unordered access.
        /// </summary>
        /// <param name="texture">The texture with unordered access.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Texture texture, uint value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears a texture with unordered access.
        /// </summary>
        /// <param name="texture">The texture with unordered access.</param>
        /// <param name="value">Set this value for the whole buffer.</param>
        public void Clear(Texture texture, int value)
        {
            throw new NotImplementedException();
        }

        public void EnableProfile(bool enabledFlag)
        {
            throw new NotImplementedException();
        }

        public void BeginProfile(Framework.Mathematics.Color profileColor, string name)
        {
            throw new NotImplementedException();
        }

        public void EndProfile()
        {
            throw new NotImplementedException();
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox)
        {
            throw new NotImplementedException();
        }

        internal void UpdateSubresource(GraphicsResource resource, int subResourceIndex, DataBox databox, ResourceRegion region)
        {
            throw new NotImplementedException();
        }

        public DataBox MapSubresource(GraphicsResource resource, int subResourceIndex, MapMode mapMode)
        {
            throw new NotImplementedException();
        }

        public void UnmapSubresource(GraphicsResource resource, int subResourceIndex)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute a compute shader from a thread group.
        /// </summary>
        /// <param name="threadCountX">The number of groups dispatched in the x direction.</param>
        /// <param name="threadCountY">The number of groups dispatched in the y direction.</param>
        /// <param name="threadCountZ">The number of groups dispatched in the w direction.</param>
        public void Dispatch(int threadCountX, int threadCountY, int threadCountZ)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute a compute shader from a thread group using arguments stored in the <see cref="IIndirectBuffer"/>.
        /// </summary>
        /// <param name="indirectBuffer">Buffer storing arguments of the standard dispatch method (threadCountX/Y/Z).</param>
        /// <param name="offsetInBytes">Offset to start to read the arguments from the indirect buffer.</param>
        public void Dispatch(Buffer indirectBuffer, int offsetInBytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies the current count of structured elements from a structured buffer to an <see cref="IIndirectBuffer"/>.
        /// </summary>
        /// <param name="sourceBuffer">The structured buffer</param>
        /// <param name="destBuffer">The indirect buffer</param>
        /// <param name="offsetToDest">Offset into the indirect buffer to write arguments to</param>
        public void CopyCount(Buffer sourceBuffer, Buffer destBuffer, int offsetToDest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders a sequence of non-indexed geometric primitives of the specified type from the current set of data input streams.
        /// </summary>
        /// <param name="primitiveType">Describes the type of primitive to render.</param>
        /// <param name="vertexCount">Number of vertex to render.</param>
        /// <param name="startVertex">Index of the first vertex to load. Beginning at startVertex, the correct number of vertices is read out of the vertex buffer.</param>
        public void Draw(PrimitiveType primitiveType, int vertexCount, int startVertex = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders a sequence of indexed geometric primitives from the current set of data input streams.
        /// </summary>
        /// <param name="primitiveType">Describes the type of primitive to render.</param>
        /// <param name="indexCount">Number of index to render.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="startVertex">Index of the first vertex to load. Beginning at startVertex, the correct number of vertices is read out of the vertex buffer.</param>
        public void DrawIndexed(PrimitiveType primitiveType, int indexCount, int startIndex = 0, int startVertex = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies a graphics resource to a destination resource.
        /// </summary>
        /// <param name="source">The source resource.</param>
        /// <param name="destination">The destination resource.</param>
        public void Copy(GraphicsResource source, GraphicsResource destination)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unsets the render targets.
        /// </summary>
        public void UnsetRenderTargets()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Executes the command list.
        /// </summary>
        /// <param name="commandList">The command list.</param>
        public void ExecuteCommandList(ICommandList commandList)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finishes the command list.
        /// </summary>
        /// <returns></returns>
        public ICommandList FinishCommandList()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets multiple stream output targets to this GraphicsDevice.
        /// </summary>
        /// <param name="buffers">The stream output buffers.</param>
        public void SetStreamTargets(params Buffer[] buffers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a new depth stencil buffer and multiple render targets to this GraphicsDevice.
        /// </summary>
        /// <param name="depthStencilBuffer">The depth stencil buffer.</param>
        /// <param name="renderTargets">The render targets.</param>
        public void SetDepthAndRenderTargets(DepthStencilBuffer depthStencilBuffer,
                                              params RenderTarget[] renderTargets)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a vertex array object.
        /// </summary>
        /// <param name="vertexArrayObject">The vertex array object.</param>
        public void SetVertexArrayObject(VertexArrayObject vertexArrayObject)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a constant buffer to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="buffer">The constant buffer to set.</param>
        public void SetConstantBuffer(ShaderStage stage, int slot, Buffer buffer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a sampler state to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="samplerState">The sampler state to set.</param>
        public void SetSamplerState(ShaderStage stage, int slot, SamplerState samplerState)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets a shader resource view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The shader stage.</param>
        /// <param name="slot">The binding slot.</param>
        /// <param name="shaderResourceView">The shader resource view.</param>
        public void SetShaderResourceView(ShaderStage stage, int slot, GraphicsResource shaderResourceView)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets an unordered access view to the shader pipeline.
        /// </summary>
        /// <param name="stage">The stage.</param>
        /// <param name="slot">The slot.</param>
        /// <param name="unorderedAccessView">The unordered access view.</param>
        /// <exception cref="System.ArgumentException">Invalid stage.;stage</exception>
        public void SetUnorderedAccessView(ShaderStage stage, int slot, GraphicsResource unorderedAccessView)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Unsets the read/write buffers.
        /// </summary>
        public void UnsetReadWriteBuffers()
        {
            throw new NotImplementedException();
        }
    }
} 
#endif
