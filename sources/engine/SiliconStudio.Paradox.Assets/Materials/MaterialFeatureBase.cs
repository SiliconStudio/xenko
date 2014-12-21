// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using System.Reflection;

using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// Base class for <see cref="IMaterialFeature"/>.
    /// </summary>
    /// <remarks>
    /// This base class automatically iterates on properties to generate the shader
    /// </remarks>
    public abstract class MaterialFeatureBase : IMaterialFeature
    {
        public virtual void GenerateShader(MaterialShaderGeneratorContext context)
        {
            GenerateShader(this, context);
        }

        /// <summary>
        /// Automatically introspect a material feature and look for all members (field or properties) containing a <see cref="IMaterialFeature"/>
        /// or a <see cref="IMaterialComputeColor"/> attribute.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="context">The context.</param>
        /// <exception cref="System.ArgumentNullException">
        /// instance
        /// or
        /// context
        /// </exception>
        private static void GenerateShader(IMaterialFeature instance, MaterialShaderGeneratorContext context)
        {
            if (instance == null) throw new ArgumentNullException("instance");
            if (context == null) throw new ArgumentNullException("context");

            var typeDescriptor = TypeDescriptorFactory.Default.Find(instance.GetType());

            // Grab a visitor if there are any defined on the feature.
            var instanceVisitor = instance as IMaterialFeatureVisitor;

            foreach (var member in typeDescriptor.Members.OfType<MemberDescriptorBase>())
            {
                var memberValue = member.Get(instance);

                // TODO: Should we log an error if a property/field is not supported?
                // TODO: Handle list/collection of IMaterialFeature?

                var feature = memberValue as IMaterialFeature;
                if (feature != null)
                {
                    if (instanceVisitor != null)
                    {
                        instanceVisitor.Visit(instance, member, feature);
                    }

                    feature.GenerateShader(context);
                }
                else
                {
                    var materialStreamAttribute = member.MemberInfo.GetCustomAttribute<MaterialStreamAttribute>();
                    if (materialStreamAttribute != null)
                    {
                        if (string.IsNullOrWhiteSpace(materialStreamAttribute.Stream))
                        {
                            context.Log.Error("Material stream cannot be null for member [{0}.{1}]", member.DeclaringType, member.MemberInfo.Name);
                            continue;
                        }

                        var computeColor = memberValue as IMaterialComputeColor;
                        if (computeColor != null)
                        {
                            var key = ParameterKeys.TryFindByName(materialStreamAttribute.ParameterKey);

                            if (instanceVisitor != null)
                            {
                                instanceVisitor.Visit(instance, member, computeColor, materialStreamAttribute);
                            }

                            var classSource = computeColor.GenerateShaderSource(context, key);
                            context.CurrentStack.SetStream(materialStreamAttribute.Stream, materialStreamAttribute.Type, classSource);
                        }
                        else
                        {
                            context.Log.Error("Error in [{0}.{1}] support only IMaterialNode instead of [{2}]", member.DeclaringType, member.MemberInfo.Name, member.Type);
                        }
                    }
                }
            }
        }
    }
}