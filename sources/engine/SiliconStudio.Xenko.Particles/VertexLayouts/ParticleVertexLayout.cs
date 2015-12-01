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

        public abstract VertexDeclaration GetVertexDeclaration();

        public abstract int Size { get; protected set; }

        public abstract int VerticesPerParticle { get; internal set; } // Will depend on the builder

        public void StartBuffer(IntPtr vtxBuff)
        {
            vertexBuffer = vtxBuff;
        }

        public void EndBuffer()
        {
            vertexBuffer = IntPtr.Zero;
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

        /// <summary>
        /// Sets the same position for all vertices created from the same particle.
        /// Assumes offset is at the first vertex of the particle.
        /// </summary>
        /// <param name="position">Position vector to assign</param>
        public virtual void SetPositionForParticle(ref Vector3 position) { }

        public virtual void SetUvCoordsForParticle(ref Vector2 uvCoords) { }

        public virtual void SetColorForParticle(ref Color4 color) { }

        public virtual void SetPositionForParticle(IntPtr ptr) { }

        public virtual void SetUvCoordsForParticle(IntPtr ptr) { }

        public virtual void SetColorForParticle(IntPtr ptr) { }

    }
}
