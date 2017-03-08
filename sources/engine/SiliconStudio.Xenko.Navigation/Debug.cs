using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Navigation
{
    public static class Debug
    {
        // TODO: Remove this
        // DEBUG FUNCTIONS
        public static void DumpObj(string filePath, Vector3[] meshData, int[] indexData = null)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Length; i++)
                {
                    Vector3 vert = meshData[i];
                    sw.WriteLine("v {0} {1} {2}", vert.X, vert.Y, vert.Z);
                }

                if (indexData == null)
                {
                    int numFaces = meshData.Length / 3;
                    for (int i = 0; i < numFaces; i++)
                    {
                        int start = 1 + i * 3;
                        sw.WriteLine("f {0} {1} {2}",
                            start + 0,
                            start + 1,
                            start + 2);
                    }
                }
                else
                {
                    int numFaces = indexData.Length / 3;
                    for (int i = 0; i < numFaces; i++)
                    {
                        sw.WriteLine("f {0} {1} {2}",
                            indexData[i * 3] + 1,
                            indexData[i * 3 + 1] + 1,
                            indexData[i * 3 + 2] + 1);
                    }
                }
                sw.Flush();
                file.Flush();
            }
        }

        public static void DumpObj(string filePath, GeometricMeshData<VertexPositionNormalTexture> meshData)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(file))
            {
                for (int i = 0; i < meshData.Vertices.Length; i++)
                {
                    VertexPositionNormalTexture vert = meshData.Vertices[i];
                    sw.WriteLine("v {0} {1} {2}", vert.Position.X, vert.Position.Y, vert.Position.Z);
                }

                int numFaces = meshData.Indices.Length / 3;
                for (int i = 0; i < numFaces; i++)
                {
                    sw.WriteLine("f {0} {1} {2}",
                        meshData.Indices[i * 3 + 0] + 1,
                        meshData.Indices[i * 3 + 1] + 1,
                        meshData.Indices[i * 3 + 2] + 1);
                }
                sw.Flush();
                file.Flush();
            }
        }

        public static void DumpBinary(string filePath, byte[] data)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                return;

            using (FileStream file = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                file.Write(data, 0, data.Length);
            }
        }
    }
}
