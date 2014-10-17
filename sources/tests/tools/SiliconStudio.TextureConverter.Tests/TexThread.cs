// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using SiliconStudio.TextureConverter;

namespace TextureToolsTest
{
    class TexThread : IDisposable
    {
        private string[] fileList;
        private int num;
        private TextureTool texTool;

        public TexThread(string[] fileList, int num)
        {
            this.fileList = fileList;
            this.texTool = new TextureTool();
            this.num = num;
        }

        public void Dispose()
        {
            texTool.Dispose();
        }

        public void Process()
        {
            TexImage image;

            foreach(string filePath in fileList)
            {
                Console.WriteLine("\n Thread # " + num + " ---------------------------------------- PROCESSING " + filePath);

                image = texTool.Load(filePath);

                texTool.Rescale(image, 0.5f, 0.5f, Filter.Rescaling.Bicubic);

                if (image.MipmapCount <= 1)
                {
                    texTool.GenerateMipMaps(image, Filter.MipMapGeneration.Cubic);
                }

                string outFile = Path.GetDirectoryName(filePath) + "\\out\\" + Path.GetFileName(filePath);
                outFile = Path.ChangeExtension(outFile, ".dds");

                texTool.Save(image, outFile, SiliconStudio.Paradox.Graphics.PixelFormat.BC3_UNorm);

                image.Dispose();
            }
        }
    }
}
