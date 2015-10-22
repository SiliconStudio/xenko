using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering
{
    public class ShadowMapFilterDefault : ShadowMapFilter
    {
        public ShadowMapFilterDefault(ShadowMap shadowMap)
            : base(shadowMap)
        {
        }

        public override ShaderClassSource GenerateShaderSource(int shadowMapCount)
        {
            return new ShaderClassSource("ShadowMapFilterDefault");
        }
    }
}