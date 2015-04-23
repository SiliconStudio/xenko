using SiliconStudio.Paradox.DataModel;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Rendering
{
    public class DirectionalLight : Light
    {
        public DirectionalLight()
        {
            Parameters.SetDefault(LightKeys.LightDirection);
        }

        /// <summary>
        /// Gets or sets the light direction.
        /// </summary>
        /// <value>
        /// The light direction.
        /// </value>
        public Vector3 LightDirection
        {
            get { return Parameters.Get(LightKeys.LightDirection); }
            set { Parameters.Set(LightKeys.LightDirection, value); }
        }
    }
}