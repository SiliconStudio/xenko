using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects
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