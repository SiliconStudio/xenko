using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.VertexLayouts
{
    public abstract class ParticleVertexLayout
    {
        protected IntPtr vertexBuffer = IntPtr.Zero;
        protected int vertexOffset = 0;

        public abstract VertexDeclaration GetVertexDeclaration();

        public abstract int Size { get; protected set; }

        public abstract int VerticesPerParticle { get; internal set; } // Will depend on the builder

        public void StartBuffer(IntPtr vtxBuff)
        {
            vertexBuffer = vtxBuff;
            vertexOffset = 0;
        }

        public void EndBuffer()
        {
            vertexBuffer = IntPtr.Zero;
            vertexOffset = 0;
        }

        public void NextVertex()
        {
            vertexBuffer += Size;
        }

        public void NextParticle()
        {
            vertexBuffer += Size * VerticesPerParticle;
        }

        public virtual void SetPosition(ref Vector3 position) { }

        public virtual void SetUvCoords(ref Vector2 uvCoords) { }

        public virtual void SetColor(ref Color4 color) { }

        public virtual void SetPosition(IntPtr ptr) { }

        public virtual void SetUvCoords(IntPtr ptr) { }

        public virtual void SetColor(IntPtr ptr) { }
    }
}
