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
