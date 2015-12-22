using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A material feature
    /// </summary>
    [DataContract(Inherited = true)]
    [NonIdentifiable]
    public abstract class MaterialFeature : IMaterialFeature
    {
        [DataMember(-20)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        protected MaterialFeature()
        {
            Enabled = true;
        }

        public void Visit(MaterialGeneratorContext context)
        {
            if (!Enabled)
                return;

            VisitFeature(context);
        }

        /// <summary>
        /// Generates the for the feature shader.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void VisitFeature(MaterialGeneratorContext context);
    }
}