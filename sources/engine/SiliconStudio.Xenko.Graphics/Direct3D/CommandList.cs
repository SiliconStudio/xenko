// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_GRAPHICS_API_DIRECT3D 
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Graphics
{
    internal class CommandList : ComponentBase, ICommandList
    {
        public SharpDX.Direct3D11.CommandList NativeCommandList { get; private set; }

        public CommandList(SharpDX.Direct3D11.CommandList commandList)
        {
            NativeCommandList = commandList;
            commandList.DisposeBy(this);
        }
    }
}
 
#endif 
