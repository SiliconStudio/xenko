// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine.Network;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.RemoteShaderCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var shaderCompilerServer = new ShaderCompilerServer();
            shaderCompilerServer.Listen(13335);
        }
    }
}
