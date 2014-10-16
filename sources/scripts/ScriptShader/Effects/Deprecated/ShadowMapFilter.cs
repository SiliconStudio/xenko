using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
{
    public abstract class ShadowMapFilter
    {
        private ShadowMap shadowMap;

        protected ShadowMapFilter(ShadowMap shadowMap)
        {
            this.shadowMap = shadowMap;
        }

        public ShadowMap ShadowMap
        {
            get { return shadowMap; }
            internal set { shadowMap = value; }
        }

        public abstract ShaderClassSource GenerateShaderSource(int shadowMapCount);
    }
}