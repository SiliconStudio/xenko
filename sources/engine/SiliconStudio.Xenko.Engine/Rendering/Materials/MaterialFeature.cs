// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// A material feature
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class MaterialFeature : IMaterialFeature
    {
        [DataMember(-20)]
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        protected MaterialFeature()
        {
        }

        public void Visit(MaterialGeneratorContext context)
        {
            if (!Enabled)
                return;

            switch (context.Step)
            {
                case MaterialGeneratorStep.PassesEvaluation:
                    MultipassGeneration(context);
                    break;
                case MaterialGeneratorStep.GenerateShader:
                    GenerateShader(context);
                    break;
            }
        }

        /// <summary>
        /// Called during prepass, used to enumerate extra passes.
        /// </summary>
        /// <param name="context">The context.</param>
        public virtual void MultipassGeneration(MaterialGeneratorContext context)
        {
        }

        /// <summary>
        /// Generates the shader for the feature.
        /// </summary>
        /// <param name="context">The context.</param>
        public abstract void GenerateShader(MaterialGeneratorContext context);
    }
}
