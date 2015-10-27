using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine.Network;
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.RemoteShaderCompiler
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
