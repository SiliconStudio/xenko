using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Images;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    public class DefaultRenderTargets : IColorTarget, INormalTarget, IVelocityTarget, IMultipleRenderViews
    {
        private readonly FastList<Texture> renderTargets = new FastList<Texture>();
        private Texture color;
        private Texture normal;
        private Texture velocity;
        private bool dirty = true;

        public Texture Color
        {
            get { return color; }
            set { color = value; dirty = true; }
        }

        public Texture Normal
        {
            get { return normal; }
            set { normal = value; dirty = true; }
        }

        public Texture Velocity
        {
            get { return velocity; }
            set { velocity = value; dirty = true; }
        }

        private void RefreshTargets()
        {
            if (!dirty)
                return;

            dirty = false;

            renderTargets.Clear();

            //color
            renderTargets.Add(Color);

            //normals
            if (Normal != null)
                renderTargets.Add(Normal);

            //velocity
            if (Velocity != null)
                renderTargets.Add(Velocity);
        }

        public Texture[] RenderTargets
        {
            get
            {
                RefreshTargets();
                return renderTargets.Items;
            }
        }

        public int RenderTargetCount
        {
            get
            {
                RefreshTargets();
                return renderTargets.Count;
            }
        }

        public int Count { get; set; }

        public int Index { get; set; }
    }
}